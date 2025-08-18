// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Media.Ptz;
using Genetec.Sdk.Queries;

namespace Genetec.Dap.CodeSamples;

class PtzCoordinatesManagerSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        // Find a suitable PTZ camera
        Camera camera = await FindPtzCamera(engine);
        if (camera is null)
        {
            Console.WriteLine("No suitable PTZ camera found. Ensure a PTZ camera is online.\n");
            return;
        }

        // Load networks into the entity cache
        await LoadNetworks(engine);

        // Get the network (client subnet) used by the camera.
        // In this sample, we assume the first network is the one we want to use.
        Network network = engine.GetEntities(EntityType.Network).OfType<Network>().FirstOrDefault();
        if (network is null)
        {
            Console.WriteLine("No network found. Ensure at least one network is configured.\n");
            return;
        }

        using var manager = new PtzCoordinatesManager();

        manager.PtzStarted += (sender, e) => Console.WriteLine($"PTZ started. Coordinates: {e.Coordinates}\n");
        manager.PtzStopped += (sender, e) => Console.WriteLine($"PTZ stopped. Coordinates: {e.Coordinates}\n");
        manager.CoordinatesReceived += (sender, e) => Console.WriteLine($"Coordinates received. Coordinates: {e.Coordinates}\n");

        Console.WriteLine($"Initializing PTZ control for camera '{camera.Name}' using network '{network.Name}'.\n");
        manager.Initialize(sdkEngine: engine, cameraGuid: camera.Guid, clientSubnet: network.Guid);

        await GoToPresets(manager, camera, token);
        await RunPatterns(manager, camera, token);
        await RunPresetTours(manager, camera, token);
        await DemonstratePtzControls(manager, camera, token);
    }

    // Finds the first available online PTZ camera
    async Task<Camera> FindPtzCamera(Engine engine)
    {
        Console.WriteLine("Searching for an online PTZ camera...\n");

        var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
        query.EntityTypeFilter.Add(EntityType.Camera);

        for (int page = 1; ; page++)
        {
            query.Page = page;
            query.PageSize = 100;

            var result = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

            var camera = result.Data.AsEnumerable().Select(row => engine.GetEntity(row.Field<Guid>(nameof(Guid)))).OfType<Camera>().FirstOrDefault(c => c.IsPtz && c.IsOnline);

            if (camera is not null)
                return camera;

            if (result.Data.Rows.Count <= query.PageSize)
                break;
        }

        return null;
    }

    // Loads networks into the entity cache
    async Task LoadNetworks(Engine engine)
    {
        Console.WriteLine("Loading networks...\n");

        var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
        query.EntityTypeFilter.Add(EntityType.Network);

        await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
    }

    // Demonstrates moving through all available presets
    async Task GoToPresets(PtzCoordinatesManager manager, Camera camera, CancellationToken token)
    {
        if (!camera.PtzCapabilities.IsSupportedCommand(PtzCommandType.GoToPreset))
        {
            Console.WriteLine("The camera does not support the GoToPreset command.\n");
            return;
        }

        List<int> presets = Enumerable.Range(camera.PtzCapabilities.PresetBase, camera.PtzCapabilities.NumberOfPresets).ToList();
        if (!presets.Any())
        {
            Console.WriteLine($"No presets found for camera '{camera.Name}'.\n");
            return;
        }

        Console.WriteLine($"Moving through {presets.Count} presets:\n");
        foreach (int preset in presets)
        {
            Console.WriteLine($"Moving to {camera.GetPtzPresetName(preset) ?? $"Preset {preset}"}... (Preset number: {preset})");

            manager.ControlPtz(PtzCommandType.GoToPreset, preset, 0);

            await Task.Delay(2000, token); // Wait for 2 seconds before moving to the next preset
        }

        Console.WriteLine("Completed moving through all presets.\n");
    }

    // Demonstrates running all available patterns
    async Task RunPatterns(PtzCoordinatesManager manager, Camera camera, CancellationToken token)
    {
        if (!camera.PtzCapabilities.IsSupportedCommand(PtzCommandType.RunPattern))
        {
            Console.WriteLine("The camera does not support the RunPattern command.\n");
            return;
        }

        List<int> patterns = Enumerable.Range(camera.PtzCapabilities.PatternBase, camera.PtzCapabilities.NumberOfPatterns).ToList();
        if (!patterns.Any())
        {
            Console.WriteLine($"No patterns found for camera '{camera.Name}'.\n");
            return;
        }

        Console.WriteLine($"Running {patterns.Count} patterns:\n");
        foreach (int pattern in patterns)
        {
            Console.WriteLine($"Running {camera.GetPtzPatternName(pattern) ?? $"Pattern {pattern}"}... (Pattern number: {pattern})");

            manager.ControlPtz(PtzCommandType.RunPattern, pattern, 0);

            await Task.Delay(2000, token); // Wait for 2 seconds before stopping the pattern
        }

        Console.WriteLine("Completed running all patterns.\n");
    }

    // Demonstrates running all available preset tours
    async Task RunPresetTours(PtzCoordinatesManager manager, Camera camera, CancellationToken token)
    {
        if (!camera.PtzCapabilities.IsSupportedCommand(PtzCommandType.RunTour))
        {
            Console.WriteLine("The camera does not support the RunTour command.\n");
            return;
        }

        List<int> presetTours = Enumerable.Range(camera.PtzCapabilities.PresetTourBase, camera.PtzCapabilities.NumberOfPresetTours).ToList();
        if (!presetTours.Any())
        {
            Console.WriteLine($"No preset tours found for camera '{camera.Name}'.\n");
            return;
        }

        Console.WriteLine($"Running {presetTours.Count} preset tours:\n");
        foreach (int tour in presetTours)
        {
            Console.WriteLine($"Running preset tour {tour}");

            manager.ControlPtz(PtzCommandType.RunTour, tour, 0);

            await Task.Delay(2000, token); // Wait for 2 seconds before stopping the preset tour
        }

        Console.WriteLine("Completed running all preset tours.\n");
    }

    // Demonstrates basic PTZ controls (pan, tilt, zoom, iris, focus)
    async Task DemonstratePtzControls(PtzCoordinatesManager manager, Camera camera, CancellationToken token)
    {
        if (camera.PtzCapabilities.IsSupportedCommand(PtzCommandType.StartPanTilt))
        {
            Console.WriteLine("Executing StartPanTilt (pan right, tilt up).");

            // arg1: Horizontal speed (-100 to 100, negative is left, positive is right)
            // arg2: Vertical speed (-100 to 100, negative is down, positive is up)
            manager.ControlPtz(commandType: PtzCommandType.StartPanTilt, arg1: 50, arg2: 50);

            await Task.Delay(2000, token); // Wait for 2 seconds before stopping pan/tilt

            manager.ControlPtz(commandType: PtzCommandType.StopPanTilt, 0, 0);

            Console.WriteLine("Executed StopPanTilt.");
        }

        if (camera.PtzCapabilities.IsSupportedCommand(PtzCommandType.StartZoom))
        {
            Console.WriteLine("Executing StartZoom (zoom in).");

            // arg1: Zoom direction (0 for zoom out, 1 for zoom in)
            // arg2: Zoom speed (0 to 100)
            manager.ControlPtz(commandType: PtzCommandType.StartZoom, 1, 50);

            await Task.Delay(2000, token); // Wait for 2 seconds before stopping zoom

            manager.ControlPtz(commandType: PtzCommandType.StopZoom, 0, 0);

            Console.WriteLine("Executed StopZoom.");
        }

        if (camera.PtzCapabilities.IsSupportedCommand(PtzCommandType.StartIris))
        {
            Console.WriteLine("Executing StartIris (open iris).");

            // arg1: Iris direction (0 for close iris, 1 for open iris)
            // arg2: Iris adjustment speed (0 to 100)
            manager.ControlPtz(commandType: PtzCommandType.StartIris, 1, 50);

            await Task.Delay(2000, token); // Wait for 2 seconds before stopping iris

            manager.ControlPtz(commandType: PtzCommandType.StopIris, 0, 0);

            Console.WriteLine("Executed StopIris.");
        }

        if (camera.PtzCapabilities.IsSupportedCommand(PtzCommandType.StartFocus))
        {
            Console.WriteLine("Executing StartFocus (focus near).");

            // arg1: Focus direction (0 for focus far, 1 for focus near)
            // arg2: Focus adjustment speed (0 to 100)
            manager.ControlPtz(commandType: PtzCommandType.StartFocus, arg1: 1, arg2: 50);

            await Task.Delay(2000, token); // Wait for 2 seconds before stopping focus

            manager.ControlPtz(commandType: PtzCommandType.StopFocus, arg1: 0, arg2: 0);

            Console.WriteLine("Executed StopFocus.");
        }
    }
}