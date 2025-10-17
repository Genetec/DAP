// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using Genetec.Sdk;
using Genetec.Sdk.Credentials;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Entities.Builders;
using Genetec.Sdk.Entities.CustomCardFormats;
using Genetec.Sdk.Queries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genetec.Dap.CodeSamples;

public class CustomCardFormatBuilderSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        var systemConfig = (SystemConfiguration)engine.GetEntity(SystemConfiguration.SystemConfigurationGuid);
        ICustomCardFormatService customCardService = systemConfig.CustomCardFormatService;

        DemonstrateGetCardFormats(customCardService);

        DemonstrateCreateUsingSetFields(customCardService);

        DemonstrateUpdateUsingSetFields(customCardService);

        await DemonstrateCreateCredential(engine, customCardService);

        DemonstrateExportAndImport(customCardService);

        DemonstrateDeleteCardFormat(customCardService);
    }

    private void DemonstrateGetCardFormats(ICustomCardFormatService service)
    {
        Console.WriteLine("Getting Custom Card Formats");
        Console.WriteLine(new string('-', 50));

        // Get all custom card formats
        List<CustomCardFormat> formats = service.GetCustomCardFormats();

        if (formats.Count == 0)
        {
            Console.WriteLine("No custom card formats found.");
        }
        else
        {
            foreach (CustomCardFormat format in formats)
            {
                Console.WriteLine($"Name: {format.Name}");
                Console.WriteLine($"  ID: {format.Id}");
                Console.WriteLine($"  Format ID: {format.FormatID}");
                Console.WriteLine($"  Format: {format.FormatString}");
                Console.WriteLine($"  Bit Length: {format.Length}");
                Console.WriteLine($"  Description: {format.Description}");
                Console.WriteLine();
            }

            // Demonstrate getting a specific format by name
            CustomCardFormat firstFormat = formats[0];
            CustomCardFormat formatByName = service.GetCustomCardFormat(firstFormat.Name);
            Console.WriteLine($"Retrieved format by name: {formatByName?.Name}");

            // Demonstrate getting a specific format by ID
            CustomCardFormat formatById = service.GetCustomCardFormat(firstFormat.Id);
            Console.WriteLine($"Retrieved format by ID: {formatById?.Name}");

            // Demonstrate getting a specific format by FormatID
            CustomCardFormat formatByFormatId = service.GetCustomCardFormat(firstFormat.FormatID);
            Console.WriteLine($"Retrieved format by Format ID: {formatByFormatId?.Name}");
        }

        Console.WriteLine();
    }

    private void DemonstrateCreateUsingSetFields(ICustomCardFormatService service)
    {
        Console.WriteLine("Creating Format Using Set Fields");
        Console.WriteLine(new string('-', 50));

        string formatName = "Demo Format With Fields";

        CustomCardFormat existingFormat = service.GetCustomCardFormats().FirstOrDefault(f => f.Name == formatName);
        if (existingFormat != null)
        {
            Console.WriteLine($"Format '{formatName}' already exists. Skipping creation.\n");
            return;
        }

        ICustomCardFormatBuilder builder = service.GetCustomCardFormatBuilder();

        CustomCardFormat customFormat = builder
            .SetName(formatName)
            .SetDescription("Demonstrates various field operations")
            .SetFormatString(null)
            .SetLength(40)
            .SetType(CardFormatTypeEnum.Wiegand)
            .AddFormatField("Field 1", "3-10", FormatFieldValueRepresentation.Decimal)
            .AddFormatField("Field 2", "11-38", FormatFieldValueRepresentation.Decimal)
            .SetFieldFixedValue("Field 2", "123")     // Sets the fixed value for Field 2
            .SetFieldAcceptRange("Field 2")           // Will not be set as Field 2 is fixed
            .RemoveFieldFixedValue("Field 2")         // Removing the fixed value on Field 2
            .SetFieldAcceptRange("Field 2")           // Will now set accept range to true
            .SetFieldFixedValue("Field 2", "127")     // Sets fixed value for Field 2 and accept range to false
            .AddParityCheck(0, ParityCheckType.Even, "3-19")
            .AddParityCheck(1, ParityCheckType.Odd, "3-19")
            .Build();

        bool success = service.AddOrUpdateCustomCardFormat(customFormat);
        if (success)
        {
            Console.WriteLine($"Successfully created format with various field operations: {formatName}");
        }
        else
        {
            Console.WriteLine($"Failed to create format: {formatName}");
        }

        Console.WriteLine();
    }

    private void DemonstrateUpdateUsingSetFields(ICustomCardFormatService service)
    {
        Console.WriteLine("Updating Existing Custom Format");
        Console.WriteLine(new string('-', 50));

        string formatName = "Demo Format With Fields";
        CustomCardFormat existingFormat = service.GetCustomCardFormats().FirstOrDefault(f => f.Name == formatName);

        if (existingFormat == null)
        {
            Console.WriteLine($"Format '{formatName}' not found. Cannot modify.\n");
            return;
        }

        // Load the builder with existing card format (can use ID, FormatID, or name)
        ICustomCardFormatBuilder builder = service.GetCustomCardFormatBuilder(formatName);

        CustomCardFormat modifiedFormat = builder
            .AddFormatField("Field 3", "39-39", FormatFieldValueRepresentation.Hexadecimal)
            .AddParityCheck(parityPosition: 2, parityType: ParityCheckType.Odd, mask: "3-19")
            .RemoveFormatField("Field 3")              // Removes field and adjust order
            .RemoveParityCheck(1)                      // Removes check and adjust priority
            .UpdateFormatFieldName("Field 1", "Field")
            .UpdateWiegandFieldRepresentation("Field 2", FormatFieldValueRepresentation.Hexadecimal)
            .UpdateWiegandFieldMask("Field", "5-10")
            .UpdateParityCheck(2, parityPosition: 1)                 // Updates parity position
            .UpdateParityCheck(2, parityType: ParityCheckType.Even)  // Updates parity type
            .UpdateParityCheck(0, mask: "4-19")                      // Updates parity mask
            .SetDescription("Updated: Demonstrates field and parity operations")
            .Build();

        bool success = service.AddOrUpdateCustomCardFormat(modifiedFormat);
        if (success)
        {
            Console.WriteLine($"Successfully modified format: {formatName}");
        }
        else
        {
            Console.WriteLine($"Failed to modify format: {formatName}");
        }

        Console.WriteLine();
    }

    private async Task DemonstrateCreateCredential(Engine engine, ICustomCardFormatService service)
    {
        Console.WriteLine("Creating Credential with Custom Format");
        Console.WriteLine(new string('-', 50));

        string formatName = "Demo Format With Fields";
        CustomCardFormat customFormat = service.GetCustomCardFormats().FirstOrDefault(f => f.Name == formatName);

        if (customFormat == null)
        {
            Console.WriteLine($"Format '{formatName}' not found. Cannot create credential.\n");
            return;
        }

        var values = new Dictionary<string, string>
        {
            { "Field", "50" },      // Field is bits 5-10 (6 bits), max value is 63
            { "Field 2", "127" }    // Field 2 has fixed value of 127
        };

        var credentialFormat = new CustomCredentialFormat(customFormat.FormatID, values);

        Credential credential = await FindCredential(credentialFormat);
        if (credential is null)
        {
            ICredentialBuilder credentialBuilder = engine.EntityManager.GetCredentialBuilder();
            string credentialName = $"Credential {customFormat.Name}";

            credential = credentialBuilder
               .SetName(credentialName)
               .SetFormat(credentialFormat)
               .Build();

            Console.WriteLine($"Successfully created credential: {credentialName}");
        }
        else
        {
            Console.WriteLine($"Credential with the specified custom format already exists: {credential.Name}");
        }

        Console.WriteLine($"  Format: {credential.Format.Name}");
        Console.WriteLine($"  Unique ID: {credential.Format.UniqueId}");
        Console.WriteLine($"  Raw Data: {credential.Format.RawData}");

        Console.WriteLine();

        async Task<Credential> FindCredential(CredentialFormat format)
        {
            var query = (CredentialConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.CredentialConfiguration);
            query.UniqueIds.Add(format);
            QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
            return args.Data.AsEnumerable().Select(row => engine.GetEntity(row.Field<Guid>(nameof(Guid)))).OfType<Credential>().FirstOrDefault();
        }
    }

    private void DemonstrateExportAndImport(ICustomCardFormatService service)
    {
        Console.WriteLine("Exporting and Importing Custom Format");
        Console.WriteLine(new string('-', 50));

        string formatName = "Demo Format With Fields";
        CustomCardFormat format = service.GetCustomCardFormats().FirstOrDefault(f => f.Name == formatName);

        if (format == null)
        {
            Console.WriteLine($"Format '{formatName}' not found. Cannot export.\n");
            return;
        }

        string exportPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{formatName}.xml");

        try
        {
            service.ExportToXMLFile(exportPath, format.FormatID);
            Console.WriteLine($"Successfully exported format to: {exportPath}");

            string importFormatName = $"{formatName}_Imported";
            CustomCardFormat existingImport = service.GetCustomCardFormats().FirstOrDefault(f => f.Name == importFormatName);

            if (existingImport == null)
            {
                ICustomCardFormatBuilder builder = service.GetCustomCardFormatBuilder();
                CustomCardFormat importedFormat = builder
                    .SetFromXmlFile(exportPath)
                    .SetName(importFormatName)
                    .SetFormatId(Guid.NewGuid())
                    .Build();

                bool success = service.AddOrUpdateCustomCardFormat(importedFormat);

                if (success)
                {
                    Console.WriteLine($"Successfully imported format as: {importFormatName}");
                }
                else
                {
                    Console.WriteLine($"Failed to import format.");
                }
            }
            else
            {
                Console.WriteLine($"Format '{importFormatName}' already exists. Skipping import.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during export/import: {ex.Message}");
        }

        Console.WriteLine();
    }

    private void DemonstrateDeleteCardFormat(ICustomCardFormatService service)
    {
        Console.WriteLine("Deleting Custom Card Format");
        Console.WriteLine(new string('-', 50));

        string formatName = "Demo Format With Fields";
        CustomCardFormat format = service.GetCustomCardFormats().FirstOrDefault(f => f.Name == formatName);

        if (format == null)
        {
            Console.WriteLine($"Format '{formatName}' not found. Cannot delete.\n");
            return;
        }

        Console.WriteLine($"Deleting format: {formatName}");
        Console.WriteLine($"  ID: {format.Id}");
        Console.WriteLine($"  Format ID: {format.FormatID}");

        bool success = service.DeleteCustomCardFormat(formatName);
        if (success)
        {
            Console.WriteLine($"Successfully deleted format: {formatName}");
        }
        else
        {
            Console.WriteLine($"Failed to delete format: {formatName}");
        }

        Console.WriteLine();
    }
}