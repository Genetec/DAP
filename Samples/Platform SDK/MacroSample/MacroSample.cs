// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk.Queries;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Scripting.Compiler;
using Genetec.Sdk.Scripting.Compiler.CodeBuilder;

namespace Genetec.Dap.CodeSamples;

public class MacroSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        // Load macros into the entity cache
        await LoadMacros(engine);

        // Retrieve macros from the entity cache
        List<Macro> macros = engine.GetEntities(EntityType.Macro).OfType<Macro>().ToList();

        // Display all macros
        DisplayMacros(macros);

        // Create a new macro
        Macro macro = await CreateNewMacro(engine);

        // Execute the new macro
        ExecuteMacro(macro);

        await Task.Delay(2000, token); // Let the macro runs for 2 seconds

        // Display running macros
        DisplayRunningMacros(engine);

        // Abort the macro if it is still running
        AbortMacro(engine, macro);
    }

    private void DisplayMacros(List<Macro> macros)
    {
        foreach (var macro in macros)
        {
            Console.WriteLine($"Macro: {macro.Name}");
            Console.WriteLine($"Description: {macro.Description}");

            if (macro.DefaultParameters.Any())
            {
                Console.WriteLine("Parameters:");
                foreach (var parameter in macro.DefaultParameters)
                {
                    Console.WriteLine($"  - Name: {parameter.Name}");
                    Console.WriteLine($"    Type: {parameter.Type.Name}");
                    Console.WriteLine($"    Value: {parameter.Value ?? "N/A"}");
                }
            }

            Console.WriteLine(new string('-', 50)); // Separator line
        }
    }

    private async Task LoadMacros(Engine engine)
    {
        Console.WriteLine("Loading macros...");

        var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
        query.EntityTypeFilter.Add(EntityType.Macro);
        await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
    }

    private async Task<Macro> CreateNewMacro(Engine engine)
    {
        Console.WriteLine("Creating a new macro...");

        IMacroSourceCodeBuilder builder = CodeBuilderFactory.Create();
        builder.AddProperty(typeof(Guid), "Entity");
        builder.AddProperty(typeof(bool), "Boolean");
        builder.AddProperty(typeof(string), "Text");
        builder.AddProperty(typeof(int), "Number");
        builder.SetClassAttributeParameters(singleInstance: true, keepRunningAfterExecute: true); // Single instance, keep running after execute
        builder.SetExecuteMethodContent(string.Empty); // This macro does not do anything, it just executes

        string macroSourceCode = builder.Build();

        Console.WriteLine($"Macro source code:\n\n{macroSourceCode}");

        Macro macro = await engine.TransactionManager.ExecuteTransactionAsync(() =>
        {
            var newMacro = (Macro)engine.CreateEntity("New Macro", EntityType.Macro);
            newMacro.SetSourceCode(macroSourceCode);
            return newMacro;
        });

        Console.WriteLine("New macro created successfully.");
        return macro;
    }

    private void ExecuteMacro(Macro macro)
    {
        Console.WriteLine($"Executing {macro.Name}");

        macro.Started += OnMacroOnStarted;
        macro.Completed += OnMacroOnCompleted;
        macro.Aborted += OnMacroOnAborted;

        var parameters = macro.DefaultParameters;
        parameters["Entity"].Value = Guid.NewGuid();
        parameters["Boolean"].Value = true;
        parameters["Text"].Value = "ABC";
        parameters["Number"].Value = 3;

        Guid instanceId = macro.Execute(parameters);
        Console.WriteLine($"{macro.Name} started. Instance ID: {instanceId}");

        void OnMacroOnStarted(object sender, MacroEventArgs e)
        {
            Console.WriteLine($"{macro.Name} started. Instance ID: {e.InstanceGuid}");
        }

        void OnMacroOnCompleted(object sender, MacroEventArgs e)
        {
            Console.WriteLine($"{macro.Name} completed. Instance ID: {e.InstanceGuid}");
            if (e.RuntimeException != null)
            {
                Console.WriteLine($"Runtime exception: {e.RuntimeException.Message}");
            }
        }

        void OnMacroOnAborted(object sender, MacroEventArgs e)
        {
            Console.WriteLine($"{macro.Name} aborted. Instance ID: {e.InstanceGuid}");
            if (e.RuntimeException != null)
            {
                Console.WriteLine($"Runtime exception: {e.RuntimeException.Message}");
            }
        }
    }

    private void DisplayRunningMacros(Engine engine)
    {
        Console.WriteLine("\nRunning macros...");

        foreach (MacroInstance instance in engine.GetMacroInstances())
        {
            Console.WriteLine($"Macro: {engine.GetEntity(instance.Entity).Name}");
            Console.WriteLine($"Execution ID: {instance.ExecutionId}");
            Console.WriteLine($"Start Time: {instance.StartTime}");
            Console.WriteLine($"Instigator: {engine.GetEntity(instance.Instigator)?.Name}");
            Console.WriteLine(new string('-', 30));
        }
    }

    private void AbortMacro(Engine engine, Macro macro)
    {
        MacroInstance instance = engine.GetMacroInstances().FirstOrDefault(i => i.Entity == macro.Guid);
        if (instance != null)
        {
            Console.WriteLine($"Aborting macro with instance ID: {instance.ExecutionId}");
            macro.AbortInstance(instance.ExecutionId);
        }
        else
        {
            Console.WriteLine("Macro is not running.");
        }
    }
}