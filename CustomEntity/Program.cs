// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Threading.Tasks;
    using Properties;
    using Sdk;
    using Sdk.Entities;

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
                var config = (SystemConfiguration)engine.GetEntity(SystemConfiguration.SystemConfigurationGuid);

                var customEntityId = new Guid("8385D04C-F04A-4125-81E9-D1C66AFDE572");

                var descriptor = new CustomEntityTypeDescriptor(customEntityId, Resources.CustomEntityName, CustomEntityTypeCapabilities.CanBeFederated | CustomEntityTypeCapabilities.IsVisible | CustomEntityTypeCapabilities.CreateDelete | CustomEntityTypeCapabilities.MapSupport,
                    new Version(1, 0))
                {
                    NameKey = nameof(Resources.CustomEntityName),
                    ResourceManagerTypeName = typeof(Resources).AssemblyQualifiedName,
                    SmallIcon = Icon.SmallIcon,
                    LargeIcon = Icon.LargeIcon
                };

                config.AddOrUpdateCustomEntityType(descriptor);
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