# Basic Plugin Template for Security Center

This sample project contains a basic plugin template for Genetec Security Center.
It demonstrates the fundamental structure and class required to create a plugin for Security Center.

## Understanding Roles in Security Center

Before diving into plugin development, it's important to understand the concept of Roles in Security Center, as plugins function as custom roles within the system.

### What are Roles?

In Security Center, Roles are components that perform specific tasks within the system. Each role is associated with one or more servers, which host and execute the role's functions. Roles are essential for various operations such as managing video units, archiving data, or synchronizing users with corporate directories.

### Core Features of Roles

1. **Role Type:** Each role has a defined type that determines its specific functions. For example, a role could be responsible for managing video units and their associated archives.

2. **Role Settings:** These settings specify the parameters within which the role operates, such as data retention periods or database configurations.

3. **Server Assignment:** Roles can be assigned to one or multiple servers. This allows for load balancing and failover capabilities.

4. **Failover Support:** Roles support failover, meaning they can automatically switch to a secondary server if the primary server fails, ensuring continuous operation of critical system functions.

### Plugins as Custom Roles

When you develop a plugin using the Plugin SDK, you're essentially creating a custom role within Security Center. Your plugin will inherit many of the core features of roles, including:

- The ability to define a specific role type with custom functionality
- Configurable settings for your plugin's operation
- Automatic server assignment and failover support
- Built-in health monitoring and database support

This architecture allows your plugin to seamlessly integrate with Security Center's existing infrastructure and benefit from its robust server-side capabilities.

## Overview of the Plugin SDK

The Plugin SDK for Genetec Security Center allows technology partners to create advanced, fully embedded integrations that function as custom roles within the system. Here's what you need to know:

### Capabilities
- Develop custom role entities similar to built-in roles like Archiver and Access Manager.
- Benefit from built-in server-side features, which are automatically inherited:
  - Failover support
  - Health monitoring (status, history, and statistics)
  - Database support
- Integrate with front-end components (Config Tool and Security Desk) for a seamless user experience.

### Key Benefits
- Full embedding within Security Center as a custom role
- Automatic inclusion of robust server-side functionality
- Seamless integration with Security Center's user interface
- Reduced development time due to inherited role features

### Important Requirements
1. **SDK Support Plan**: Creating a Security Center plugin requires purchasing a Gold SDK support plan.
2. **Licensing**: A specific part number must be included in the end-user's Security Center license to enable plugin functionality.

By using the Plugin SDK, developers can create powerful, deeply integrated solutions that extend Security Center's capabilities while leveraging built-in role features and maintaining consistency with the core product.


## Prerequisites

- .NET Framework 4.8.1
- Genetec Security Center SDK
- Visual Studio 2022

## Getting Started

1.  Install the latest Genetec Security Center SDK. This will create the necessary environment variables automatically.
   -   `GSC_SDK`: Points to the location of the Genetec SDK for .NET Framework 4.8.1
   -   `GSC_SDK_CORE`: Points to the location of the Genetec SDK for .NET 8
3.  Clone this repository to your local machine.
4.  Open the solution in Visual Studio **as an Administrator**.
5.  Build the BasicPluginTemplate project.

## Creating a Plugin

To create a plugin for Security Center, follow these steps:

1. Create a new class that inherits from the `Plugin` class found in `Genetec.Sdk.Plugin.dll`.
2. Ensure your plugin class has a default constructor.
3. Add the `PluginProperty` attribute to your class, specifying a class that inherits from `PluginDescriptor`.
4. Implement the required abstract members in both the plugin class and the PluginDescriptor class.
5. Ensure that your PluginDescriptor class defines a unique PluginGuid for your plugin.

Here's a more detailed example of how your plugin classes should be structured:

```csharp
using System;
using Genetec.Sdk.Plugin;

[PluginProperty(typeof(YourPluginDescriptor))]
public class YourPlugin : Plugin
{
    public YourPlugin()
    {
        // Default public constructor
    }

    // Implement abstract members
    protected override void OnPluginStart()
    {
        // Plugin startup logic
    }

    protected override void OnPluginLoaded()
    {
        // Plugin loaded logic
    }

    protected override void OnQueryReceived(ReportQueryReceivedEventArgs args)
    {
        // Handle queries
    }

    // Other overrides and custom methods as needed
}

public class YourPluginDescriptor : PluginDescriptor
{
    // Implement abstract properties
    public override string Name => "Your Plugin Name";
    public override string Description => "Description of your plugin";
    public override Guid PluginGuid => new Guid("YOUR-UNIQUE-GUID-HERE");

    // Override other virtual properties as needed
}
```

