# Plugin SDK

The Plugin SDK for Genetec Security Center allows technology partners to create advanced, fully embedded integrations that function as custom roles within the system.

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

1. Install the latest Genetec Security Center SDK. This will create the necessary environment variables automatically.
   - `GSC_SDK`: Points to the location of the Genetec SDK for .NET Framework 4.8.1
   - `GSC_SDK_CORE`: Points to the location of the Genetec SDK for .NET 8
2. Clone this repository to your local machine.
3. Open the solution in Visual Studio **as an Administrator**.
4. Build the project.

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

2. **Abstract Properties in PluginDescriptor Class:**
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

## Database Support for Plugins

Plugins can leverage Security Center's database infrastructure through the `IPluginDatabaseSupport` interface. This provides a standardized way to manage plugin-specific database operations.

### Understanding and Implementing IPluginDatabaseSupport

#### Overview

The `IPluginDatabaseSupport` interface is an important component in the plugin architecture that enables database operations for plugins. It provides a standardized way for plugins to interact with their associated databases.

#### Purpose

The primary purposes of IPluginDatabaseSupport are:
1. To indicate that a plugin requires database support.
2. To provide a standardized way for Security Center to interact with the plugin's database operations.

#### Interface Definition

```csharp
public interface IPluginDatabaseSupport
{
    DatabaseManager DatabaseManager { get; }
}
```

#### Key Concepts

**DatabaseManager Property**: This property provides access to the DatabaseManager, which handles all database-related operations for the plugin.

#### Implementation

To implement IPluginDatabaseSupport in your plugin:

1. Implement the interface in your main plugin class:

```csharp
[PluginProperty(typeof(YourPluginDescriptor))]
public class YourPlugin : Plugin, IPluginDatabaseSupport
{
    private readonly YourDatabaseManager m_databaseManager = new YourDatabaseManager();

    public DatabaseManager DatabaseManager => m_databaseManager;

    // ... other plugin code ...
}
```

2. Create a custom DatabaseManager class:

```csharp
public class YourDatabaseManager : DatabaseManager
{
    // Implement required methods such as GetSpecificCreationScript, DatabaseCleanup, etc.
    // ... (as explained in following sections)
}
```

#### Best Practices

1. **Resource Management**: Implement proper disposal of database resources such as SqlConnection and SqlCommand instances.
2. **Connection Management**: Use the `DatabaseConfiguration` provided by the system to create database connections.
3. **Error Handling**: Implement robust error handling in your DatabaseManager.

#### Integration with Security Center

By implementing IPluginDatabaseSupport:

1. Security Center recognizes that your plugin requires database support.
2. It calls your DatabaseManager methods at appropriate times (e.g., for creation, upgrades, or cleanup).

#### Example Usage

Here's how you might use the DatabaseManager in your plugin operations:

```csharp
public class YourPlugin : Plugin, IPluginDatabaseSupport
{
    private readonly YourDatabaseManager m_databaseManager = new YourDatabaseManager();

    public DatabaseManager DatabaseManager => m_databaseManager;

    protected override void OnPluginStart()
    {
        base.OnPluginStart();

        // Example of using the database
        using (var connection = m_databaseManager.Configuration.CreateSqlDatabaseConnection())
        {
            // Perform some database operation
        }
    }

    // ... other plugin methods ...
}
```

#### Considerations

1. **Performance**: Optimize your database operations for efficiency.
2. **Scalability**: Design your database schema and operations to work efficiently as your plugin's data grows.
3. **Upgrades and Migrations**: When releasing new versions of your plugin, ensure that your upgrade scripts can handle upgrading from any previous version.
4. **Testing**: Thoroughly test your plugin to ensure proper functionality of all database operations.

By correctly implementing IPluginDatabaseSupport, you ensure that your plugin can efficiently manage its database operations. This interface is key to integrating your plugin's database needs with the broader application ecosystem.

### Understanding and Implementing GetSpecificCreationScript

#### Overview

The `GetSpecificCreationScript` method is a crucial component in the initialization of your plugin's database. It provides the SQL script necessary to create the initial database structure, including tables, stored procedures, and other database objects specific to your plugin.

#### Purpose

The primary purposes of GetSpecificCreationScript are:
1. To define the initial database schema for your plugin
2. To create any necessary stored procedures, functions, or other database objects

#### Implementation

In your `DatabaseManager` derived class, override the `GetSpecificCreationScript` method:

```csharp
public override string GetSpecificCreationScript(string databaseName)
{
    return Resources.CreationScript;
}
```

