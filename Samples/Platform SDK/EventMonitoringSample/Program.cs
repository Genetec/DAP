// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
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

        engine.SetEventFilter(new[]
        {
            EventType.UnitConnected,
            EventType.UnitDisconnected,
            EventType.InterfaceOnline,
            EventType.InterfaceOffline,
            EventType.EntityWarning
        });

        engine.EventReceived += (sender, e) =>
        {
            Entity entity = engine.GetEntity(e.SourceGuid);

            switch (e.EventType)
            {
                case EventType.UnitConnected:
                    Console.WriteLine($"{entity.Name} connected.");
                    break;
                case EventType.UnitDisconnected:
                    Console.WriteLine($"{entity.Name} disconnected.");
                    break;
                case EventType.InterfaceOnline:
                    Console.WriteLine($"{entity.Name} interface online.");
                    break;
                case EventType.InterfaceOffline:
                    Console.WriteLine($"{entity.Name} interface offline.");
                    break;
                case EventType.EntityWarning:
                    Console.WriteLine($"{entity.Name} warning.");
                    break;
            }     
        };

        ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

        if (state == ConnectionStateCode.Success)
        {
            await LoadEntities();

            Console.WriteLine($"Listening to events: {string.Join(",", engine.GetEventFilter())}");
        }
        else
        {
            Console.WriteLine($"Logon failed: {state}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();

        async Task LoadEntities()
        {
            var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
            query.EntityTypeFilter.Add(EntityType.Unit);
            await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
        }
    }
}