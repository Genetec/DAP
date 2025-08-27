# What is the Workspace SDK?

The Workspace SDK is a development framework that allows you to extend Security Center's client applications (Security Desk and Config Tool) with custom user interface components. It enables developers to create seamless integrations that feel like native parts of the Security Center experience.
This guide demonstrates how to build a Workspace module for Security Center. Workspace modules allow you to extend Security Desk and Config Tool with custom UI tasks, panels, widgets, options, and other components.

## Overview

Workspace modules run inside the Security Center client applications (Security Desk or Config Tool). They are loaded by these applications and provide custom user interface functionality. Unlike plugins, Workspace modules do not run as server-side Roles and are intended exclusively for client-side extensions.

## Key Concepts

### Module Class Requirements
Your workspace module must have a **default public constructor** (parameterless constructor). Security Center uses reflection to instantiate your module, and it requires a constructor that takes no parameters.

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
### Module Inheritance
Your workspace module must inherit from `Sdk.Workspace.Modules.Module`. This base class provides the framework for integrating with Security Center's client applications and handles the module lifecycle.

### Registration in Load() Method
The `Load()` method is called when Security Center starts up and loads your module. This is where you register all your UI extensions (tasks, widgets, options, etc.) with the workspace. Registration tells Security Center what components your module provides and makes them available to users.

### Application Type Checking
Security Center has multiple client applications (Security Desk and Config Tool), and your module may need to behave differently in each. Use `Workspace.ApplicationType` to determine which application is currently running your module and register only appropriate components for that context.

### Assembly Resolution for Dependencies
The static constructor with `AssemblyResolver.Initialize()` is only needed when your module depends on third-party libraries or custom assemblies that are not part of the Genetec SDK. The SDK assemblies are automatically resolved by Security Center.

### Shared Process and AppDomain Architecture
All workspace modules loaded by Security Desk (or Config Tool) run within the same Windows process and share the same .NET AppDomain. This has several important implications:

**What this means:**
- When Security Desk or Config Tool starts, it loads ALL registered workspace modules into its single process
- All modules share the same memory space and runtime environment
- Modules can potentially interfere with each other if not designed carefully

**Implications for developers:**
- **Assembly version conflicts**: If Module A uses Newtonsoft.Json v10.0 and Module B uses v12.0, only the first version loaded will be used. The second module may fail with type loading errors
- **Global state sharing**: Static variables and singletons are shared across all modules
- **Shared Engine instance**: All modules share the same Security Center Engine instance, which means:
  - **Single Directory connection**: All modules use the same connection to the Directory Server
  - **Shared entity cache**: Changes made by one module to cached entities are immediately visible to all other modules
  - **Common credentials**: All modules operate under the same user credentials that were used to log into Config Tool or Security Desk
  - **Unified privileges**: Module operations are subject to the logged-in user's access rights and privileges in Security Center
- **Exception handling**: An unhandled exception in one module can potentially crash the entire Security Desk application, affecting all modules
- **Performance impact**: A poorly performing module (CPU/memory intensive operations) can affect the responsiveness of Security Desk and other modules
- **Assembly loading**: Once an assembly is loaded, it cannot be unloaded until the entire process shuts down

**Best practices:**
- Use compatible versions of third-party dependencies across all your organization's modules
- Avoid long-running operations in UI threads
- Implement proper exception handling to prevent crashes
- Test modules together, not just individually
- Consider the impact of your module on the overall Security Desk performance

## Prerequisites

* .NET Framework 4.8.1
* Genetec Security Center SDK installed (creates required environment variables)
  * `GSC_SDK`: Points to SDK location for .NET Framework
* Visual Studio 2022

## Module Lifecycle and Resource Management

Understanding the workspace module lifecycle is crucial for proper resource management.

### Lifecycle Events

1. **Constructor**: Runs when Security Center instantiates your module (must be parameterless)
2. **Initialize()**: Called by the framework to provide the Workspace instance
3. **Load()**: Called when Security Center loads your module - register all components here
4. **Unload()**: Called when Security Center shuts down - clean up resources here

## Creating a Workspace Module

Create a class that inherits from `Sdk.Workspace.Modules.Module` and override the `Load()` and `Unload()` methods. Register your extensions in `Load()` based on the application type.

### Example: Basic Task Registration

```csharp
using Sdk;
using Sdk.Workspace.Modules;

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

### Example: Application-Specific Registration

```csharp
using Sdk;
using Sdk.Workspace.Modules;

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

### Example: Options Extension Registration