##### Key Points:

1. The method takes a `databaseName` parameter, which you can use if your script needs to reference the database name dynamically.
2. It returns a string containing the entire SQL script for creating your database objects.
3. Typically, this script is stored as a resource in your project (as seen in the `Resources.CreationScript` reference).

#### Best Practices

1. **Comprehensive Script**: Ensure your creation script includes all necessary database objects (tables, indexes, stored procedures, functions, etc.) for your plugin to function correctly.
2. **Idempotent Script**: Write your script to be idempotent, meaning it can be run multiple times without error. Use `IF NOT EXISTS` checks before creating objects.

#### Example Creation Script

Here's an example of what a creation script might look like:

```sql
-- Creation script for MyPlugin

-- Create main table
IF NOT EXISTS (SELECT * FROM sys.objects 
               WHERE object_id = OBJECT_ID(N'[dbo].[MyPlugin_MainTable]') 
               AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MyPlugin_MainTable](
        [Id] INT NOT NULL PRIMARY KEY IDENTITY,
        [Name] NVARCHAR(100) NOT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [Data] NVARCHAR(MAX) NULL
    )
END

-- Create index
IF NOT EXISTS (SELECT * FROM sys.indexes 
               WHERE name='IX_MyPlugin_MainTable_Name' 
               AND object_id = OBJECT_ID('dbo.MyPlugin_MainTable'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_MyPlugin_MainTable_Name] 
    ON [dbo].[MyPlugin_MainTable]([Name])
END

-- Create stored procedure
IF NOT EXISTS (SELECT * FROM sys.objects 
               WHERE object_id = OBJECT_ID(N'[dbo].[MyPlugin_GetData]') 
               AND type in (N'P', N'PC'))
BEGIN
    EXEC dbo.sp_executesql @statement = N'
    CREATE PROCEDURE [dbo].[MyPlugin_GetData]
        @Name NVARCHAR(100)
    AS
    BEGIN
        SET NOCOUNT ON;
        SELECT * FROM [dbo].[MyPlugin_MainTable]
        WHERE [Name] = @Name
    END'
END

-- Insert initial data if needed
IF NOT EXISTS (SELECT * FROM [dbo].[MyPlugin_MainTable])
BEGIN
    INSERT INTO [dbo].[MyPlugin_MainTable] ([Name], [Data])
    VALUES ('InitialEntry', 'This is the initial data entry')
END
```

#### Integration with Security Center

The `GetSpecificCreationScript` method is typically called by the main application when:
- Your plugin is being installed for the first time
- The application is verifying or repairing the database structure

By providing this script, you ensure that your plugin's database is properly initialized and structured, regardless of when or where it's installed.

#### Considerations

- The creation script should create a database structure that is compatible with the current version of your plugin.
- Any future changes to the database structure should be handled through `DatabaseUpgradeItem`s, not by modifying this creation script.
- Be mindful of the script's execution time, especially for larger databases or complex structures.
- Test your creation script thoroughly, including on different database server versions if applicable.

By properly implementing the `GetSpecificCreationScript` method, you ensure that your plugin's database is correctly initialized, providing a solid foundation for your plugin's data management needs.

### Understanding and Implementing DatabaseUpgradeItem

#### Overview

`DatabaseUpgradeItem` is a important component for managing database schema evolution in your plugin. It allows you to define and execute database upgrades smoothly as your plugin evolves across different versions.

#### Purpose

The primary purposes of DatabaseUpgradeItem are:
1. To define database schema changes required when upgrading from one version to another
2. To provide a structured way to apply these changes in the correct order
3. To ensure database compatibility across different versions of your plugin

#### Implementation

##### Defining Upgrade Items

In your `DatabaseManager` derived class, override the `GetDatabaseUpgradeItems` method to define your upgrade items:

```csharp
public override IEnumerable<DatabaseUpgradeItem> GetDatabaseUpgradeItems()
{
    // Upgrade from version 50001 to 500002
    yield return new DatabaseUpgradeItem(50001, 500002, Resources.UpgradeScript_50002);

    // Upgrade from version 50002 to 500003
    yield return new DatabaseUpgradeItem(50002, 500003, Resources.UpgradeScript_50003);

    // You can add more upgrade items as your plugin evolves
}
```

##### Creating Upgrade Scripts

1. Create SQL scripts for each upgrade step. These scripts should contain all necessary SQL commands to migrate the database schema from the source version to the target version.
2. Store these scripts as resources in your project (as seen in the `Resources.UpgradeScript_XXXXX` references).

