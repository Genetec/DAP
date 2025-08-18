// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using Genetec.Sdk;
using Genetec.Sdk.Queries;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genetec.Dap.CodeSamples;

public class EntityCacheSample : SampleBase
{
    private readonly object m_consoleLock = new object(); // Lock object to synchronize console output

    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        engine.EntitiesAdded += OnEntitiesAdded;
        engine.EntitiesInvalidated += OnEntitiesInvalidated;
        engine.EntitiesRemoved += OnEntitiesRemoved;

        lock (m_consoleLock)
        {
            Console.WriteLine("Entity cache before user logon");
            PrintEntityCache(engine);
        }

        lock (m_consoleLock)
        {
            Console.WriteLine("\nEntity cache before loading entities");
            PrintEntityCache(engine);
        }

        // Load entities of the specified types into the entity cache
        await LoadEntities(engine, EntityType.Area, EntityType.Door, EntityType.AccessPoint, EntityType.AccessRule, EntityType.Schedule);

        lock (m_consoleLock)
        {
            Console.WriteLine("\nEntity cache after loading entities");
            PrintEntityCache(engine);
        }
    }

    private void OnEntitiesAdded(object sender, EntitiesAddedEventArgs e)
    {
        lock (m_consoleLock)
        {
            foreach (EntityUpdateInfo info in e.Entities)
            {
                var engine = (Engine)sender;
                Console.WriteLine($"Entity has been added: {engine.GetEntity(info.EntityGuid)}");
            }
        }
    }

    private void OnEntitiesInvalidated(object sender, EntitiesInvalidatedEventArgs e)
    {
        lock (m_consoleLock)
        {
            foreach (EntityUpdateInfo info in e.Entities)
            {
                var engine = (Engine)sender;
                Console.WriteLine($"Entity has been modified: {engine.GetEntity(info.EntityGuid)}");
            }
        }
    }

    private void OnEntitiesRemoved(object sender, EntitiesRemovedEventArgs e)
    {
        lock (m_consoleLock)
        {
            foreach (EntityUpdateInfo info in e.Entities)
            {
                Console.WriteLine($"Entity has been deleted: {info.EntityType} {info.EntityGuid}");
            }
        }
    }

    private void PrintEntityCache(Engine engine)
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

    private async Task LoadEntities(Engine engine, params EntityType[] types)
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
}