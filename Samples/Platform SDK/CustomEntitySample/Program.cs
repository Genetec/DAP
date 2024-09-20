// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Dap.CodeSamples.Properties;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries;

const string server = "localhost";
const string username = "admin";
const string password = "";
const string customEntityId = "8385D04C-F04A-4125-81E9-D1C66AFDE572"; // TODO: Replace this GUID with your own.

SdkResolver.Initialize();

await RunSample();

async Task RunSample()
{
    using var engine = new Engine();

    ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

    if (state == ConnectionStateCode.Success)
    {
        // Retrieve the system configuration entity
        var config = (SystemConfiguration)engine.GetEntity(SystemConfiguration.SystemConfigurationGuid);

        // Retrieve and display existing custom entity type descriptors
        IReadOnlyCollection<CustomEntityTypeDescriptor> descriptors = config.GetCustomEntityTypeDescriptors();
        DisplayCustomEntityTypeDescriptors(descriptors);

        // Create or update the custom entity type
        CreateOrUpdateCustomEntityType(config);

        // Create a new custom entity
        await CreateCustomEntity(engine);

        // Query and display all custom entities
        await DisplayCustomEntities(engine);
    }
    else
    {
        Console.WriteLine($"logon failed: {state}");
    }

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}

void DisplayCustomEntityTypeDescriptors(IEnumerable<CustomEntityTypeDescriptor> descriptors)
{
    Console.WriteLine("Custom Entity Type Descriptors:");

    if (!descriptors.Any())
    {
        Console.WriteLine("No custom entity types found.");
        return;
    }

    foreach (CustomEntityTypeDescriptor descriptor in descriptors)
    {
        Console.WriteLine($"ID: {descriptor.Id}");
        Console.WriteLine($"Name: {descriptor.Name}");
        Console.WriteLine();
    }
}

void CreateOrUpdateCustomEntityType(SystemConfiguration config)
{
    var id = new Guid(customEntityId);

    if (config.GetCustomActionTypeDescriptor(id) is not null)
    {
        Console.WriteLine($"Custom Entity Type with ID {customEntityId} already exists.");
        return;
    }

    Console.WriteLine($"Creating new Custom Entity Type with ID: {customEntityId}");

    var capabilities = CustomEntityTypeCapabilities.CanBeFederated | 
                       CustomEntityTypeCapabilities.IsVisible |
                       CustomEntityTypeCapabilities.CreateDelete | 
                       CustomEntityTypeCapabilities.MapSupport;

    var descriptor = new CustomEntityTypeDescriptor(id, Resources.CustomEntityName, capabilities, new Version(1, 0))
    {
        NameKey = nameof(Resources.CustomEntityName),
        ResourceManagerTypeName = typeof(Resources).AssemblyQualifiedName,
        SmallIcon = Icon.SmallIcon,
        LargeIcon = Icon.LargeIcon,
        SingleInstance = false,
        HierarchicalChildTypes = [EntityType.Camera]
    };

    config.AddOrUpdateCustomEntityType(descriptor);
    Console.WriteLine("Custom Entity Type created successfully.");
}

async Task CreateCustomEntity(Engine engine)
{
    Console.WriteLine("\nCreating a new Custom Entity...");

    CustomEntity customEntity = await engine.TransactionManager.ExecuteTransactionAsync(() =>
    {
        var entity = engine.CreateCustomEntity("Custom entity", new Guid(customEntityId));
        entity.RunningState = State.Running;
        return entity;
    });

    Console.WriteLine($"Created Custom Entity: {customEntity.Name} (GUID: {customEntity.Guid})");
}

async Task DisplayCustomEntities(Engine engine)
{
    Console.WriteLine("\nQuerying for custom entities...");

    var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
    query.EntityTypeFilter.Add(EntityType.CustomEntity);
    query.CustomEntityTypes.Add(new Guid(customEntityId));

    QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

    Console.WriteLine($"Number of custom entities found: {args.Data.Rows.Count}");

    List<CustomEntity> entities = args.Data.AsEnumerable()
        .Select(row => engine.GetEntity(row.Field<Guid>(nameof(Guid))))
        .OfType<CustomEntity>()
        .ToList();

    if (entities.Count > 0)
    {
        Console.WriteLine("\nCustom Entities:");
        Console.WriteLine(new string('-', 30));

        foreach (var entity in entities)
        {
            Console.WriteLine($"Name: {entity.Name}");
            Console.WriteLine($"GUID: {entity.Guid}");
            Console.WriteLine($"Description: {entity.Description ?? "N/A"}");
            Console.WriteLine($"Created On: {entity.CreatedOn}");
            Console.WriteLine(new string('-', 30));
        }
    }
    else
    {
        Console.WriteLine("No custom entities found.");
    }
}