```csharp
using Sdk;
using Sdk.Workspace.Modules;

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

## Component Registration Types

Workspace modules can register various types of components:

### Tasks
```csharp
var task = new CustomTask();
task.Initialize(Workspace);
Workspace.Tasks.Register(task);
```

### Components (Widgets, Builders)
```csharp
var builder = new CustomWidgetBuilder();
builder.Initialize(Workspace);
Workspace.Components.Register(builder);
```

### Options Extensions
```csharp
var options = new CustomOptionsExtensions();
options.Initialize(Workspace);
Workspace.Options.Register(options);
```

## Dependency Resolution: AddFoldersToAssemblyProbe vs AssemblyResolver

There are two mechanisms for resolving non-SDK dependencies in workspace modules, and understanding when to use each is important.

### AddFoldersToAssemblyProbe

This is a Security Center-specific feature configured during module registration.

**How it works:**
- Set in the registration XML file: `<Item Key="AddFoldersToAssemblyProbe" Value="True" />`
- Or in registry (legacy): `AddFoldersToAssemblyProbe = True`
- Security Center automatically configures .NET's private probing paths to include your module's directory
- Works at the AppDomain level, affecting assembly resolution for the entire application

**When to use:**
- Your module has simple dependencies (DLLs that just need to be found)
- Dependencies don't require special loading logic
- You want Security Center to handle the resolution automatically
- Most common scenario for workspace modules

**Example:**
```xml
<PluginInstallation>
  <Version>1</Version>
  <Configuration>
    <Item Key="Enabled" Value="True" />
    <Item Key="ClientModule" Value="C:\MyModule\MyModule.dll" />
    <Item Key="AddFoldersToAssemblyProbe" Value="True" />
  </Configuration>
</PluginInstallation>
```

Your dependencies in `C:\MyModule\` will be found automatically.

### AssemblyResolver (Custom Assembly Resolution)

This is a .NET mechanism that you implement in code.

**How it works:**
- You register a custom handler for the `AppDomain.AssemblyResolve` event
- When .NET cannot find an assembly, your handler is called
- Your code decides how to locate and load the assembly
- Provides full control over the loading process

**When to use:**
- You need custom logic for loading assemblies (version selection, conditional loading, etc.)
- Dependencies are located in non-standard locations
- You need to load assemblies from embedded resources
- You want to implement fallback loading strategies
- AddFoldersToAssemblyProbe is not sufficient for your needs

**Example:**
```csharp
public class SampleModule : Module
{
    static SampleModule() => AssemblyResolver.Initialize();
    
    // The AssemblyResolver handles complex loading scenarios
}
```

### Which Should You Use?

**Start with AddFoldersToAssemblyProbe** because:
- Simpler to configure (no code required)
- Handled by Security Center automatically
- Works for 95% of scenarios
- Less prone to errors

**Use AssemblyResolver when:**
- AddFoldersToAssemblyProbe doesn't work for your scenario
- You need custom loading logic
- Dependencies are in multiple locations
- You need version-specific loading behavior

### Can You Use Both?

**No, typically you choose one approach:**
- If you set `AddFoldersToAssemblyProbe=True`, Security Center will usually find your dependencies automatically
- If that's not sufficient, implement a custom AssemblyResolver
- Using both can lead to confusion about which mechanism is resolving which assemblies

**Important**: Only implement assembly resolution if your module uses third-party or custom libraries beyond the Genetec SDK.

If your module uses non-SDK dependencies:

1. Place all third-party DLLs in the same directory as your workspace module DLL
2. Register an assembly resolver in a static constructor
3. Use the provided `AssemblyResolver` class from the samples

```csharp
public class SampleModule : Module
{
    // Only add this if you have non-SDK dependencies
    static SampleModule() => AssemblyResolver.Initialize();
    
