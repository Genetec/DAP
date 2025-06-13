// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk.Entities;
using Genetec.Sdk;
using Genetec.Sdk.Queries;

const string server = "localhost";
const string username = "admin";
const string password = "";

SdkResolver.Initialize();

using var engine = new Engine();

ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

if (state == ConnectionStateCode.Success)
{
    // Load areas into the entity cache
    await LoadAreas();

    // Retrieve areas from the entity cache
    IEnumerable<Area> areas = engine.GetEntities(EntityType.Area).OfType<Area>();

    foreach (var area in areas)
    {
        DisplayToConsole(area);
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

    } while (args.Data.Rows.Count > query.PageSize);
}

void DisplayToConsole(Area area)
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