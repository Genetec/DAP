// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk.Queries;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Scripting.Compiler;
using Genetec.Sdk.Scripting.Compiler.CodeBuilder;

const string server = "localhost";
const string username = "admin";
const string password = "";

SdkResolver.Initialize();

using var engine = new Engine();

ConnectionStateCode state = await engine.LoginManager.LogOnAsync(server, username, password);

if (state == ConnectionStateCode.Success)
{
    await LoadMacrosIntoEntityCache();

    List<Macro> macros = engine.GetEntities(EntityType.Macro).OfType<Macro>().ToList();
    DisplayMacros(macros);

    Macro macro = await CreateNewMacro();
    ExecuteMacro(macro);

    DisplayRunningMacros();

    await Task.Delay(2000); // Wait for 2 seconds

    AbortMacroIfRunning(macro);
}
else
{
    Console.WriteLine($"Login failed with state: {state}");
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

void DisplayMacros(List<Macro> macros)
{
    Console.WriteLine($"{macros.Count} macros loaded");

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

async Task LoadMacrosIntoEntityCache()
{
    Console.WriteLine("Loading macros...");

    var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
    query.EntityTypeFilter.Add(EntityType.Macro);
    await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
}

async Task<Macro> CreateNewMacro()
{
    IMacroSourceCodeBuilder builder = CodeBuilderFactory.Create();
    builder.AddProperty(typeof(Guid), "Entity");
    builder.AddProperty(typeof(bool), "Boolean");
    builder.AddProperty(typeof(string), "Text");
    builder.AddProperty(typeof(int), "Number");

    string macroSourceCode = builder.Build();

    Macro macro = await engine.TransactionManager.ExecuteTransactionAsync(() =>
    {
        var newMacro = (Macro)engine.CreateEntity("New Macro", EntityType.Macro);
        newMacro.SetSourceCode(macroSourceCode);
        return newMacro;
    });

    Console.WriteLine("New macro created successfully.");
    return macro;
}

void ExecuteMacro(Macro macro)
{
    macro.Started += OnMacroOnStarted;
    macro.Completed += OnMacroOnCompleted;
    macro.Aborted += OnMacroOnAborted;

    var parameters = macro.DefaultParameters;
    parameters["Entity"].Value = Guid.NewGuid();
    parameters["Boolean"].Value = true;
    parameters["Text"].Value = "ABC";
    parameters["Number"].Value = 3;

    Guid instanceId = macro.Execute(parameters);
    Console.WriteLine($"Executing the new macro. Instance ID: {instanceId}");
}

void DisplayRunningMacros()
{
    Console.WriteLine("\nGetting running macros...");

    foreach (MacroInstance instance in engine.GetMacroInstances())
    {
        Console.WriteLine($"Macro: {engine.GetEntity(instance.Entity).Name}");
        Console.WriteLine($"Execution ID: {instance.ExecutionId}");
        Console.WriteLine($"Start Time: {instance.StartTime}");
        Console.WriteLine($"Instigator: {engine.GetEntity(instance.Instigator)?.Name}");
        Console.WriteLine(new string('-', 30));
    }
}

void AbortMacroIfRunning(Macro macro)
{
    MacroInstance instance = engine.GetMacroInstances().FirstOrDefault(i => i.Entity == macro.Guid);
    if (instance != null)
    {
        Console.WriteLine("Aborting macro...");
        macro.AbortInstance(instance.ExecutionId);
    }
    else
    {
        Console.WriteLine("Macro is not running.");
    }
}

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


