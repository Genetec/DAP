// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

using System;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Queries;

SdkResolver.Initialize();

await RunSample();

async Task RunSample()
{
    object consoleLock = new object(); // Lock object to synchronize console output

    const string server = "localhost";
    const string username = "admin";
    const string password = "";

    using var engine = new Engine();

    engine.EntitiesAdded += OnEntitiesAdded;
    engine.EntitiesInvalidated += OnEntitiesInvalidated;
    engine.EntitiesRemoved += OnEntitiesRemoved;

    lock (consoleLock)
    {
        Console.WriteLine("Entity cache before user logon");
        PrintEntityCache(engine);
    }

    ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

    if (state == ConnectionStateCode.Success)
    {
        lock (consoleLock)
        {
            Console.WriteLine("\nEntity cache before loading entities");
            PrintEntityCache(engine);
        }

        // Load entities of the specified types into the entity cache
        await LoadEntities(engine, EntityType.Area, EntityType.Door, EntityType.AccessPoint, EntityType.AccessRule, EntityType.Schedule);

        lock (consoleLock)
        {
            Console.WriteLine("\nEntity cache after loading entities");
            PrintEntityCache(engine);
        }
    }
    else
    {
        Console.WriteLine($"Logon failed: {state}");
    }

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();

    void OnEntitiesAdded(object sender, EntitiesAddedEventArgs e)
    {
        lock (consoleLock)
        {
            foreach (EntityUpdateInfo info in e.Entities)
            {
                Console.WriteLine($"Entity has been added: {engine.GetEntity(info.EntityGuid)}");
            }
        }
    }

    void OnEntitiesInvalidated(object sender, EntitiesInvalidatedEventArgs e)
    {
        lock (consoleLock)
        {
            foreach (EntityUpdateInfo info in e.Entities)
            {
                Console.WriteLine($"Entity has been modified: {engine.GetEntity(info.EntityGuid)}");
            }
        }
    }

    void OnEntitiesRemoved(object sender, EntitiesRemovedEventArgs e)
    {
        lock (consoleLock)
        {
            foreach (EntityUpdateInfo info in e.Entities)
            {
                Console.WriteLine($"Entity has been deleted: {info.EntityType} {info.EntityGuid}");
            }
        }
    }
}

void PrintEntityCache(Engine engine)
{
    Console.WriteLine("\n===== Entity Cache Summary =====");
    Console.WriteLine($"{"Entity Type",-25} | {"Count",10}");
    Console.WriteLine(new string('-', 38));

    int totalEntities = 0;

    foreach (var entityType in Enum.GetValues(typeof(EntityType)).OfType<EntityType>().Except(new[] { EntityType.None, EntityType.ReportTemplate }).OrderBy(type => type.ToString()))
    {
        int count = engine.GetEntities(entityType).Count;
        totalEntities += count;

        if (count > 0)
        {
            Console.WriteLine($"{entityType,-25} | {count,10:N0}");
        }
    }

    Console.WriteLine(new string('-', 38));
    Console.WriteLine($"Total entities: {totalEntities:N0}");
    Console.WriteLine("================================\n");
}

async Task LoadEntities(Engine engine, params EntityType[] types)
{
    Console.WriteLine("Loading entities into the entity cache...\n");

    var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
    query.EntityTypeFilter.AddRange(types);
    query.DownloadAllRelatedData = true;
    query.Page = 1;
    query.PageSize = 1000;

    QueryCompletedEventArgs args;
    do
    {
        args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
        query.Page++;

    } while (args.Data.Rows.Count >= query.PageSize);
}