### Important Notes:

1. **Abstract Members in Plugin Class:**
   - `OnPluginLoaded()`: Called when the plugin is loaded. Use this for any setup that needs to happen before the plugin starts.
   - `OnPluginStart()`: Called when the plugin is started. Use this method to initialize your plugin's resources and start any background tasks.
   - `OnQueryReceived(ReportQueryReceivedEventArgs args)`: Called when the plugin receives a query. Implement this to handle any native or custom queries your plugin supports.
   - `Dispose(bool disposing)`: Called when the plugin is deactivated. Call this method to clean up any resources used by your plugin.
2. 
1. **Abstract Properties in PluginDescriptor Class:**
   - `Name`: The name of your plugin as it will appear in Security Center.
   - `Description`: A brief description of your plugin's functionality.
   - `PluginGuid`: A unique identifier for your plugin. This must be unique across all plugins.

3. **Unique PluginGuid:**
   - Each plugin must have its own unique `PluginGuid`. This GUID is used by Security Center to identify and manage your plugin.
   - Generate a new GUID for each plugin you create. You can use tools like Visual Studio's Create GUID tool or online GUID generators.
   - Never reuse a `PluginGuid` from another plugin, as this can cause conflicts in Security Center.

Remember to replace `YourPlugin`, `YourPluginDescriptor`, and `"YOUR-UNIQUE-GUID-HERE"` with appropriate names and a unique GUID for your plugin.

## Post-Build Process

The project file includes a post-build event that automatically registers the plugin with Security Center on the development machine. This process is important for testing and using your plugin.

### Post-Build Event Details

Here's the post-build event command from the project file:

```
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\Genetec\Security Center\Plugins\$(ProjectName)" /v Enabled /t REG_SZ /d "True" /f
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\Genetec\Security Center\Plugins\$(ProjectName)" /v ServerModule /t REG_SZ /d "$(TargetPath)" /f
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\Genetec\Security Center\Plugins\$(ProjectName)" /v AddFoldersToAssemblyProbe /t REG_SZ /d "True" /f

REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Genetec\Security Center\Plugins\$(ProjectName)" /v Enabled /t REG_SZ /d "True" /f
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Genetec\Security Center\Plugins\$(ProjectName)" /v ServerModule /t REG_SZ /d "$(TargetPath)" /f
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Genetec\Security Center\Plugins\$(ProjectName)" /v AddFoldersToAssemblyProbe /t REG_SZ /d "True" /f
```

This command performs the following actions:

1. Adds registry entries for the plugin under both 32-bit and 64-bit registry hives:
   - `HKEY_LOCAL_MACHINE\SOFTWARE\Genetec\Security Center\Plugins\$(ProjectName)`
   - `HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Genetec\Security Center\Plugins\$(ProjectName)`

2. Sets three key values for each registry entry:
   - `Enabled`: Set to "True" to enable the plugin.
   - `ServerModule`: Set to the full path of the built plugin DLL.
   - `AddFoldersToAssemblyProbe`: Set to "True" to allow Security Center to probe for additional assemblies in the plugin's folder.

### Why is this helpful?

1. **Automatic Registration**: This process automatically registers your plugin with Security Center, eliminating the need for manual registration steps.

2. **Consistency**: Ensures that the correct registry entries are always created or updated, reducing the chance of configuration errors.

### Important Notes:

- This post-build event requires administrative privileges to modify the registry. Make sure you're running Visual Studio as an administrator.
- The `$(ProjectName)` and `$(TargetPath)` are MSBuild variables that automatically use your project's name and the built DLL's path.
- If you rename your project or change the output path, these registry entries will be updated accordingly in subsequent builds.
- Remember to remove or modify these registry entries if you uninstall or move your plugin.

By including this post-build event, the template ensures that your plugin is ready for testing immediately after each successful build. This streamlines the development and testing process for Security Center plugins.

### Additional considerations:

