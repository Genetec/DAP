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
using System.Net;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Entities.Video;
using Genetec.Sdk.Queries;
using Genetec.Sdk.Workflows;
using Genetec.Sdk.Workflows.UnitManager;

SdkResolver.Initialize();

await RunSample();

async Task RunSample()
{
    const string server = "localhost";
    const string username = "admin";
    const string password = "";

    using var engine = new Engine();

    ConnectionStateCode state = await engine.LogOnAsync(server: server, username: username, password: password);

    if (state == ConnectionStateCode.Success)
    {
        PrintSupportedCameras(engine.VideoUnitManager);

        VideoUnitProductInfo productInfo = engine.VideoUnitManager.FindProductsByManufacturer("Generic Stream").FirstOrDefault(p => p.ProductType == "RTSP");

        if (productInfo != null)
        {
            Console.WriteLine("Enrolling video unit from Generic RTSP stream...");

            var uri = new Uri("rtsp://127.0.0.1:554/mystream/live"); // TODO: Replace with your RTSP stream URI

            // Create the AddVideoUnitInfo object without duplicating IP/port
            AddVideoUnitInfo addVideoUnitInfo = new(videoUnitProductInfo: productInfo,
                ipEndPoint: new IPEndPoint(IPAddress.Parse(uri.Host), uri.Port),
                useDefaultCredentials: true);

            addVideoUnitInfo.SpecialFeatures.Add("StreamUri", uri.OriginalString);

            ArchiverRole archiver = await GetArchiverRole(engine);
            if (archiver != null)
            {
                try
                {
                    Progress<EnrollmentResult> progress = new(result => Console.WriteLine(result));
                    Guid videoUnitId = await AddVideoUnit(engine.VideoUnitManager, addVideoUnitInfo, archiver.Guid, progress);
                    var videoUnit = (VideoUnit)engine.GetEntity(videoUnitId);
                    Console.WriteLine($"Video unit: {videoUnit} has been created.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                Console.WriteLine("No Archiver role was found. An Archiver role must be created to enroll a camera.");
            }
        }
    }
    else
    {
        Console.WriteLine($"Logon failed: {state}");
    }

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}

async Task<ArchiverRole> GetArchiverRole(Engine engine)
{
    var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
    query.EntityTypeFilter.Add(EntityType.Role);
    var result = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
    return result.Data.AsEnumerable().Select(row => engine.GetEntity(row.Field<Guid>(nameof(Guid)))).OfType<ArchiverRole>().FirstOrDefault();
}

void PrintSupportedCameras(IVideoUnitManager manager)
{
    Console.WriteLine("Listing supported manufacturers and models");

    foreach (var grouping in ListSupportedCameras(manager).GroupBy(productInfo => productInfo.Manufacturer).OrderBy(grouping => grouping.Key))
    {
        Console.WriteLine($"Manufacturer: {grouping.Key}");
        Console.WriteLine(new string('-', 50));

        foreach (var productInfo in grouping)
        {
            Console.WriteLine($"  Product Type: {productInfo.ProductType}");
            Console.WriteLine($"  Description: {productInfo.Description}");
            Console.WriteLine("\n  Capabilities:");
            PrintProductCapability(productInfo.ProductCapability);
            Console.WriteLine();
        }

        Console.WriteLine();
    }

    void PrintProductCapability(ProductCapability capability)
    {
        if (capability == null)
            return;

        const string indent = "    ";  // 4 spaces for alignment

        Console.WriteLine($"{indent}Username: {capability.IsUsernameSupported,-6} Password: {capability.IsPasswordSupported}");

        Console.WriteLine($"{indent}Command Port:     {capability.IsCommandPortSupported,-6} (Default: {capability.DefaultCommandPort})");
        Console.WriteLine($"{indent}Secure Port:      {capability.IsSecureCommandPortSupported,-6} (Default: {capability.DefaultSecureCommandPort})");
        Console.WriteLine($"{indent}Discovery Port:   {capability.IsDiscoveryPortSupported,-6} (Default: {capability.DefaultDiscoveryPort})");

        Console.WriteLine($"{indent}IPv4: {capability.IsIPv4Supported,-6} IPv6: {capability.IsIPv6Supported,-6} Hostname: {capability.IsHostnameSupported}");
        Console.WriteLine($"{indent}Manual Add: {capability.IsManualAddSupported}");

        if (capability.SpecialFeatures.Any())
        {
            Console.WriteLine($"\n{indent}Special Features:");
            foreach (string feature in capability.SpecialFeatures)
            {
                Console.WriteLine($"{indent}- {feature}");
            }
        }
    }

    IEnumerable<VideoUnitProductInfo> ListSupportedCameras(IVideoUnitManager videoUnitManager) => videoUnitManager.Manufacturers.SelectMany(videoUnitManager.FindProductsByManufacturer);
}

async Task<Guid> AddVideoUnit(IVideoUnitManager videoUnitManager, AddVideoUnitInfo videoUnitInfo, Guid archiver, IProgress<EnrollmentResult> progress = default)
{
    var completion = new TaskCompletionSource<Guid>();

    videoUnitManager.EnrollmentStatusChanged += OnEnrollmentStatusChanged;
    try
    {
        AddUnitResponse response = await videoUnitManager.AddVideoUnit(videoUnitInfo, archiver);

        return response.Error != Error.None ? throw new Exception($"Fail to add video unit {response.Error}") : await completion.Task;
    }
    finally
    {
        videoUnitManager.EnrollmentStatusChanged -= OnEnrollmentStatusChanged;
    }

    void OnEnrollmentStatusChanged(object sender, Genetec.Sdk.EventsArgs.UnitEnrolledEventArgs e)
    {
        progress?.Report(e.EnrollmentResult);

        if (e.EnrollmentResult != EnrollmentResult.Connecting)
        {
            if (e.Unit != Guid.Empty)
            {
                completion.SetResult(e.Unit);
            }
            else
            {
                completion.SetException(new Exception($"Unable to enroll the video unit: {e.EnrollmentResult}"));
            }
        }
    }
}