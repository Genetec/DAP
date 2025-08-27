// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples.Properties;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries;

namespace Genetec.Dap.CodeSamples;

public class CustomEntitySample : SampleBase
{
    const string s_customEntityId = "8385D04C-F04A-4125-81E9-D1C66AFDE572"; // TODO: Replace with your custom entity ID

    protected override async Task RunAsync(Engine engine, CancellationToken token)
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

    void DisplayCustomEntityTypeDescriptors(IReadOnlyCollection<CustomEntityTypeDescriptor> descriptors)
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
        var id = new Guid(s_customEntityId);

        if (config.GetCustomActionTypeDescriptor(id) is not null)
        {
            Console.WriteLine($"Custom Entity Type with ID {s_customEntityId} already exists.");
            return;
        }

        Console.WriteLine($"Creating new Custom Entity Type with ID: {s_customEntityId}");

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
            HierarchicalChildTypes = new[] { EntityType.Camera }
        };

        config.AddOrUpdateCustomEntityType(descriptor);
        Console.WriteLine("Custom Entity Type created successfully.");
    }

    async Task CreateCustomEntity(Engine engine)
    {
        Console.WriteLine("\nCreating a new Custom Entity...");

        CustomEntity customEntity = await engine.TransactionManager.ExecuteTransactionAsync(() =>
        {
            var entity = engine.CreateCustomEntity("Custom entity", new Guid(s_customEntityId));
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
        query.CustomEntityTypes.Add(new Guid(s_customEntityId));

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
}