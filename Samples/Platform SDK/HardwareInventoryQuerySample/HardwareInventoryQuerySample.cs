// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Queries;

namespace Genetec.Dap.CodeSamples;

public class HardwareInventoryQuerySample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        List<HardwareInventory> hardwareItems = await QueryHardwareInventory(engine);
        DisplayInventory(hardwareItems);
    }

    private async Task<List<HardwareInventory>> QueryHardwareInventory(Engine engine)
    {
        var query = (HardwareInventoryQuery)engine.ReportManager.CreateReportQuery(ReportType.HardwareInventoryReport);

        QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
        return args.Data.AsEnumerable().Select(MapToHardwareInventory).ToList();

        HardwareInventory MapToHardwareInventory(DataRow row) => new()
        {
            UnitName = row.Field<string>(HardwareInventoryQuery.UnitTypeColumnName), // This is actually unit name
            UnitType = row.Field<EntityType>(HardwareInventoryQuery.UnitTypeNameColumnName),
            Manufacturer = row.Field<string>(HardwareInventoryQuery.ManufacturerColumnName),
            FirmwareVersion = row.Field<string>(HardwareInventoryQuery.FirmwareVersionColumnName),
            IpAddress = row.Field<string>(HardwareInventoryQuery.IpAddressColumnName),
            RoleGuid = row.Field<Guid>(HardwareInventoryQuery.RoleColumnName)
        };
    }

    private void DisplayInventory(List<HardwareInventory> hardwareItems)
    {
        if (!hardwareItems.Any())
        {
            Console.WriteLine("No hardware inventory items found.");
            return;
        }

        Console.WriteLine("Hardware Inventory:");
        Console.WriteLine(new string('-', 80));
        Console.WriteLine($"{"Unit Name",-20} {"Unit Type",-15} {"Manufacturer",-15} {"Firmware",-10} {"IP Address",-15}");
        Console.WriteLine(new string('-', 80));

        foreach (var item in hardwareItems)
        {
            Console.WriteLine($"{item.UnitName,-20} {item.UnitType,-15} {item.Manufacturer,-15} {item.FirmwareVersion,-10} {item.IpAddress,-15}");
        }

        Console.WriteLine(new string('-', 80));
        Console.WriteLine($"Total items: {hardwareItems.Count}");
    }
}