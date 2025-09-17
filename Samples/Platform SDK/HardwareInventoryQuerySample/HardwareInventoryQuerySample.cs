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
        List<HardwareInventoryInfo> allHardware = await GetAllHardware(engine);
        DisplayHardwareInventory(engine, allHardware);
    }

    private async Task<List<HardwareInventoryInfo>> GetAllHardware(Engine engine)
    {
        Console.WriteLine("Retrieving all hardware inventory...");

        var query = (HardwareInventoryQuery)engine.ReportManager.CreateReportQuery(ReportType.HardwareInventoryReport);
        query.SourceTypes.Add(HardwareUnitType.Video);
        query.SourceTypes.Add(HardwareUnitType.AccessControl);
        query.SourceTypes.Add(HardwareUnitType.IntrusionDetection);
        query.SourceTypes.Add(HardwareUnitType.Lpr);

        QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
        return args.Data.AsEnumerable().Select(CreateHardwareInventoryInfo).ToList();
    }

    private static HardwareInventoryInfo CreateHardwareInventoryInfo(DataRow row)
    {
        return new HardwareInventoryInfo
        {
            UnitName = row.Field<string>(HardwareInventoryQuery.UnitTypeColumnName),
            UnitType = row.Field<EntityType?>(HardwareInventoryQuery.UnitTypeNameColumnName),
            Manufacturer = row.Field<string>(HardwareInventoryQuery.ManufacturerColumnName),
            ProductType = row.Field<string>(HardwareInventoryQuery.ProductTypeColumnName),
            Role = row.Field<Guid?>(HardwareInventoryQuery.RoleColumnName),
            FirmwareVersion = row.Field<string>(HardwareInventoryQuery.FirmwareVersionColumnName),
            IpAddress = row.Field<string>(HardwareInventoryQuery.IpAddressColumnName),
            PhysicalAddress = row.Field<string>(HardwareInventoryQuery.PhysicalAddressColumnName),
            TimeZone = row.Field<string>(HardwareInventoryQuery.TimeZoneColumnName),
            User = row.Field<string>(HardwareInventoryQuery.UserColumnName),
            PasswordStrength = row.Field<string>(HardwareInventoryQuery.PasswordStrengthColumnName),
            UpgradeStatus = row.Field<string>(HardwareInventoryQuery.UpgradeStatusColumnName),
            NextUpgrade = row.Field<string>(HardwareInventoryQuery.NextUpgradeColumnName),
            ReasonForUpgradeFailure = row.Field<UnitUpgradeErrorDetails?>(HardwareInventoryQuery.ReasonForUpgradeFailureColumnName),
            State = row.Field<State?>(HardwareInventoryQuery.StateColumnName),
            PlatformVersion = row.Field<string>(HardwareInventoryQuery.PlatformVersionColumnName),
            LastUpdatePassword = row.Field<string>(HardwareInventoryQuery.LastUpdatePasswordColumnName),
            UpgradeProgression = row.Field<string>(HardwareInventoryQuery.UpgradeProgressionColumnName),
            ReaderType = row.Field<string>(HardwareInventoryQuery.ReaderTypeColumnName),
            ReaderEncryptionStatus = row.Field<DeviceReaderEncryptionStatus?>(HardwareInventoryQuery.ReaderEncryptionStatusColumnName),
            Related = row.Field<string>(HardwareInventoryQuery.RelatedColumnName),
            LicenseConsumption = row.Field<string>(HardwareInventoryQuery.LicenseConsumptionColumnName)
        };
    }

    private void DisplayHardwareInventory(Engine engine, List<HardwareInventoryInfo> inventory)
    {
        Console.WriteLine($"\nFound {inventory.Count} hardware items:\n");

        if (!inventory.Any())
        {
            Console.WriteLine("No hardware inventory items found.");
            return;
        }

        foreach (var item in inventory)
        {
            Console.WriteLine($"Hardware: {item.UnitName ?? "Unknown"}");
            Console.WriteLine($"  Type: {item.UnitType}");
            Console.WriteLine($"  Manufacturer: {item.Manufacturer ?? "N/A"}");
            Console.WriteLine($"  Product Type: {item.ProductType ?? "N/A"}");
            Console.WriteLine($"  Role: {GetEntityName(item.Role)}");
            Console.WriteLine($"  IP Address: {item.IpAddress ?? "N/A"}");
            Console.WriteLine($"  Firmware Version: {item.FirmwareVersion ?? "N/A"}");
            Console.WriteLine($"  Platform Version: {item.PlatformVersion ?? "N/A"}");
            Console.WriteLine($"  State: {item.State?.ToString() ?? "N/A"}");
            Console.WriteLine($"  Physical Address: {item.PhysicalAddress ?? "N/A"}");
            Console.WriteLine($"  Time Zone: {item.TimeZone ?? "N/A"}");
            Console.WriteLine($"  User: {item.User ?? "N/A"}");
            Console.WriteLine($"  Password Strength: {item.PasswordStrength ?? "N/A"}");
            Console.WriteLine($"  Upgrade Status: {item.UpgradeStatus ?? "N/A"}");

            if (!string.IsNullOrEmpty(item.ReaderType))
            {
                Console.WriteLine($"  Reader Type: {item.ReaderType}");
                Console.WriteLine($"  Reader Encryption: {item.ReaderEncryptionStatus?.ToString() ?? "N/A"}");
                Console.WriteLine($"  Related: {item.Related ?? "N/A"}");
            }

            if (!string.IsNullOrEmpty(item.LicenseConsumption))
            {
                Console.WriteLine($"  License Consumption: {item.LicenseConsumption}");
            }

            Console.WriteLine(new string('-', 50));
        }

        string GetEntityName(Guid? entityId) => engine.GetEntity(entityId.GetValueOrDefault())?.Name ?? "Unknown";
    }
}