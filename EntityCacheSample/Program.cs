// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Sdk;
    using Sdk.Queries;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            const string server = "localhost";
            const string username = "admin";
            const string password = "";

            using var engine = new Engine();

            engine.EntitiesAdded += (sender, e) =>
            {
                foreach (var info in e.Entities)
                {
                    Console.WriteLine($"Entity has been added: {engine.GetEntity(info.EntityGuid)}");
                }
            };

            engine.EntitiesInvalidated += (sender, e) =>
            {
                foreach (var info in e.Entities)
                {
                    Console.WriteLine($"Entity has been modified: {engine.GetEntity(info.EntityGuid)}");
                }
            };

            engine.EntitiesRemoved += (sender, e) =>
            {
                foreach (var info in e.Entities)
                {
                    Console.WriteLine($"Entity has been deleted: {info.EntityType} {info.EntityGuid}");
                }
            };

            ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

            if (state == ConnectionStateCode.Success)
            {
                PrintEntityCache();

                await LoadEntities(EntityType.AccessPoint, EntityType.AccessRule);

                PrintEntityCache();
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }
            
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            void PrintEntityCache()
            {
                Console.WriteLine("Entity cache");

                foreach (var entityType in Enum.GetValues(typeof(EntityType)).OfType<EntityType>())
                {
                    try
                    {
                        var count = engine.GetEntities(entityType).Count;
                        if (count > 0)
                        {
                            Console.WriteLine($"{entityType,-20}: {count}");
                        }
                    }
                    catch
                    {
                    }
                }
            }

            async Task LoadEntities(params EntityType[] types)
            {
                Console.WriteLine("Loading entities...");

                var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
                query.EntityTypeFilter.AddRange(types);
                query.DownloadAllRelatedData = false;
                query.Page = 1;
                query.PageSize = 1000;

                QueryCompletedEventArgs args;

                do
                {
                    args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
                    query.Page++;

                } while (args.Error == ReportError.TooManyResults || args.Data.Rows.Count > query.PageSize);
            }
        }
    }
}
