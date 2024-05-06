// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Sdk;
    using Sdk.Entities;
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

            engine.ObserveConnected().SelectMany(async connected =>
                {
                    if (!connected)
                        return Observable.Empty<RtspMediaRouterRole>();

                    await LoadEntities();

                    return engine.GetEntities(EntityType.Role).OfType<RtspMediaRouterRole>()
                        .ToObservable()
                        .SelectMany(role => engine.ObserveRoleCurrentServer(role).Select(_ => role));
                })
                .Switch()
                .Subscribe(OnMediaGatewayCurrentServerChanged);

            var state = await engine.LogOnAsync(server, username, password);

            if (state != ConnectionStateCode.Success) Console.WriteLine($"Logon failed: {state}");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            void OnMediaGatewayCurrentServerChanged(RtspMediaRouterRole role)
            {
                if (engine.GetEntity(role.CurrentServer) is Server server)
                {
                    Console.WriteLine($"{role.Name} is now running on {server.Name}");
                }
                else
                {
                    Console.WriteLine($"{role.Name} is has no server assigned");
                }
            }

            async Task LoadEntities()
            {
                var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
                query.EntityTypeFilter.Add(EntityType.Role);
                query.EntityTypeFilter.Add(EntityType.Server);
                query.DownloadAllRelatedData = true;
                await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
            }

        }
    }
}