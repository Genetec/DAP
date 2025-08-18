// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk.Entities;
using Genetec.Sdk;
using Genetec.Sdk.Queries;

namespace Genetec.Dap.CodeSamples;

public class PeopleCountingSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        // Load areas into the entity cache
        await LoadAreas(engine);

        // Retrieve areas from the entity cache
        IEnumerable<Area> areas = engine.GetEntities(EntityType.Area).OfType<Area>();

        foreach (var area in areas)
        {
            DisplayToConsole(area, engine);
        }
    }

    private async Task LoadAreas(Engine engine)
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

        } while (args.Data.Rows.Count > query.PageSize);
    }

    private void DisplayToConsole(Area area, Engine engine)
    {
        Console.WriteLine($"\n{area.Name}");
        Console.WriteLine($"People Count: {area.PeopleCount.Count}");

        if (area.PeopleCount.Any())
        {
            // Display information for up to 10 cardholders in this area
            Console.WriteLine("\nShowing up to 10 cardholders:");

            foreach (PeopleCountRecord record in area.PeopleCount.Take(10))
            {
                Entity cardholder = engine.GetEntity(record.Cardholder);
                Console.WriteLine($"  Name: {cardholder.Name}");
                Console.WriteLine($"  Location: {record.Status}");
                Console.WriteLine($"  Last Access: {record.LastAccess}");
                Console.WriteLine();
            }
        }
    }
}