#### Best Practices

1. **Incremental Upgrades**: Define upgrade items for each incremental version change. This allows for step-by-step upgrades and makes it easier to test and troubleshoot.
2. **Idempotent Scripts**: Write your upgrade scripts to be idempotent (can be run multiple times without changing the result beyond the initial application). This helps prevent issues if an upgrade is interrupted and needs to be rerun.
3. **Testing**: Thoroughly test each upgrade path, including upgrades that skip versions (e.g., 50001 to 500003).

#### Integration with Security Center

By implementing `DatabaseUpgradeItem`, your plugin integrates with the application's database upgrade mechanism. This allows the system to:

- Determine the current version of your plugin's database schema
- Apply necessary upgrades in the correct order when updating your plugin
- Ensure database compatibility when the plugin is updated

#### Considerations

- Upgrades are typically applied automatically by the main application when your plugin is updated.
- Your plugin should be prepared to work with any version of the database schema from the minimum supported version up to the current version.
- Consider data migration needs in addition to schema changes. Some upgrades may require moving or transforming existing data.

#### Example Upgrade Script

Here's an example of what an upgrade script might look like:

```sql
-- Upgrade script from version 50001 to 500002

-- Add a new column to an existing table
IF NOT EXISTS (SELECT * FROM sys.columns 
                WHERE object_id = OBJECT_ID(N'[dbo].[YourTable]') 
                AND name = 'NewColumn')
BEGIN
    ALTER TABLE [dbo].[YourTable]
    ADD NewColumn INT NULL
END

-- Create a new table
IF NOT EXISTS (SELECT * FROM sys.objects 
               WHERE object_id = OBJECT_ID(N'[dbo].[NewTable]') 
               AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[NewTable](
        [Id] INT NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL
    )
END
```

By properly implementing `DatabaseUpgradeItem`, you ensure that your plugin's database schema can evolve smoothly alongside your plugin's functionality, maintaining compatibility and performance across different versions.

### Understanding and Implementing DatabaseCleanupThreshold

#### Overview

`DatabaseCleanupThreshold` is an important component in managing database maintenance for your plugin. It allows you to define rules for automatic cleanup of old data, helping to maintain optimal database performance and manage storage efficiently.

#### Purpose

The primary purposes of DatabaseCleanupThreshold are:
1. To define what data should be cleaned up
2. To specify how long data should be retained before cleanup
3. To provide a mechanism for the main application to manage database maintenance across multiple plugins

#### Implementation

##### Defining Cleanup Thresholds

In your `DatabaseManager` derived class, override the `GetDatabaseCleanupThresholds` method to define your cleanup thresholds:

```csharp
public override IEnumerable<DatabaseCleanupThreshold> GetDatabaseCleanupThresholds()
{
    yield return new DatabaseCleanupThreshold(
        name: "LogCleanup",
        title: "Log Retention",
        defaultIsEnabled: true,
        defaultRetentionPeriod: 30 // days
    );
    
    // You can define multiple thresholds for different types of data
    yield return new DatabaseCleanupThreshold(
        name: "TemporaryDataCleanup",
        title: "Temporary Data Retention",
        defaultIsEnabled: true,
        defaultRetentionPeriod: 7 // days
    );
}
```

##### Implementing Cleanup Logic

Override the `DatabaseCleanup` method to implement the actual cleanup logic:

```csharp
public override void DatabaseCleanup(string name, int retentionPeriod)
{
    switch (name)
    {
        case "LogCleanup":
            DeleteOldLogs(retentionPeriod);
            break;
        case "TemporaryDataCleanup":
            DeleteTemporaryData(retentionPeriod);
            break;
    }
}
```

#### Best Practices

1. **Meaningful Names**: Choose clear, descriptive names for your cleanup thresholds.
2. **Appropriate Defaults**: Set sensible default values for `defaultIsEnabled` and `defaultRetentionPeriod`.
3. **Efficient Cleanup**: Implement cleanup operations efficiently to minimize impact on system performance.
4. **Error Handling**: Implement robust error handling in your cleanup methods.
5. **Logging**: Log cleanup activities for troubleshooting purposes.

#### Integration with Security Center

By implementing `DatabaseCleanupThreshold` allows the plugin to:

- View and modify retention periods for different types of data
- Schedule cleanup operations according to the plugin configuration

#### Considerations

- The cleanup operation is triggered by Security Center, not your plugin directly.
- Your plugin should be prepared to handle cleanup requests at any time.
- Consider the impact of cleanup operations on any active processes in your plugin.
