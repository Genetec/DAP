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
    using System.Linq;
    using System.Threading.Tasks;
    using Genetec.Sdk.Queries;
    using Genetec.Sdk.Scripting.Compiler;
    using Genetec.Sdk;
    using Genetec.Sdk.Entities;
    using Genetec.Sdk.Scripting.Compiler.CodeBuilder;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            const string server = "localhost";
            const string username = "admin";
            const string password = "";

            using var engine = new Engine();

            ConnectionStateCode state = await engine.LoginManager.LogOnAsync(server, username, password);

            if (state == ConnectionStateCode.Success)
            {
                Console.WriteLine("Loading all macros into the Engine's entity cache...");

                var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
                query.EntityTypeFilter.Add(EntityType.Macro);
                await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
                Console.WriteLine("Macros loaded into cache.");

                List<Macro> macros = engine.GetEntities(EntityType.Macro).OfType<Macro>().ToList();
                Console.WriteLine($"{macros.Count} macro(s) fetched from the cache:");
                foreach (var entity in macros)
                {
                    Console.WriteLine($"- {entity.Name}");
                }

                IMacroSourceCodeBuilder builder = CodeBuilderFactory.Create();
                builder.AddProperty(typeof(int), "Parameter1");
                builder.AddProperty(typeof(int), "Parameter2");
                builder.AddProperty(typeof(int), "Parameter3");

                string macroSourceCode = builder.Build();

                Console.WriteLine("Creating a new macro...");
                Macro macro = await engine.TransactionManager.ExecuteTransactionAsync(() =>
                {
                    var macro = (Macro)engine.CreateEntity("New Macro", EntityType.Macro);
                    macro.SetSourceCode(macroSourceCode);
                    return macro;
                });
                Console.WriteLine("New macro created successfully.");

                macro.Started += OnMacroOnStarted;
                macro.Completed += OnMacroOnCompleted;
                macro.Aborted += OnMacroOnAborted;

                Macro.ReadOnlyMacroParameterCollection parameters = macro.DefaultParameters;
                parameters["Parameter1"].Value = 1;
                parameters["Parameter2"].Value = 2;
                parameters["Parameter3"].Value = 3;    

                Guid instanceId = macro.Execute(parameters);
                Console.WriteLine($"Executing the new macro instance ID: {instanceId}");
            }
            else
            {
                Console.WriteLine($"Login failed with state: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            void OnMacroOnStarted(object sender, MacroEventArgs e)
            {
                var macro = (Macro)engine.GetEntity(e.MacroGuid);
                Console.WriteLine($"{macro.Name} started. Instance ID: {e.InstanceGuid}");
            }

            void OnMacroOnCompleted(object sender, MacroEventArgs e)
            {
                var macro = (Macro)engine.GetEntity(e.MacroGuid);
                Console.WriteLine($"{macro.Name} completed. Instance ID: {e.InstanceGuid}");

                if (e.RuntimeException != null)
                {
                    Console.WriteLine($"Runtime exception: {e.RuntimeException.Message}");
                }
            }

            void OnMacroOnAborted(object sender, MacroEventArgs e)
            {
                var macro = (Macro)engine.GetEntity(e.MacroGuid);
                Console.WriteLine($"{macro.Name} aborted. Instance ID: {e.InstanceGuid}");

                if (e.RuntimeException != null)
                {
                    Console.WriteLine($"Runtime exception: {e.RuntimeException.Message}");
                }
            }
        }
    }
}