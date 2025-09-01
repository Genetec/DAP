// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sdk;
using Sdk.Entities;
using Sdk.Queries;

public class CameraConfigurationSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        // Load cameras into the entity cache
        await LoadEntities(engine, token, EntityType.Camera);

        // Retrieve cameras from the entity cache
        List<Camera> cameras = engine.GetEntities(EntityType.Camera).OfType<Camera>().ToList();
        Console.WriteLine($"{cameras.Count} cameras loaded");

        // Retrieve camera configurations for all cameras
        IList<CameraConfiguration> configurations = await GetCameraConfigurations(cameras);

        // Display camera configurations
        foreach (CameraConfiguration configuration in configurations)
        {
            DisplayToConsole(configuration);
        }


        async Task<IList<CameraConfiguration>> GetCameraConfigurations(IEnumerable<Camera> cameras)
        {
            Console.WriteLine("Retrieving camera configurations...");

            var query = (CameraConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.CameraConfiguration);
            query.Cameras.AddRange(cameras.Select(camera => camera.Guid));

            QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

            return args.Data.AsEnumerable().Select(CreateFromDataRow).ToList();

            CameraConfiguration CreateFromDataRow(DataRow row) => new()
            {
                Guid = row.Field<Guid>("Guid"),
                Manufacturer = row.Field<string>("Manufacturer"),
                Model = row.Field<string>("Model"),
                StreamUsage = row.Field<string>("StreamUsage"),
                Schedule = row.Field<string>("Schedule"),
                VideoFormat = row.Field<string>("VideoFormat"),
                Resolution = row.Field<string>("Resolution"),
                RecordingMode = row.Field<string>("RecordingMode"),
                NetworkSetting = row.Field<string>("NetworkSetting"),
                MulticastAddress = row.Field<string>("MulticastAddress"),
                Port = row.Field<string>("Port"),
                Trickling = row.Field<string>("Trickling"),
                BitRate = row.Field<int>("BitRate"),
                ImageQuality = row.Field<int>("ImageQuality"),
                KeyFrameInterval = row.Field<int>("KeyFrameInterval"),
                KeyFrameIntervalUnits = row.Field<int>("KeyFrameIntervalUnits"),
                FrameRate = row.Field<int>("FrameRate"),
                UnitFirmwareVersion = row.Field<string>("UnitFirmwareVersion"),
                UnitIpAddress = row.Field<string>("UnitIpAddress"),
                RetentionPeriod = row.Field<int>("RetentionPeriod")
            };
        }

        void DisplayToConsole(CameraConfiguration config)
        {
            Console.WriteLine("Camera Configuration:");
            Console.WriteLine($"Camera:               {engine.GetEntity(config.Guid).Name}");
            Console.WriteLine($"Manufacturer:         {config.Manufacturer}");
            Console.WriteLine($"Model:                {config.Model}");
            Console.WriteLine($"Stream Usage:         {config.StreamUsage}");
            Console.WriteLine($"Schedule:             {config.Schedule}");
            Console.WriteLine($"Video Format:         {config.VideoFormat}");
            Console.WriteLine($"Resolution:           {config.Resolution}");
            Console.WriteLine($"Recording Mode:       {config.RecordingMode}");
            Console.WriteLine($"Network Setting:      {config.NetworkSetting}");
            Console.WriteLine($"Multicast Address:    {config.MulticastAddress}");
            Console.WriteLine($"Port:                 {config.Port}");
            Console.WriteLine($"Trickling:            {config.Trickling}");
            Console.WriteLine($"Bit Rate:             {config.BitRate} bps");
            Console.WriteLine($"Image Quality:        {config.ImageQuality}");
            Console.WriteLine($"Key Frame Interval:   {config.KeyFrameInterval} {(config.KeyFrameIntervalUnits == 0 ? "seconds" : "images")}");
            Console.WriteLine($"Frame Rate:           {config.FrameRate} fps");
            Console.WriteLine($"Unit Firmware:        {config.UnitFirmwareVersion}");
            Console.WriteLine($"Unit IP Address:      {config.UnitIpAddress}");
            Console.WriteLine($"Retention Period:     {config.RetentionPeriod} days");
            Console.WriteLine(new string('-', 50));
        }
    }
}