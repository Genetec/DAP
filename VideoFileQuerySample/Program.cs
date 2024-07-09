// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries;
using Genetec.Sdk.Queries.Video;

const string server = "localhost";
const string username = "admin";
const string password = "";

SdkResolver.Initialize();

using var engine = new Engine();

ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

if (state == ConnectionStateCode.Success)
{
    // Load cameras into the entity cache
    await LoadCameras();

    // Retrieve cameras from the entity cache
    List<Camera> cameras = engine.GetEntities(EntityType.Camera).OfType<Camera>().ToList();
    Console.WriteLine($"{cameras.Count} cameras loaded");

    // Retrieve video file information for all cameras
    IEnumerable<VideoFileInfo> videoFileInfos = await GetVideoFileInfos(cameras);

    // Display video file information
    foreach (VideoFileInfo videoFileInfo in videoFileInfos)
    {
        DisplayToConsole(videoFileInfo);
    }
}
else
{
    Console.WriteLine($"Logon failed: {state}");
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

Task LoadCameras()
{
    Console.WriteLine("Loading cameras...");

    var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
    query.EntityTypeFilter.Add(EntityType.Camera);

    return Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
}

async Task<IEnumerable<VideoFileInfo>> GetVideoFileInfos(IEnumerable<Camera> cameras)
{
    Console.WriteLine("Retrieving video file information...");

    var query = (VideoFileQuery)engine.ReportManager.CreateReportQuery(ReportType.VideoFile);
    query.Cameras.AddRange(cameras.Select(camera => camera.Guid));

    QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

    return args.Data.AsEnumerable().Select(CreateFromDataRow).ToList();

    VideoFileInfo CreateFromDataRow(DataRow row) => new()
    {
        CameraGuid = row.Field<Guid>(VideoFileQuery.CameraGuidColumnName),
        ArchiveSourceGuid = row.Field<Guid>(VideoFileQuery.ArchiveSourceGuidColumnName),
        StartTime = row.Field<DateTime>(VideoFileQuery.StartTimeColumnName),
        EndTime = row.Field<DateTime>(VideoFileQuery.EndTimeColumnName),
        FilePath = row.Field<string>(VideoFileQuery.FilePathColumnName),
        FileSize = row.Field<decimal>(VideoFileQuery.FileSizeColumnName),
        MetadataPath = row.Field<string>(VideoFileQuery.MetadataPathColumnName),
        ProtectionStatus = (VideoProtectionState)row.Field<uint>(VideoFileQuery.ProtectionStatusColumnName),
        InfiniteProtection = row.Field<bool>(VideoFileQuery.InfiniteProtectionColumnName),
        Drive = row.Field<string>(VideoFileQuery.DriveColumnName),
        Error = row.Field<uint>(VideoFileQuery.ErrorColumnName),
        ProtectionEndDateTime = row.Field<DateTime>(VideoFileQuery.ProtectionEndDateTimeColumnName)
    };
}

void DisplayToConsole(VideoFileInfo info)
{
    Console.WriteLine("Video File Information:");
    Console.WriteLine($"  {"Camera:",-22} {engine.GetEntity(info.CameraGuid).Name}");
    Console.WriteLine($"  {"Archive Source:",-22} {engine.GetEntity(info.ArchiveSourceGuid).Name}");
    Console.WriteLine($"  {"Start Time:",-22} {info.StartTime.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine($"  {"End Time:",-22} {info.EndTime.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine($"  {"File Path:",-22} {info.FilePath}");
    Console.WriteLine($"  {"File Size:",-22} {info.FileSize:N0} bytes");
    Console.WriteLine($"  {"Metadata Path:",-22} {info.MetadataPath}");
    Console.WriteLine($"  {"Protection Status:",-22} {info.ProtectionStatus}");
    Console.WriteLine($"  {"Infinite Protection:",-22} {info.InfiniteProtection}");
    Console.WriteLine($"  {"Drive:",-22} {info.Drive}");
    Console.WriteLine($"  {"Error:",-22} {info.Error}");
    Console.WriteLine($"  {"Protection End Date:",-22} {info.ProtectionEndDateTime.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine(new string('-', 60));
}

class VideoFileInfo
{
    public Guid CameraGuid { get; set; }
    public Guid ArchiveSourceGuid { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string FilePath { get; set; }
    public decimal FileSize { get; set; }
    public string MetadataPath { get; set; }
    public VideoProtectionState ProtectionStatus { get; set; }
    public bool InfiniteProtection { get; set; }
    public string Drive { get; set; }
    public uint Error { get; set; }
    public DateTime ProtectionEndDateTime { get; set; }
}