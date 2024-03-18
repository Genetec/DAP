// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
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

    internal class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            const string server = "localhost";
            const string username = "admin";
            const string password = "";

            using (var engine = new Engine())
            {
                await engine.LogOnAsync(server: server, username: username, password: password);

                PrintSupportedCameras(engine.VideoUnitManager);

                VideoUnitProductInfo productInfo = engine.VideoUnitManager.FindProductsByManufacturer("Genetec").First(p => p.ProductType == "All");

                var endpoint = new IPEndPoint(address: IPAddress.Parse("127.0.0.1"), port: 16000);

                AddVideoUnitInfo info = new AddVideoUnitInfo(videoUnitProductInfo: productInfo, endpoint, useDefaultCredentials: true);

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

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        static async Task<ArchiverRole> GetArchiverRole(Engine engine)
        {
            var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
            query.EntityTypeFilter.Add(EntityType.Role);
            var result = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
            return result.Data.AsEnumerable().Select(row => engine.GetEntity(row.Field<Guid>(nameof(Guid)))).OfType<ArchiverRole>().FirstOrDefault();
        }

        static void PrintSupportedCameras(IVideoUnitManager manager)
        {
            foreach (var grouping in ListSupportedCameras(manager).GroupBy(productInfo => productInfo.Manufacturer))
            {
                Console.WriteLine($"Manufacturer: {grouping.Key}");
                Console.WriteLine(new string('-', 20));

                foreach (var productInfo in grouping)
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
}
