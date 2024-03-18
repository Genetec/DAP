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

            var engine = new Engine();

            await engine.LogOnAsync(server, username, password);

            PrintEntityCache();

            await LoadEntities(EntityType.AccessPoint, EntityType.AccessRule);

            PrintEntityCache();

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
