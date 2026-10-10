# What is the Workspace SDK?

Use the Workspace SDK to extend Security Desk and Config Tool with custom tasks, panels, widgets, options, and other user interface components. These samples show how to build Workspace modules for those client applications.

## Prerequisites

- **.NET Framework 4.8.1 targeting pack**: the samples target `net481`. Workspace modules must target .NET Framework because Security Desk and Config Tool host them in-process on .NET Framework. Client modules targeting .NET 8 or .NET 10 are not supported.
- **Security Center SDK**: installed with the `GSC_SDK` environment variable configured.
- **Build tools**: Visual Studio 2022 version 17.8 or later, or the .NET 8 SDK or a later compatible SDK for command-line builds. The samples compile with C# 12.
- **Security Center**: the Security Desk and Config Tool client applications are installed.
- **Valid Security Center license**: all samples include the development SDK certificate.

## Building a sample

The Workspace SDK projects use `Debug` and `Release` configurations and target .NET Framework 4.8.1. Selecting `Debug_NET8` or `Release_NET8` in the solution still builds these projects for `net481`.

1. Open an elevated PowerShell terminal or run Visual Studio as an administrator. The samples' post-build registration writes to `HKEY_LOCAL_MACHINE`.
2. Set `GSC_SDK` to the installed SDK directory containing the .NET Framework assemblies. Replace the example path with your installation:

   ```powershell
   $env:GSC_SDK = 'C:\Program Files (x86)\Genetec Security Center 5.14 SDK'
   ```

3. From the repository root, build the PageTask sample:

   ```powershell
   dotnet build "Samples/Workspace SDK/PageTaskSample/PageTaskSample.csproj" -c Debug -f net481
   ```

4. Use `-c Release` for a release build, or replace the project path with another Workspace sample. Restart Security Desk or Config Tool to load the registered module.

