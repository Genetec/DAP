// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries;

SdkResolver.Initialize();

await RunSample();

async Task RunSample()
{
    const string server = "localhost";
    const string username = "admin";
    const string password = "";

    using var engine = new Engine();

    ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

    if (state != ConnectionStateCode.Success)
    {
        Console.WriteLine($"Logon failed: {state}");
        return;
    }

    await LoadPartitions(); // Load all partitions into the entity cache

    // Create initial partitions
    var defaultPartition1 = (Partition)engine.CreateEntity("Partition 1", EntityType.Partition);
    var defaultPartition2 = (Partition)engine.CreateEntity("Partition 2", EntityType.Partition);

    // Set default creation partitions
    // New entities will be automatically added to these partitions
    // Up to 3 partitions can be set, In this example, only two partitions are set
    engine.DefaultEntityCreationPartitions = new CreationPartitions(defaultPartition1.Guid, defaultPartition2.Guid, null);

    DisplayCreationPartitions(engine.DefaultEntityCreationPartitions);

    // Create two cardholders (they should be automatically added to the default partitions)
    var cardholder1 = (Cardholder)engine.CreateEntity("Cardholder 1", EntityType.Cardholder);
    var cardholder2 = (Cardholder)engine.CreateEntity("Cardholder 2", EntityType.Cardholder);

    DisplayEntityPartitions(cardholder1);
    DisplayEntityPartitions(cardholder2);

    // Create a new partition
    var newPartition = (Partition)engine.CreateEntity("Partition 3", EntityType.Partition);

    // Demonstrate two ways to add entities to a non-default partition
    newPartition.AddMember(cardholder1);
    Console.WriteLine($"\nAdded {cardholder1.Name} to {newPartition.Name} using Partition.AddMember (Method 1)");
    DisplayEntityPartitions(cardholder1);

    ((IPartitionSupport)cardholder2).InsertIntoPartition(newPartition.Guid);
    Console.WriteLine($"\nAdded {cardholder2.Name} to {newPartition.Name} using Entity.InsertIntoPartition (Method 2)");
    DisplayEntityPartitions(cardholder2);

    Console.WriteLine("\nPartition members after adding cardholders:");
    DisplayPartitionMembers(newPartition);

    // Demonstrate two ways to remove entities from a partition
    newPartition.RemoveMember(cardholder1);
    Console.WriteLine($"\nRemoved {cardholder1.Name} from {newPartition.Name} using Partition.RemoveMember (Method 1)");
    DisplayEntityPartitions(cardholder1);

    ((IPartitionSupport)cardholder2).RemoveFromPartition(newPartition.Guid);
    Console.WriteLine($"\nRemoved {cardholder2.Name} from {newPartition.Name} using Entity.RemoveFromPartition (Method 2)");
    DisplayEntityPartitions(cardholder2);

    DisplayPartitionMembers(newPartition);

    async Task LoadPartitions()
    {
        Console.WriteLine("Loading partitions...");
        var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
        query.EntityTypeFilter.Add(EntityType.Partition);
        query.DownloadAllRelatedData = false;
        query.Page = 1;
        query.PageSize = 1000;
        QueryCompletedEventArgs args;
        do
        {
            args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
            query.Page++;
        } while (args.Data.Rows.Count > query.PageSize);
    }

    void DisplayCreationPartitions(CreationPartitions creationPartitions)
    {
        Console.WriteLine("\nDefault creation partitions:");

        Console.WriteLine("  First:  " + (creationPartitions.First.HasValue ? ((Partition)engine.GetEntity(creationPartitions.First.Value)).Name : "None"));
        Console.WriteLine("  Second: " + (creationPartitions.Second.HasValue ? ((Partition)engine.GetEntity(creationPartitions.Second.Value)).Name : "None"));
        Console.WriteLine("  Third:  " + (creationPartitions.Third.HasValue ? ((Partition)engine.GetEntity(creationPartitions.Third.Value)).Name : "None"));
    }

    void DisplayPartitionMembers(Partition partition)
    {
        Console.WriteLine($"Members of {partition.Name}:");
        foreach (var memberGuid in partition.Members)
        {
            Entity member = engine.GetEntity(memberGuid);
            Console.WriteLine($"  - {member.Name} ({member.EntityType})");
        }
    }

    void DisplayEntityPartitions(Entity entity)
    {
        Console.WriteLine($"\nPartitions for {entity.Name}:");

        IEnumerable<Guid> partitions = entity.GetPartitions();
        if (!partitions.Any())
        {
            Console.WriteLine("  - No partitions");
            return;
        }

        foreach (var partitionGuid in partitions)
        {
            Console.WriteLine($"  - {engine.GetEntity(partitionGuid).Name}");
        }
    }
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();
