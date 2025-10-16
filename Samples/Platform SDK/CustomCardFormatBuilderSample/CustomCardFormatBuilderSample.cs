// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Credentials;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Entities.CustomCardFormats;

public class CustomCardFormatBuilderSample : SampleBase
{
    protected override Task RunAsync(Engine engine, CancellationToken token)
    {
        DisplayCredentialFormats(engine);

        var config = (SystemConfiguration)engine.GetEntity(SystemConfiguration.SystemConfigurationGuid);
        ICustomCardFormatService service = config.CustomCardFormatService;

        Console.WriteLine("Creating a simple custom card format with a single field...");
        CreateSimpleCustomCardFormat(service, "Simple Custom Format", 32);

        Console.WriteLine("Creating a complex custom card format with multiple fields...");
        CreateComplexCustomCardFormat(service, "Complex Custom Format", 64);

        Console.WriteLine("Creating a custom card format with parity checks...");
        CreateCustomCardFormatWithParityChecks(service, "Wiegand 48-bit with Parity", 48);

        Console.WriteLine("Creating an ABA custom card format...");
        CreateABACustomCardFormat(service, "ABA Track 2 Format");

        Console.WriteLine();
        Console.WriteLine("Custom card formats created successfully!");
        Console.WriteLine();

        DisplayCredentialFormats(engine);

        return Task.CompletedTask;
    }

    private void CreateSimpleCustomCardFormat(ICustomCardFormatService service, string name, int bitLength)
    {
        // Create a simple custom Wiegand card format with a single field
        // The mask format is "bits:start-end" where start is the first bit and end is the last bit
        string mask = $"bits:0-{bitLength - 1}";

        var builder = service.GetCustomCardFormatBuilder();
        var customFormat = builder
            .SetName(name)
            .SetType(CardFormatTypeEnum.Wiegand)
            .SetLength(bitLength)
            .AddFormatField("CardId", mask, FormatFieldValueRepresentation.Decimal)
            .Build();

        service.AddOrUpdateCustomCardFormat(customFormat);

        Console.WriteLine($"Created custom card format: {name}");
        Console.WriteLine($"  Bit Length: {bitLength}");
        Console.WriteLine($"  Fields: 1 (CardId)");
        Console.WriteLine();
    }

    private void CreateComplexCustomCardFormat(ICustomCardFormatService service, string name, int bitLength)
    {
        var builder = service.GetCustomCardFormatBuilder();
        var customFormat = builder
            .SetName(name)
            .SetType(CardFormatTypeEnum.Wiegand)
            .SetLength(bitLength)
            .AddFormatField("FacilityCode", "bits:0-7", FormatFieldValueRepresentation.Decimal)
            .AddFormatField("CardNumber", "bits:8-31", FormatFieldValueRepresentation.Decimal)
            .AddFormatField("IssueCode", "bits:32-39", FormatFieldValueRepresentation.Decimal)
            .AddFormatField("Reserved", "bits:40-63", FormatFieldValueRepresentation.Decimal)
            .Build();

        service.AddOrUpdateCustomCardFormat(customFormat);

        Console.WriteLine($"Created custom card format: {name}");
        Console.WriteLine($"  Bit Length: {bitLength}");
        Console.WriteLine($"  Fields: 4 (FacilityCode, CardNumber, IssueCode, Reserved)");
        Console.WriteLine();
    }

    private void CreateCustomCardFormatWithParityChecks(ICustomCardFormatService service, string name, int bitLength)
    {
        var builder = service.GetCustomCardFormatBuilder();
        var customFormat = builder
            .SetName(name)
            .SetType(CardFormatTypeEnum.Wiegand)
            .SetLength(bitLength)
            .AddFormatField("FacilityCode", "bits:1-8", FormatFieldValueRepresentation.Decimal)
            .AddFormatField("CardNumber", "bits:9-24", FormatFieldValueRepresentation.Decimal)
            .AddFormatField("Reserved", "bits:25-46", FormatFieldValueRepresentation.Decimal)
            .AddParityCheck(0, ParityCheckType.Even, "bits:1-23")
            .AddParityCheck(47, ParityCheckType.Odd, "bits:24-46")
            .Build();

        service.AddOrUpdateCustomCardFormat(customFormat);

        Console.WriteLine($"Created custom card format: {name}");
        Console.WriteLine($"  Bit Length: {bitLength}");
        Console.WriteLine($"  Fields: 3 (FacilityCode, CardNumber, Reserved)");
        Console.WriteLine($"  Parity Checks: 2 (Even at bit 0, Odd at bit 47)");
        Console.WriteLine();
    }

    private void CreateABACustomCardFormat(ICustomCardFormatService service, string name)
    {
        var builder = service.GetCustomCardFormatBuilder();
        var customFormat = builder
            .SetName(name)
            .SetType(CardFormatTypeEnum.ABA)
            .AddFormatField("PAN", 16, DelimiterType.Equals)  // Primary Account Number
            .AddFormatField("ExpirationDate", 4, DelimiterType.Equals)
            .AddFormatField("ServiceCode", 3, DelimiterType.Colon)
            .Build();

        service.AddOrUpdateCustomCardFormat(customFormat);

        Console.WriteLine($"Created ABA custom card format: {name}");
        Console.WriteLine($"  Type: ABA (Magnetic Stripe)");
        Console.WriteLine($"  Fields: 3 (PAN, ExpirationDate, ServiceCode)");
        Console.WriteLine();
    }

    private void DisplayCredentialFormats(Engine engine)
    {
        Console.WriteLine("Existing Credential Formats:");
        Console.WriteLine("----------------------------");

        var config = (SystemConfiguration)engine.GetEntity(entityId: SystemConfiguration.SystemConfigurationGuid);

        int standardCount = 0;
        int customCount = 0;

        foreach (CredentialFormat format in config.CredentialFormats)
        {
            if (format.IsCustomFormat)
            {
                customCount++;
                Console.WriteLine($"[CUSTOM] {format.Name}");
                Console.WriteLine($"  Type: {format.Type}");
                Console.WriteLine($"  Bit Length: {format.BitLength}");
                Console.WriteLine();
            }
            else
            {
                standardCount++;
            }
        }

        Console.WriteLine($"Total Formats: {config.CredentialFormats.Count} (Standard: {standardCount}, Custom: {customCount})");
        Console.WriteLine();
    }
}