A Workspace module is a class library loaded by a client application. Launch the client to use it. For registration and deployment, see [Development-time registration](#development-time-registration).

## Build and deployment dependencies

The samples reference `Genetec.Sdk.dll` and `Genetec.Sdk.Workspace.dll` through `GSC_SDK`. Some also reference `Genetec.Sdk.Controls.dll`. Their project files declare non-SDK packages as needed, including `Prism.Core`, `Newtonsoft.Json`, and `System.Resources.Extensions`; `dotnet build` restores these packages automatically.

Install the .NET Framework 4.8.1 runtime on workstations running these samples. Keep **Copy Local** set to `False` for SDK references because Security Desk and Config Tool supply the SDK assemblies at runtime. Deploy required non-SDK dependencies beside the registered module DLL. Use versions compatible with the client and other loaded modules; older packages are not necessarily compatible. See [Dependency resolution](#dependency-resolution-with-addfolderstoassemblyprobe-and-assemblyresolver) for probing and custom resolution.

## Overview

Workspace modules run inside Security Desk or Config Tool. These client applications load the modules to provide custom user interface components. Workspace modules are exclusively client-side extensions and do not run as server-side roles.

## Module requirements

Workspace modules have constructor, inheritance, registration, and dependency requirements.

### Module constructor requirements

Your Workspace module must have a **public parameterless constructor**. Security Center uses reflection to instantiate the module.

```csharp
public class SampleModule : Module
{
    // This default constructor is required (can be implicit)
    public SampleModule()
    {
        // Optional: initialization code here
    }
    
    // ... rest of your module code
}
```

If you don't explicitly define a constructor, C# provides an implicit default constructor, which satisfies this requirement.

### Module inheritance

Your workspace module must inherit from `Sdk.Workspace.Modules.Module`. This base class provides the framework for integrating with Security Center's client applications and handles the module lifecycle.

### Registration in `Load()`

The `Load()` method is called when Security Center starts and loads your module. Register all your user interface extensions, such as tasks, widgets, and options, with the workspace in this method. Registration identifies the components your module provides and makes them available to users.

### Application type checking

Security Center has multiple client applications, including Security Desk and Config Tool. Your module may need to behave differently in each. Use `Workspace.ApplicationType` to identify the running application and register only components appropriate for that application.

### Assembly resolution for dependencies

The static constructor with `AssemblyResolver.Initialize()` is only needed when your module depends on third-party libraries or custom assemblies that are not part of the Genetec SDK. The SDK assemblies are automatically resolved by Security Center.

### Shared process and AppDomain

At startup, each instance of Security Desk or Config Tool loads all registered Workspace modules into its Windows process and .NET AppDomain. The modules share that application's memory space and runtime environment. Dependency version conflicts and unhandled exceptions in one module can affect other modules in that application.

Module developers must account for these shared resources and possible failures:

- **Assembly version conflicts**: if Module A uses Newtonsoft.Json v10.0 and Module B uses v12.0, a binding configuration that selects an incompatible version for Module B can cause type loading errors.
- **Global state sharing**: modules using the same loaded type share that type's static variables and singletons.
- **Shared Engine instance**: All modules share the same Security Center Engine instance, with these consequences:

  - **Single Directory connection**: all modules use the same connection to the Directory Server.
  - **Shared entity cache**: changes made by one module to cached entities are immediately visible to all other modules.
  - **Common credentials**: all modules operate under the same user credentials used to log on to Config Tool or Security Desk.
  - **Unified privileges**: module operations are subject to the Security Center access rights and privileges of the user who is logged on.
- **Exception handling**: an unhandled exception in one module can potentially crash the entire Security Desk application, affecting all modules.
- **Performance impact**: a module that performs operations requiring high CPU or memory usage can affect the responsiveness of Security Desk and other modules.
- **Assembly loading**: once an assembly is loaded, it cannot be unloaded until the entire process shuts down.

Use compatible versions of third-party dependencies across your organization's modules. Avoid long-running operations on UI threads, handle exceptions, and test modules together to check their effect on Security Desk performance. Use `async` and `await` for I/O operations to avoid blocking the UI thread, and avoid lengthy operations in `Load()`.

## Module lifecycle and resource management

Use the workspace module lifecycle to plan resource initialization and cleanup.

### Lifecycle events

1. **Constructor**: runs when Security Center instantiates your module; the constructor must be parameterless.
2. **Initialize()**: the framework calls this method to provide the Workspace instance.
3. **Load()**: Security Center calls this method when loading your module; register components here.
4. **Unload()**: Security Center calls this method during shutdown; clean up resources here.

## Creating a Workspace module

Create a class that inherits from `Sdk.Workspace.Modules.Module` and override the `Load()` and `Unload()` methods. Register your extensions in `Load()` based on the application type.

### Example: basic task registration

This example uses [NotepadTask.cs](TaskSample/NotepadTask.cs) from [TaskSample](TaskSample/). The project includes the task's image resources and registration.

```csharp
using Genetec.Sdk;
using Genetec.Sdk.Workspace.Modules;

namespace Genetec.Dap.CodeSamples
{
    public class SampleModule : Module
    {
        public override void Load()
        {
            if (Workspace.ApplicationType is ApplicationType.SecurityDesk or ApplicationType.ConfigTool)
            {
                var task = new NotepadTask();
                task.Initialize(Workspace);
                Workspace.Tasks.Register(task);
            }
        }

        public override void Unload()
        {
        }
    }
}
```

### Example: application-specific registration

This example is incomplete. `CustomWidgetBuilder` and `ConfigPageTask` are placeholders for classes you must supply. Replace them with your own widget builder and Config Tool task before compiling.

```csharp
using Genetec.Sdk;
using Genetec.Sdk.Workspace.Modules;

namespace Genetec.Dap.CodeSamples
{
    public class SampleModule : Module
    {
        public override void Load()
        {
            switch (Workspace.ApplicationType)
            {
                case ApplicationType.SecurityDesk:
                    RegisterSecurityDeskComponents();
                    break;
                case ApplicationType.ConfigTool:
                    RegisterConfigToolComponents();
                    break;
            }
        }

        private void RegisterSecurityDeskComponents()
        {
            // Register Security Desk specific components
            var widget = new CustomWidgetBuilder();
            widget.Initialize(Workspace);
            Workspace.Components.Register(widget);
        }

        private void RegisterConfigToolComponents()
        {
            // Register Config Tool specific components
            var task = new ConfigPageTask();
            task.Initialize(Workspace);
            Workspace.Tasks.Register(task);
        }

        public override void Unload()
        {
        }
    }
}
```

### Example: options extension registration

Use [OptionsExtensionSample](OptionsExtensionSample/) as the complete project for this example. It supplies [SampleOptionsExtensions.cs](OptionsExtensionSample/SampleOptionsExtensions.cs), its option page, settings serialization, and resources. The example also uses [AssemblyResolver.cs](../Shared/AssemblyResolver.cs) for non-SDK dependencies.

```csharp
using Genetec.Sdk;
using Genetec.Sdk.Workspace.Modules;

namespace Genetec.Dap.CodeSamples
{
    public class SampleModule : Module
    {
        // Only needed if you have non-SDK dependencies
        static SampleModule() => AssemblyResolver.Initialize();

        public override void Load()
        {
            if (Workspace.ApplicationType == ApplicationType.SecurityDesk)
            {
                var extensions = new SampleOptionsExtensions();
                extensions.Initialize(Workspace);
                Workspace.Options.Register(extensions);
            }
        }

        public override void Unload()
        {
        }
    }
}
```

## Component registration types

Workspace modules can register these component types:

### Tasks

```csharp
var task = new CustomTask();
task.Initialize(Workspace);
Workspace.Tasks.Register(task);
```

### Widgets and component builders

```csharp
var builder = new CustomWidgetBuilder();
builder.Initialize(Workspace);
Workspace.Components.Register(builder);
```

### Options extensions

```csharp
var options = new CustomOptionsExtensions();
options.Initialize(Workspace);
Workspace.Options.Register(options);
```

## Dependency resolution with AddFoldersToAssemblyProbe and AssemblyResolver

Workspace modules can resolve non-SDK dependencies with `AddFoldersToAssemblyProbe` or a custom resolver such as the sample `AssemblyResolver`.

### AddFoldersToAssemblyProbe

This is a Security Center-specific feature configured during module registration.

This registration option affects dependency resolution as follows:

- Set it in the registration XML file: `<Item Key="AddFoldersToAssemblyProbe" Value="True" />`.
- For legacy registry registration, use `AddFoldersToAssemblyProbe = True`.
- Security Center adds the registered module directories to an additional dependency lookup.
- The additional lookup uses `AppDomain.AssemblyResolve` and loads a DLL only when its complete assembly identity matches the requested identity.

Use automatic probing when the following conditions apply:

- Your module's dependencies are DLLs that need to be found.
- Dependencies do not require special loading logic.
- You want Security Center to handle resolution automatically.

**Example:**
```xml
<PluginInstallation>
  <Version>2</Version>
  <Configuration>
    <Item Key="Enabled" Value="True" />
    <Item Key="ClientModule" Value="C:\MyModule\MyModule.dll" />
    <Item Key="AddFoldersToAssemblyProbe" Value="True" />
  </Configuration>
</PluginInstallation>
```

This registration adds `C:\MyModule\` to the additional dependency lookup. A dependency in that directory can be loaded through this lookup only when its complete assembly identity matches the requested identity.

### Custom assembly resolution with AssemblyResolver

The sample `AssemblyResolver` implements custom dependency loading through the .NET `AppDomain.AssemblyResolve` event.

The sample resolver handles dependency loading as follows:

- Calling `AssemblyResolver.Initialize()` registers a handler for the `AppDomain.AssemblyResolve` event.
- When .NET cannot resolve an assembly, it calls the handler.
- The handler looks for a DLL with the requested assembly name in the resolver's own assembly directory and loads it if present.

A custom resolver can address these requirements; adapt the sample helper if needed:

- You need custom logic for loading assemblies, such as version selection or conditional loading.
- Dependencies are located in other directories.
- You need to load assemblies from embedded resources.
- You need fallback loading strategies.
- `AddFoldersToAssemblyProbe` is not sufficient for your needs.

**Example:**
```csharp
public class SampleModule : Module
{
    static SampleModule() => AssemblyResolver.Initialize();
    
    // The AssemblyResolver handles complex loading scenarios
}
```

### Choosing a resolution mechanism

Start with `AddFoldersToAssemblyProbe` when Security Center can resolve your module's dependencies automatically without custom code. Use a custom resolver when automatic probing is insufficient, dependencies are in other locations, or you need conditional loading or version selection. Adapt the sample `AssemblyResolver` if you need behavior beyond loading DLLs from its own assembly directory.

### Using both mechanisms

You can use both mechanisms. When `AddFoldersToAssemblyProbe=True` resolves your module's dependencies, no custom resolver is needed. Add a custom resolver only when you need additional loading behavior.

The sample resolver loads DLLs from its own assembly's directory. Searching other locations or implementing different loading rules requires adapting its implementation.

Only implement assembly resolution if your module uses third-party or custom libraries beyond the Genetec SDK.

If you choose to use the sample resolver:

1. Place all third-party DLLs in the same directory as your Workspace module DLL.
2. Register an assembly resolver in a static constructor.
3. Use the provided `AssemblyResolver` class from the samples.

```csharp
public class SampleModule : Module
{
    // Only add this if you have non-SDK dependencies
    static SampleModule() => AssemblyResolver.Initialize();
    
    // ... rest of your module code
}
```

Security Center automatically resolves the Genetec SDK assemblies. Do not attempt to resolve them manually.

## Development-time registration

For development and testing, workspace modules need to be registered with Security Center. The SDK samples include post-build steps that automatically register modules during development:

```xml
<Target Name="PostBuild" AfterTargets="PostBuildEvent">
  <Exec Command="REG ADD &quot;HKEY_LOCAL_MACHINE\SOFTWARE\Genetec\Security Center\Plugins\$(ProjectName)&quot; /v Enabled /t REG_SZ /d &quot;True&quot; /f
REG ADD &quot;HKEY_LOCAL_MACHINE\SOFTWARE\Genetec\Security Center\Plugins\$(ProjectName)&quot; /v ClientModule /t REG_SZ /d &quot;$(TargetPath)&quot; /f
REG ADD &quot;HKEY_LOCAL_MACHINE\SOFTWARE\Genetec\Security Center\Plugins\$(ProjectName)&quot; /v AddFoldersToAssemblyProbe /t REG_SZ /d &quot;True&quot; /f

REG ADD &quot;HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Genetec\Security Center\Plugins\$(ProjectName)&quot; /v Enabled /t REG_SZ /d &quot;True&quot; /f
REG ADD &quot;HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Genetec\Security Center\Plugins\$(ProjectName)&quot; /v ClientModule /t REG_SZ /d &quot;$(TargetPath)&quot; /f
REG ADD &quot;HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Genetec\Security Center\Plugins\$(ProjectName)&quot; /v AddFoldersToAssemblyProbe /t REG_SZ /d &quot;True&quot; /f" />
</Target>
```

Register the `ClientModule` on each workstation that runs your extension in Security Desk or Config Tool. For deployment on Security Center 5.13 or later, use a `*.Plugin.xml` file as described in [Deploying plugins and Workspace modules](https://github.com/Genetec/DAP/wiki/plugin-sdk-deployment). Registry registration remains available for development and older deployments.

The post-build registration writes to `HKEY_LOCAL_MACHINE`, so it requires administrative privileges. Run Visual Studio as an administrator when building samples that include this post-build target.

## Module requirements during development

Plan registration, dependency compatibility, resource cleanup, logging, and testing when developing a Workspace module.

### Application type checking

Check `Workspace.ApplicationType` before registering components. Register only components used by the running application.

### Dependency management

Workspace modules loaded by the same client application share a process and AppDomain, so coordinate their dependencies.

#### Shared AppDomain

Workspace modules loaded by the same client application share its process and AppDomain. Their dependencies can conflict. The version used by a module depends on assembly identity and the client's binding configuration. See [How the runtime locates assemblies](https://learn.microsoft.com/en-us/dotnet/framework/deployment/how-the-runtime-locates-assemblies).

The following example illustrates a possible conflict when binding settings select a dependency version incompatible with another module:

```text
Security Desk Process
├── Module A (loads Newtonsoft.Json v10.0.3)
├── Module B (tries to load Newtonsoft.Json v12.0.1) FAILS
└── Module C (expects Newtonsoft.Json v11.0.2) FAILS
```

If the client resolves Modules B and C to Module A's Newtonsoft.Json v10.0.3 and their required APIs are incompatible, this can cause:

- `TypeLoadException` if the API has changed.
- `MissingMethodException` if methods were added or removed.
- Runtime behavior differences if internal logic changed.

#### Dependency coordination

- If you develop multiple modules, maintain a dependency matrix specifying versions for all your organization's modules.
- Select dependency versions compatible with the Security Center client and other loaded modules. Check the APIs and assembly versions required by each component before updating packages.
- Reduce third-party dependencies where possible; each dependency can introduce a conflict.
- Test your modules with other Workspace modules that will be deployed in the same environment.

#### Detecting dependency conflicts

Use these checks to detect dependency conflicts:

- Use `dotnet list package` to audit dependencies across projects.
- Implement integration tests that load multiple modules together.

### Error handling

Handle errors in `Load()` to prevent failures when loading the module. Consider try-catch blocks around registration calls for components that are not critical.

### Resource management

Subscribe to events in `Load()`. In `Unload()`, unsubscribe from events, dispose of loggers, and release acquired resources to prevent resource leaks. Because `Unload()` may not run during application shutdown, plan cleanup for that possibility as well.

### Logging and diagnostics

Use the SDK's `Logger` class for logging, and dispose of logger instances in `Unload()`. Consider methods with `[DebugMethod]` attributes for runtime diagnostics. To debug a module, attach Visual Studio's debugger to the Security Desk or Config Tool process running it. Check Security Center logs for error details.

### Testing

Test your module with every Security Center version you support and with other Workspace modules. Repeat compatibility checks after Security Center upgrades. Confirm that required dependencies are deployed in the correct directory, the workstation has the required .NET Framework runtime, and dependency versions are compatible with other loaded modules.

## Registration examples

These examples register page tasks, dashboard widgets, and custom actions.

### Page tasks

```csharp
var pageTask = new CreatePageTask<CustomPage>();
pageTask.Initialize(Workspace);
Workspace.Tasks.Register(pageTask);
```

### Dashboard widgets

```csharp
var widgetBuilder = new CustomWidgetBuilder();
widgetBuilder.Initialize(Workspace);
Workspace.Components.Register(widgetBuilder);
```

### Custom actions

```csharp
var actionBuilder = new CustomActionBuilder();
actionBuilder.Initialize(Workspace);
Workspace.Components.Register(actionBuilder);
```

## Workspace modules and plugins

Choose a workspace module for client-side user interface extensions and a plugin for server-side functionality.

### Workspace modules

- **Execution location**: run inside Security Desk or Config Tool as client extensions.
- **Purpose**: extend the user interface with custom tasks, panels, widgets, and options.
- **Capabilities**: 
  - Create custom UI components
  - Add menu items and tasks
  - Extend existing Security Center pages
- **Limitations**:
  - Cannot run server-side logic
  - Cannot create custom roles
  - No custom database support
- **Use cases**: 
  - Custom dashboards or widgets  
  - Specialized reporting tools
  - Third-party system integrations that only need UI components
  - Custom configuration pages

### Plugins as custom roles

- **Execution location**: run on the Security Center server as custom roles.
- **Purpose**: extend Security Center's server-side functionality.
- **Capabilities**:
  - Support custom database
  - Failover support
  - Include optional client-side components (workspace modules)
- **Use cases**:
  - Custom access control integrations
  - Third-party system synchronization
  - Custom events and alarms processing
  - Background data processing tasks

### Choosing a module or plugin

Choose the component based on whether the integration needs a client user interface or server processing.

Choose a Workspace module for these requirements:

- Custom UI components only
- Client-side data visualization
- Custom reporting interfaces

Choose a plugin for these requirements:

- Server-side processing
- Database access
- Background services
- Custom business logic that runs independently of UI

A plugin can process data and apply business logic while a Workspace module provides the user interface for configuration and monitoring.
