// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Genetec.Sdk;
    using Sdk.Entities;
    using Sdk.Queries;

    internal class Program
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
                PrintPrivileges(engine);
                await PrintUsersPrivileges(engine);
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static async Task PrintUsersPrivileges(Engine engine)
        {
            var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
            query.EntityTypeFilter.Add(EntityType.User);
            var result = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

            var definitions = engine.SecurityManager.GetPrivilegeDefinitions().ToDictionary(information => information.Id);

            foreach (User user in engine.GetEntities(EntityType.User).OfType<User>())
            {
                Console.WriteLine($"User: {user.Name}");

                foreach (var privilegeInformation in user.Privileges.Where(e => e.State == PrivilegeAccess.Granted))
                {
                    if (definitions.TryGetValue(privilegeInformation.PrivilegeGuid, out var definition) && definition.Type != PrivilegeType.Group)
                    {
                        Console.WriteLine($"{definition.Description}");
                    }
                }
            }
        }

        static void PrintPrivileges(Engine engine)
        {
            var definitions = engine.SecurityManager.GetPrivilegeDefinitions().ToDictionary(information => information.Id);

            foreach (var privilege in definitions.Values.Where(p => p.ParentId == Guid.Empty))
            {
                PrintPrivilege(privilege, 0);
            }

            void PrintPrivilege(PrivilegeDefinitionInformation privilege, int depth)
            {
                Console.WriteLine($"{new string(' ', depth * 2)}({privilege.Type}) {privilege.Description}");

                foreach (var child in privilege.Children)
                {
                    if (definitions.TryGetValue(child, out var value))
                    {
                        PrintPrivilege(value, depth + 1);
                    }
                }
            }
        }
    }
}