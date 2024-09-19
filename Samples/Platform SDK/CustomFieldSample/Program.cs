// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Genetec.Sdk.Queries;
    using Sdk;
    using Sdk.Entities;
    using Sdk.Entities.CustomFields;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            const string server = "localhost";
            const string username = "admin";
            const string password = "";
            const string customFieldName = "Employee ID";
            const string customFieldValue = "12345";

            using var engine = new Engine();

            ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

            if (state == ConnectionStateCode.Success)
            {
                var configuration = (SystemConfiguration)engine.GetEntity(SystemConfiguration.SystemConfigurationGuid);
                ICustomFieldService customFieldService = configuration.CustomFieldService;

                await PrintExistingCustomFields(customFieldService);

                CustomField customField = await CreateOrUpdateCustomField(customFieldService);

                await CreateCardholderWithCustomField(customFieldService, customField, customFieldValue);

                await QueryAndPrintCardholder(customFieldService, customField, customFieldValue);
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            async Task PrintExistingCustomFields(ICustomFieldService service)
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

                void PrintCustomFieldDetails(CustomField customField)
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
            }

            async Task<CustomField> CreateOrUpdateCustomField(ICustomFieldService service)
            {
                Console.WriteLine($"Searching for existing custom field: {customFieldName}");
                CustomField customField = service.CustomFields.FirstOrDefault(cf => cf.EntityType == EntityType.Cardholder && cf.Name.Equals(customFieldName, StringComparison.OrdinalIgnoreCase));

                if (customField is null)
                {
                    Console.WriteLine($"Custom field '{customFieldName}' not found. Creating new custom field...");

                    customField = service.CreateCustomFieldBuilder()
                        .SetEntityType(EntityType.Cardholder)
                        .SetName(customFieldName)
                        .SetValueType(CustomFieldValueType.Text)
                        .SetDefaultValue(string.Empty)
                        .Build();

                    await service.AddCustomFieldAsync(customField);
                    Console.WriteLine($"Created new custom field: {customField.Name}");
                }
                else
                {
                    Console.WriteLine($"Custom field '{customFieldName}' already exists:");
                }

                return customField;
            }

            Task CreateCardholderWithCustomField(ICustomFieldService service, CustomField customField, object value) => engine.TransactionManager.ExecuteTransactionAsync(() =>
            {
                Console.WriteLine("Creating new cardholder");

                var cardholder = (Cardholder)engine.CreateEntity("John Doe", EntityType.Cardholder);
                cardholder.FirstName = "John";
                cardholder.LastName = "Doe";
                cardholder.EmailAddress = "johndoe@example.com";

                Console.WriteLine($"Setting custom field '{customFieldName}'");

                service.SetValue(customField, cardholder.Guid, value);
            });

            async Task QueryAndPrintCardholder(ICustomFieldService service, CustomField customField, string customFieldValue)
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
    }
}