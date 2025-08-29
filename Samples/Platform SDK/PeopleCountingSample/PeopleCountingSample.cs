// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk.Entities;
using Genetec.Sdk;

namespace Genetec.Dap.CodeSamples;

public class PeopleCountingSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        // Load areas into the entity cache
        await LoadEntities(engine, token, EntityType.Area);

        // Retrieve areas from the entity cache
        IEnumerable<Area> areas = engine.GetEntities(EntityType.Area).OfType<Area>();

        foreach (var area in areas)
        {
            DisplayToConsole(area, engine);
        }
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