// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Sdk;
    using Sdk.Entities;
    using Sdk.Entities.CustomFields;
    using Sdk.Queries;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            const string server = "localhost";
            const string username = "admin";
            const string password = "";

            var engine = new Engine();

            var state = await engine.LogOnAsync(server, username, password);

            if (state == ConnectionStateCode.Success)
            {
                var configuration = (SystemConfiguration)engine.GetEntity(SystemConfiguration.SystemConfigurationGuid);

                ICustomFieldService service = configuration.CustomFieldService;

                const string customFieldName = "My Custom Field";

                var field = service.CustomFields.FirstOrDefault(cf => cf.Name.Equals(customFieldName, StringComparison.OrdinalIgnoreCase) && cf.EntityType == EntityType.Cardholder);
                if (field is null)
                {
                    field = configuration.CustomFieldService.CreateCustomFieldBuilder()
                        .SetEntityType(EntityType.Cardholder)
                        .SetName(customFieldName)
                        .SetValueType(CustomFieldValueType.Text)
                        .Build();

                    await service.AddCustomFieldAsync(field);
                }
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

        }

        async Task<IEnumerable<Entity>> FindEntityByCustomFieldValue(Engine engine, CustomField field, object value)
        {
            var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
            query.CustomFields.Add(new CustomFieldFilter(field, value));
            var result = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
            return result.Data.AsEnumerable().Select(row => engine.GetEntity(row.Field<Guid>(nameof(Guid)))).OfType<Cardholder>();
        }
    }
}