    // ... rest of your module code
}
```

**Note**: The Genetec SDK assemblies are automatically resolved by Security Center. Do not attempt to resolve them manually.

## Development-Time Registration

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

**Important**: This registry-based approach is for development only. For production deployment, Security Center 5.13+ uses XML configuration files instead of registry entries. See the separate "Deploying Plugins and Workspace Modules" guide for complete production deployment instructions.

## Best Practices

### Application Type Checking
* Always check `Workspace.ApplicationType` before registering components
* Only register components appropriate for the current application
* Use specific checks rather than registering everything everywhere

### Dependency Management

#### Shared AppDomain
All workspace modules share the same process and AppDomain. This creates a unique challenge not found in typical application development.

**What happens with version conflicts:**
```
Security Desk Process
├── Module A (loads Newtonsoft.Json v10.0.3)
├── Module B (tries to load Newtonsoft.Json v12.0.1) FAILS
└── Module C (expects Newtonsoft.Json v11.0.2) FAILS
```

When Module A loads first, its version of Newtonsoft.Json becomes the "winning" version. Modules B and C will be forced to use v10.0.3, which may cause:
- `TypeLoadException` if the API has changed
- `MissingMethodException` if methods were added/removed
- Runtime behavior differences if internal logic changed

#### Coordination Strategies

1. **Organization-wide dependency management**: If you develop multiple modules, maintain a shared dependency matrix specifying which versions to use across all modules

2. **Conservative versioning**: Use older, stable versions of dependencies rather than the latest versions to maximize compatibility

3. **Minimal dependencies**: Reduce third-party dependencies where possible. Each dependency is a potential conflict point

4. **Testing with other modules**: Always test your modules alongside other workspace modules that will be deployed in the same environment

#### Detection and Prevention

- Use tools like `dotnet list package` to audit dependencies across projects
- Implement integration tests that load multiple modules together

### Error Handling
* Implement proper error handling in `Load()` to prevent module loading failures
* Consider wrapping registration calls in try-catch blocks for non-critical components

### Resource Management
* **Clean up resources in `Unload()`**: Always dispose of loggers, unsubscribe from events, and release any resources your module acquired
* **Event subscription management**: Subscribe to events in `Load()` and unsubscribe in `Unload()` to prevent memory leaks
* Be mindful that `Unload()` may not always be called during application shutdown, so design for graceful degradation

### Logging and Diagnostics
* **Use SDK logging**: Implement proper logging using the Genetec SDK's `Logger` class for consistency with Security Center's logging infrastructure
* **Dispose loggers**: Always dispose logger instances in your `Unload()` method to prevent resource leaks
* **Diagnostic methods**: Consider implementing debug methods with `[DebugMethod]` attributes for troubleshooting

### Testing
* Test your module on all supported Security Center versions
* Verify compatibility when Security Center is upgraded
* Test with other workspace modules to ensure no conflicts

## Common Registration Patterns

### Page Tasks
```csharp
var pageTask = new CreatePageTask<CustomPage>();
pageTask.Initialize(Workspace);
Workspace.Tasks.Register(pageTask);
```

### Dashboard Widgets
```csharp
var widgetBuilder = new CustomWidgetBuilder();
widgetBuilder.Initialize(Workspace);
Workspace.Components.Register(widgetBuilder);
```

### Custom Actions
```csharp
var actionBuilder = new CustomActionBuilder();
actionBuilder.Initialize(Workspace);
Workspace.Components.Register(actionBuilder);
```

## Module vs Plugin Distinction

Understanding the difference between workspace modules and plugins is crucial for choosing the right approach for your integration.

### Workspace Modules
- **Execution location**: Run inside Security Desk or Config Tool (client-side only)
- **Purpose**: Extend the user interface with custom tasks, panels, widgets, and options
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

### Plugins (Custom Roles)
- **Execution location**: Run on the Security Center server as custom roles
- **Purpose**: Extend Security Center's server-side functionality
- **Capabilities**:
  - Support custom database
  - Failover support
  - Include optional client-side components (workspace modules)
- **Use cases**:
  - Custom access control integrations
  - Third-party system synchronization
  - Custom events and alarms processing
  - Background data processing tasks

### Decision Matrix

**Choose Workspace Module when you need:**
- Custom UI components only
- Client-side data visualization
- Custom reporting interfaces

**Choose Plugin when you need:**
- Server-side processing
- Database access
- Background services
- Custom business logic that runs independently of UI

**Hybrid Approach:**
Many integrations use both: a plugin for server-side logic and a workspace module for the user interface. The plugin handles data processing and business logic, while the workspace module provides the UI for configuration and monitoring.

## Troubleshooting
* Check Security Center logs for detailed error messages
* Verify all dependencies are in the correct directory
* Ensure .NET Framework version compatibility
* Check for conflicting assembly versions with other modules

### Performance Issues
* Use async/await for I/O operations to avoid blocking the UI thread
* Avoid heavy operations in the `Load()` method

### Debugging Tips
* Use the SDK's logging framework for consistent logging with Security Center
* Implement debug methods with `[DebugMethod]` attributes for runtime diagnostics
* Test with multiple modules loaded to identify interaction issues
* Use Visual Studio's debugger by attaching to the Security Desk or Config Tool process
