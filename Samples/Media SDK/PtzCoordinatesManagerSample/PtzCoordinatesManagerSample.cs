// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Media.Ptz;

namespace Genetec.Dap.CodeSamples;

public class PtzCoordinatesManagerSample : SampleBase
{
    private const int ObservationMilliseconds = 2000;

    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        // Load all cameras into the entity cache.
        await LoadEntities(engine, token, EntityType.Camera);

        Camera camera = engine.GetEntities(EntityType.Camera).OfType<Camera>().FirstOrDefault(camera => camera.IsPtz && camera.IsOnline);
        if (camera is null)
        {
            Console.WriteLine("No suitable PTZ camera found. Ensure a PTZ camera is online.");
            return;
        }

        await LoadEntities(engine, token, EntityType.Network);

        // This sample assumes the first network represents the application's workstation.
        // Select the appropriate client network for your deployment.
        Network network = engine.GetEntities(EntityType.Network).OfType<Network>().FirstOrDefault();
        if (network is null)
        {
            Console.WriteLine("No network found. Ensure at least one network is configured.");
            return;
        }

        using var manager = new PtzCoordinatesManager();
        manager.PtzStarted += (sender, e) => Console.WriteLine($"PTZ started. Coordinates: {e.Coordinates}");
        manager.PtzStopped += (sender, e) => Console.WriteLine($"PTZ stopped. Coordinates: {e.Coordinates}");
        manager.CoordinatesReceived += (sender, e) => Console.WriteLine($"Coordinates received. Coordinates: {e.Coordinates}");
        manager.UnhandledException += (sender, e) =>
        {
            Console.Error.WriteLine($"PTZ exception for camera {camera.Guid}: {e.ExceptionObject}");
            ReportErrorDetails(manager);
        };

        Console.WriteLine($"Initializing PTZ control for camera '{camera.Name}' using network '{network.Name}'.");
        manager.Initialize(sdkEngine: engine, cameraGuid: camera.Guid, clientSubnet: network.Guid);

