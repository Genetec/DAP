// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Genetec.Sdk.Entities;
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

            ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

            if (state == ConnectionStateCode.Success)
            {
                await LoadAreas();

                var areas = engine.GetEntities(EntityType.Area).OfType<Area>();
                foreach (var area in areas)
                {
                    Console.WriteLine($"{area.Name}: {area.PeopleCount.Count}");
                }
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            async Task LoadAreas()
            {
                Console.WriteLine("Loading areas...");

                var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
                query.EntityTypeFilter.Add(EntityType.Area);
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
