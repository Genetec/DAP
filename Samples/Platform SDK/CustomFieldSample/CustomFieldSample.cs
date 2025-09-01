// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Entities.CustomFields;
using Genetec.Sdk.Queries;

namespace Genetec.Dap.CodeSamples;

public class CustomFieldSample : SampleBase
{
    private const string s_customFieldName = "Employee ID";
    private const string s_customFieldValue = "12345";

    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        var configuration = (SystemConfiguration)engine.GetEntity(SystemConfiguration.SystemConfigurationGuid);
        ICustomFieldService customFieldService = configuration.CustomFieldService;

        await PrintExistingCustomFields(customFieldService);

        CustomField customField = await CreateOrUpdateCustomField(customFieldService);

        await CreateCardholderWithCustomField(engine, customFieldService, customField, s_customFieldValue);

        await QueryAndPrintCardholder(engine, customFieldService, customField, s_customFieldValue);
    }

    private async Task PrintExistingCustomFields(ICustomFieldService service)
    {
        Console.WriteLine("Existing Custom Fields:");

        IReadOnlyList<CustomField> customFields = await service.GetCustomFieldsAsync();

        foreach (IGrouping<EntityType, CustomField> group in customFields.GroupBy(cf => cf.EntityType).OrderBy(g => g.Key))
        {
            Console.WriteLine($"\nEntity Type: {group.Key}");
            Console.WriteLine(new string('-', 20));

            foreach (CustomField customField in group.OrderBy(cf => cf.Name))
            {
                PrintCustomFieldDetails(customField);
            }
        }
    }

    private void PrintCustomFieldDetails(CustomField customField)
    {
        Console.WriteLine($"  Name: {customField.Name}");
        Console.WriteLine($"    Guid: {customField.Guid}");
        Console.WriteLine($"    ValueType: {customField.ValueType}");
        Console.WriteLine($"    CustomEntityTypeId: {customField.CustomEntityTypeId}");
        Console.WriteLine($"    DefaultValue: {customField.DefaultValue}");
        Console.WriteLine($"    GroupName: {customField.GroupName}");
        Console.WriteLine($"    GroupPriority: {customField.GroupPriority}");
        Console.WriteLine($"    Mandatory: {customField.Mandatory}");
        Console.WriteLine($"    Owner: {customField.Owner}");
        Console.WriteLine($"    ShowInReports: {customField.ShowInReports}");
        Console.WriteLine($"    Unique: {customField.Unique}");
        Console.WriteLine($"    CustomDataType: {customField.CustomDataType}");
        Console.WriteLine();
    }

    private async Task<CustomField> CreateOrUpdateCustomField(ICustomFieldService service)
    {
        Console.WriteLine($"Searching for existing custom field: {s_customFieldName}");
        CustomField customField = service.CustomFields.FirstOrDefault(cf => cf.EntityType == EntityType.Cardholder && cf.Name.Equals(s_customFieldName, StringComparison.OrdinalIgnoreCase));

        if (customField is null)
        {
            Console.WriteLine($"Custom field '{s_customFieldName}' not found. Creating new custom field...");

            customField = service.CreateCustomFieldBuilder()
                .SetEntityType(EntityType.Cardholder)
                .SetName(s_customFieldName)
                .SetValueType(CustomFieldValueType.Text)
                .SetDefaultValue(string.Empty)
                .Build();

            await service.AddCustomFieldAsync(customField);
            Console.WriteLine($"Created new custom field: {customField.Name}");
        }
        else
        {
            Console.WriteLine($"Custom field '{s_customFieldName}' already exists:");
        }

        return customField;
    }

    private Task CreateCardholderWithCustomField(Engine engine, ICustomFieldService service, CustomField customField, object value) => engine.TransactionManager.ExecuteTransactionAsync(() =>
    {
        Console.WriteLine("Creating new cardholder");

        var cardholder = (Cardholder)engine.CreateEntity("John Doe", EntityType.Cardholder);
        cardholder.FirstName = "John";
        cardholder.LastName = "Doe";
        cardholder.EmailAddress = "johndoe@example.com";

        Console.WriteLine($"Setting custom field '{s_customFieldName}'");

        service.SetValue(customField, cardholder.Guid, value);
    });

    private async Task QueryAndPrintCardholder(Engine engine, ICustomFieldService service, CustomField customField, string customFieldValue)
    {
        Console.WriteLine($"Searching for cardholder with custom field '{customField.Name}' value: {customFieldValue}");

        var query = (CardholderConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.CardholderConfiguration);
        query.CustomFields.Add(new CustomFieldFilter(customField, customFieldValue, FieldRangeType.Equal));

        QueryCompletedEventArgs queryResults = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

        Console.WriteLine($"Found {queryResults.Data.Rows.Count} cardholder(s) matching the query:");

        foreach (Cardholder cardholder in queryResults.Data.AsEnumerable().Select(row => engine.GetEntity(row.Field<Guid>(nameof(Guid)))).OfType<Cardholder>())
        {
            Console.WriteLine($"- Name: {cardholder.Name}");
            Console.WriteLine($"  First Name: {cardholder.FirstName}");
            Console.WriteLine($"  Last Name: {cardholder.LastName}");
            Console.WriteLine($"  Email: {cardholder.EmailAddress}");
            Console.WriteLine($"  Custom Field '{customField.Name}' Value: {service.GetValue(customField, cardholder.Guid)}");
        }
    }
}