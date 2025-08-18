// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genetec.Dap.CodeSamples
{
    public class SecurityManagerSample : SampleBase
    {
        protected override async Task RunAsync(Engine engine, CancellationToken token)
        {
            PrintPrivileges(engine);
            await PrintUsersPrivileges(engine);
        }

        private async Task PrintUsersPrivileges(Engine engine)
        {
            var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
            query.EntityTypeFilter.Add(EntityType.User);
            await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

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

        private void PrintPrivileges(Engine engine)
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