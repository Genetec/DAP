// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Sdk;
using Sdk.Entities;
using Sdk.Entities.Video;
using Sdk.Queries;
using Sdk.Workflows;
using Sdk.Workflows.UnitManager;

class Program
{
    static Program() => SdkResolver.Initialize();

    static async Task Main()
    {
        const string server = "localhost";
        const string username = "admin";
        const string password = "";

        using var engine = new Engine();

        ConnectionStateCode state = await engine.LogOnAsync(server: server, username: username, password: password);

        if (state == ConnectionStateCode.Success)
        {
            PrintSupportedCameras(engine.VideoUnitManager);

            VideoUnitProductInfo productInfo = engine.VideoUnitManager.FindProductsByManufacturer("Genetec").First(p => p.ProductType == "All");

            var endpoint = new IPEndPoint(address: IPAddress.Parse("127.0.0.1"), port: 16000);

            var info = new AddVideoUnitInfo(videoUnitProductInfo: productInfo, endpoint, useDefaultCredentials: true);

            ArchiverRole role = await GetArchiverRole(engine);
            if (role != null)
            {
                try
                {
                    Guid videoUnitId = await AddVideoUnit(engine.VideoUnitManager, info, role.Guid);
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
        else
        {
            Console.WriteLine($"Logon failed: {state}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static async Task<ArchiverRole> GetArchiverRole(Engine engine)
    {
        var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
        query.EntityTypeFilter.Add(EntityType.Role);
        QueryCompletedEventArgs result = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
        return result.Data.AsEnumerable().Select(row => engine.GetEntity(row.Field<Guid>(nameof(Guid)))).OfType<ArchiverRole>().FirstOrDefault();
    }

    static void PrintSupportedCameras(IVideoUnitManager manager)
    {
        foreach (IGrouping<string, VideoUnitProductInfo> grouping in ListSupportedCameras(manager).GroupBy(productInfo => productInfo.Manufacturer))
        {
            Console.WriteLine($"Manufacturer: {grouping.Key}");
            Console.WriteLine(new string('-', 20));

            foreach (VideoUnitProductInfo productInfo in grouping)
            {
                Console.WriteLine($"  - Product Type: {productInfo.ProductType}");
                Console.WriteLine($"    Description: {productInfo.Description}");
                Console.WriteLine();
            }

            Console.WriteLine();
        }
    }

    static IEnumerable<VideoUnitProductInfo> ListSupportedCameras(IVideoUnitManager videoUnitManager)
    {
        return videoUnitManager.Manufacturers.SelectMany(videoUnitManager.FindProductsByManufacturer);
    }

    static async Task<Guid> AddVideoUnit(IVideoUnitManager videoUnitManager, AddVideoUnitInfo videoUnitInfo, Guid archiver)
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

        void OnEnrollmentStatusChanged(object sender, Sdk.EventsArgs.UnitEnrolledEventArgs e)
        {
            if (e.EnrollmentResult == EnrollmentResult.Added)
            {
                completion.SetResult(e.Unit);
            }
        }
    }
}