When developing plugins for Security Center, it's important to understand how the Genetec Server interacts with plugin assemblies. This knowledge will help you avoid compilation issues and optimize your development workflow.

When the Genetec Server service starts, it loads plugin assemblies. This process has important implications for development:

1. **File Usage**: The Genetec Server process opens and loads the plugin assembly files. This can prevent other processes, including your development environment, from modifying these files.

2. **Restart Requirement**: For Security Center to load an updated version of your plugin, the Genetec Server service typically needs to be restarted. This allows it to release the existing assembly files and load the new versions.

### Impact on Development

This approach to handling plugin assemblies affects your development process in the following ways:

1. **Compilation Errors**: Attempting to rebuild your plugin while the Genetec Server service is running may result in build process failures. This occurs because the build cannot replace the existing assembly file that's being loaded by the Genetec Server process.

2. **Delayed Updates**: Changes to your plugin won't take effect in Security Center until the Genetec Server service is restarted, allowing it to load the new assembly versions.

### Managing Plugin Development

To effectively develop plugins given these considerations:

1. **Service Restart Cycle**: 
   - Stop the Genetec Server service before rebuilding your plugin.
   - Rebuild your solution.
   - Start the Genetec Server service to test your changes.

2. **Build Event Automation**: Consider automating service management in your build process, but use caution in shared environments.

### Example Build Events

Pre-build event to stop the service:
```
net stop "GenetecServer"
```

Post-build event to start the service:
```
net start "GenetecServer"
```

### Troubleshooting Compilation Issues

If you encounter errors related to file access during compilation:

1. Ensure the Genetec Server service is not running.
2. Check for any other processes that might be accessing your assembly files.
3. In rare cases, a system restart might be necessary to fully release all file handles.

By understanding these aspects of how Security Center manages plugin assemblies, you can develop more efficiently and troubleshoot issues more effectively.

## Debugging Techniques for Plugins

Debugging plugins for Security Center requires some specific techniques. Here are effective methods to debug your plugin:

### 1. Using Debugger.Launch()

You can add `Debugger.Launch()` in your plugin's code to trigger the debugger attachment when that part of the code is executed. This is particularly useful for debugging initialization issues.

Common places to add `Debugger.Launch()`:
- In the plugin's public default constructor
- In the `OnLoaded` method
- Anywhere you want to start debugging

Example:

```csharp
using System.Diagnostics;

public class YourPlugin : Plugin
{
    public YourPlugin()
    {
#if DEBUG
        Debugger.Launch(); // This will prompt to attach a debugger when the constructor is called
#endif
    }

    // You can also place it in OnLoaded or other methods as needed
}
```

When `Debugger.Launch()` is executed, it will prompt you to choose a debugger to attach to the process.

Benefits of using #IF DEBUG:
- Prevents accidental debugging prompts in production environments.
- Keeps your release builds clean and optimized.
- Allows you to include debugging code without impacting production performance.

Remember to always build your plugin in Release mode for production deployments to ensure that debug code is not included.

### 2. Manually Attaching to the GenetecPlugin.exe Process

For each instance of a plugin running, Security Center starts a separate GenetecPlugin.exe process. You can attach your debugger to this process to debug your plugin.

Steps to attach the debugger:

1. In Visual Studio, go to Debug > Attach to Process.
2. Look for GenetecPlugin.exe in the list of available processes.
3. If there are multiple GenetecPlugin.exe processes, you need to identify the correct one.

To determine which GenetecPlugin.exe process corresponds to your plugin:

1. Open Windows Task Manager.
2. Go to the Details tab.
3. Find the GenetecPlugin.exe processes.
4. Look at the "Command line" column (you may need to add this column if it's not visible).
5. The command line argument will contain the name of your plugin.

Example of a command line argument:

```
"GenetecPlugin.exe" /CustomActionSample_1098577fb9949c6812beb3c8c3bdc9ff "33800"
```

In this example, "CustomActionSample" is the name of the plugin.

Once you've identified the correct process, select it in Visual Studio's "Attach to Process" dialog and click "Attach".

Benefits of attaching to the process:
- You can debug your plugin while it's running within Security Center.
- You can set breakpoints in your code and step through it as it executes.
- You can inspect variables and the call stack at runtime.

Remember to build your plugin in Debug mode and ensure that the PDB (Program Database) files are generated and available for the best debugging experience.