        await GoToPresets(engine, manager, camera, token);
        await RunPatterns(engine, manager, camera, token);
        await RunPresetTours(engine, manager, camera, token);
        await DemonstratePtzControls(engine, manager, camera, token);
    }

    private static bool CanControlPtz(Engine engine, Camera camera, PtzCommandType command,
        SdkPrivilege privilege, PtzCommandType? stopCommand = null)
    {
        if (!camera.PtzCapabilities.IsSupportedCommand(command))
        {
            Console.WriteLine($"Skipping {command}: camera {camera.Guid} does not support it.");
            return false;
        }

        if (stopCommand.HasValue && !camera.PtzCapabilities.IsSupportedCommand(stopCommand.Value))
        {
            Console.WriteLine($"Skipping {command}: camera {camera.Guid} does not support {stopCommand.Value}.");
            return false;
        }

        if (!engine.SecurityManager.IsPrivilegeGranted(privilege, camera.Guid))
        {
            Console.WriteLine($"Skipping {command}: privilege check returned false for user {engine.LoggedUser.Guid}, camera {camera.Guid}, required privilege {privilege}.");
            return false;
        }

        return true;
    }

    private static void ReportErrorDetails(PtzCoordinatesManager manager)
    {
        var error = manager.ErrorDetails;
        if (error != null)
            Console.Error.WriteLine($"Current PTZ error state: {error.Code}. {error.Details}");
    }

    // Requests presets in the camera's supported ID range.
    private async Task GoToPresets(Engine engine, PtzCoordinatesManager manager, Camera camera, CancellationToken token)
    {
        int presetCount = camera.PtzCapabilities.NumberOfPresets;
        if (presetCount <= 0)
        {
            Console.WriteLine($"Camera '{camera.Name}' reports no supported presets.");
            return;
        }

        foreach (int preset in Enumerable.Range(camera.PtzCapabilities.PresetBase, presetCount))
        {
            token.ThrowIfCancellationRequested();
            if (!CanControlPtz(engine, camera, PtzCommandType.GoToPreset, SdkPrivilege.UsePtzPresets))
                return;

            token.ThrowIfCancellationRequested();
            manager.ControlPtz(PtzCommandType.GoToPreset, preset, 0);
            Console.WriteLine($"Requested preset {preset}: {camera.GetPtzPresetName(preset) ?? $"Preset {preset}"}");

            await Task.Delay(ObservationMilliseconds, token);
            ReportErrorDetails(manager);
        }
    }

    // Requests patterns in the camera's supported ID range.
    private async Task RunPatterns(Engine engine, PtzCoordinatesManager manager, Camera camera, CancellationToken token)
    {
        int patternCount = camera.PtzCapabilities.NumberOfPatterns;
        if (patternCount <= 0)
        {
            Console.WriteLine($"Camera '{camera.Name}' reports no supported patterns.");
            return;
        }

        foreach (int pattern in Enumerable.Range(camera.PtzCapabilities.PatternBase, patternCount))
        {
            token.ThrowIfCancellationRequested();
            if (!CanControlPtz(engine, camera, PtzCommandType.RunPattern, SdkPrivilege.UsePtzPatterns))
                return;

            token.ThrowIfCancellationRequested();
            manager.ControlPtz(PtzCommandType.RunPattern, pattern, 0);
            Console.WriteLine($"Requested pattern {pattern}: {camera.GetPtzPatternName(pattern) ?? $"Pattern {pattern}"}");

            await Task.Delay(ObservationMilliseconds, token); // Pause before requesting the next pattern.
            ReportErrorDetails(manager);
        }
    }

    // Starts and stops preset tours in the camera's supported ID range.
    private async Task RunPresetTours(Engine engine, PtzCoordinatesManager manager, Camera camera, CancellationToken token)
    {
        int presetTourCount = camera.PtzCapabilities.NumberOfPresetTours;
        if (presetTourCount <= 0)
        {
            Console.WriteLine($"Camera '{camera.Name}' reports no supported preset tours.");
            return;
        }

        foreach (int tour in Enumerable.Range(camera.PtzCapabilities.PresetTourBase, presetTourCount))
        {
            token.ThrowIfCancellationRequested();
            if (!CanControlPtz(engine, camera, PtzCommandType.StartPresetTour, SdkPrivilege.UsePtzPatterns,
                PtzCommandType.StopPresetTour))
                return;

            token.ThrowIfCancellationRequested();
            try
            {
                manager.ControlPtz(PtzCommandType.StartPresetTour, tour, 0);
                Console.WriteLine($"Requested StartPresetTour for tour {tour}.");
                await Task.Delay(ObservationMilliseconds, token);
            }
            finally
            {
                manager.ControlPtz(PtzCommandType.StopPresetTour, tour, 0);
                Console.WriteLine($"Requested StopPresetTour for tour {tour}.");
                // Allow the stop request to be processed before disposing the manager.
                await Task.Delay(ObservationMilliseconds);
                ReportErrorDetails(manager);
            }
        }
    }

    // Demonstrates basic PTZ controls (pan, tilt, zoom, iris, focus).
    private async Task DemonstratePtzControls(Engine engine, PtzCoordinatesManager manager, Camera camera, CancellationToken token)
    {
        // Pan right and tilt up. Both speeds use the range -100 to 100.
        await RunMovement(PtzCommandType.StartPanTilt, PtzCommandType.StopPanTilt,
            SdkPrivilege.BasicPtzOperations, arg1: 50, arg2: 50);

        // Zoom in (0) at speed 50. Use 1 to zoom out.
        await RunMovement(PtzCommandType.StartZoom, PtzCommandType.StopZoom,
            SdkPrivilege.BasicPtzOperations, arg1: 0, arg2: 50);

        // Open the iris (0) at speed 50. Use 1 to close it.
        await RunMovement(PtzCommandType.StartIris, PtzCommandType.StopIris,
            SdkPrivilege.ChangeFocusAndIris, arg1: 0, arg2: 50);

        // Focus near (0) at speed 50. Use 1 for far focus.
        await RunMovement(PtzCommandType.StartFocus, PtzCommandType.StopFocus,
            SdkPrivilege.ChangeFocusAndIris, arg1: 0, arg2: 50);

        async Task RunMovement(PtzCommandType startCommand, PtzCommandType stopCommand,
            SdkPrivilege privilege, int arg1, int arg2)
        {
            token.ThrowIfCancellationRequested();
            if (!CanControlPtz(engine, camera, startCommand, privilege, stopCommand))
                return;

            token.ThrowIfCancellationRequested();
            try
            {
                manager.ControlPtz(startCommand, arg1, arg2);
                Console.WriteLine($"Requested {startCommand} with arguments {arg1}, {arg2}.");
                await Task.Delay(ObservationMilliseconds, token);
            }
            finally
            {
                manager.ControlPtz(stopCommand, 0, 0);
                Console.WriteLine($"Requested {stopCommand}.");
                await Task.Delay(ObservationMilliseconds);
                ReportErrorDetails(manager);
            }
        }
    }
}
