// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
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

            using var engine = new Engine();

            ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

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
    }
}
