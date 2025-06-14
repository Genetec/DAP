// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

using Genetec.Sdk;
using System;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk.Queries;
using System.Threading.Tasks;
using System.Data;
using System.Collections.Generic;
using Genetec.Sdk.Entities;
using System.Linq;

const string server = "localhost";
const string username = "admin";
const string password = "";

SdkResolver.Initialize();

await RunSample();

Console.Write("Press any key to exit...");
Console.ReadKey(true);

async Task RunSample()
{
    using var engine = new Engine();

    ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

    if (state != ConnectionStateCode.Success)
    {
        Console.WriteLine($"Login failed: {state}");
        return;
    }

    // Load cameras into the entity cache
    await LoadCameras();

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

    async Task LoadCameras()
    {
        Console.WriteLine("Loading cameras...");

        var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
        query.EntityTypeFilter.Add(EntityType.Camera);
        await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
    }

    async Task<IList<CameraConfiguration>> GetCameraConfigurations(IEnumerable<Camera> cameras)
    {
        Console.WriteLine("Retrieving camera configurations...");

        var query = (CameraConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.CameraConfiguration);
        query.Cameras.AddRange(cameras.Select(camera => camera.Guid));

        QueryCompletedEventArgs queryResult = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

        return queryResult.Data.AsEnumerable().Select(CreateFromDataRow).ToList();

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

class CameraConfiguration
{
    public Guid Guid { get; set; }
    public string Manufacturer { get; set; }
    public string Model { get; set; }
    public string StreamUsage { get; set; }
    public string Schedule { get; set; }
    public string VideoFormat { get; set; }
    public string Resolution { get; set; }
    public string RecordingMode { get; set; }
    public string NetworkSetting { get; set; }
    public string MulticastAddress { get; set; }
    public string Port { get; set; }
    public string Trickling { get; set; }
    public int BitRate { get; set; }
    public int ImageQuality { get; set; }
    public int KeyFrameInterval { get; set; }
    public int KeyFrameIntervalUnits { get; set; }
    public int FrameRate { get; set; }
    public string UnitFirmwareVersion { get; set; }
    public string UnitIpAddress { get; set; }
    public int RetentionPeriod { get; set; }
}