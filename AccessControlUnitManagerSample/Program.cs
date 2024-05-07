// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Security;
    using System.Threading.Tasks;
    using Sdk;
    using Sdk.Entities;
    using Sdk.Entities.AccessControl;
    using Sdk.Queries;

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
                AccessManagerRole accessManagerRole = await GetAccessManagerRole();

                var info = new AddAccessControlUnitInfo(
                    address: IPAddress.Parse("127.0.0.1"),
                    extensionType: AccessControlExtensionType.CloudLink,
                    port: 80,
                    username: "admin",
                    password: new SecureString());

                engine.AccessControlUnitManager.UnitEnrollmentSucceeded += (sender, e) => Console.WriteLine("Unit enrollment succeeded");
                engine.AccessControlUnitManager.UnitEnrollmentFailed += (sender, e) => Console.WriteLine($"Unit enrollment failed: {e.ActionDetails}");
                engine.AccessControlUnitManager.UnitEnrollmentUpdated += (sender, e) => Console.WriteLine($"Unit enrollment progress: {e.State}");

                Console.WriteLine("Enrolling access control unit...");
                engine.AccessControlUnitManager.EnrollAccessControlUnit(info, accessManagerRole.Guid);
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            async Task<AccessManagerRole> GetAccessManagerRole()
            {
                var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
                query.EntityTypeFilter.Add(EntityType.Role);
                query.DownloadAllRelatedData = true;

                await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

                return engine.GetEntities(EntityType.Role).OfType<AccessManagerRole>().FirstOrDefault();
            }
        }
    }
}
