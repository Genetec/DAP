// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

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

        using var engine = new Engine();

        ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

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