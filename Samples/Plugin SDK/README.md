# Plugin SDK

The Plugin SDK for Genetec™ Security Center lets technology partners create integrations that run as custom roles.

## Roles in Security Center

Plugins function as custom roles in Security Center.

### What are roles?

In Security Center, roles are components that perform specific tasks within the system. Each role is associated with one or more servers, which host and execute the role's functions. Role operations include managing video units, archiving data, or synchronizing users with corporate directories.

### Role features

- **Role type:** each role has a defined type that determines its specific functions. For example, a role could be responsible for managing video units and their associated archives.
- **Role settings:** these settings specify the parameters within which the role operates, such as data retention periods or database configurations.
- **Server assignment:** roles can be assigned to one or multiple servers. This allows for load balancing and failover capabilities.
- **Failover support:** a role can automatically switch to a secondary server if the primary server fails.

### Plugins as custom roles

A plugin developed with the Plugin SDK implements a custom Security Center role. It inherits role features, including:

- The ability to define a specific role type with custom functionality
- Configurable settings for your plugin's operation
- Automatic server assignment and failover support
- Built-in health monitoring and database support

## Plugin SDK overview

The Plugin SDK provides server role capabilities and client integration.

### Capabilities

The Plugin SDK provides these capabilities:

- Develop custom role entities similar to built-in roles such as the Archiver and Access Manager roles.
- Use built-in server-side features, which are automatically inherited:
  - Failover support
  - Health monitoring with status, history, and statistics
  - Database support
- Integrate with client components in Config Tool and Security Desk.

### Support and licensing requirements

The Plugin SDK has these support and licensing requirements:

- **SDK support plan**: creating a Security Center plugin requires purchasing a Gold SDK support plan.
- **Licensing**: the end user's Security Center license must include the part number assigned to your plugin.

## Prerequisites

- **.NET Framework 4.8.1 targeting pack**: the Plugin SDK samples in this repository target `net481`. Selecting a solution configuration named `_NET8` does not retarget these projects.
- **Security Center SDK**: installed with the `GSC_SDK` environment variable pointing to its .NET Framework assemblies.
- **Build tools**: Visual Studio 2022 version 17.8 or later, or the .NET 8 SDK or a later compatible SDK for command-line builds. The samples compile with C# 12. Build with administrative privileges when running post-build registration.
- **Security Center**: installed and running on your system.
- **Valid Security Center license**: all samples include the development SDK certificate.

## Building a sample

All Plugin SDK samples in this repository target .NET Framework 4.8.1. Their `Debug` and `Release` configurations produce `net481` assemblies, including when the solution maps an `_NET8` configuration to them.

1. Open an elevated PowerShell terminal or run Visual Studio as an administrator. The samples' post-build registration writes to `HKEY_LOCAL_MACHINE`.
2. Set `GSC_SDK` to the installed SDK directory containing the .NET Framework assemblies. Replace the example path with your installation:

   ```powershell
   $env:GSC_SDK = 'C:\Program Files (x86)\Genetec Security Center 5.14 SDK'
   ```

3. From the repository root, build the `BasicPluginTemplate` sample:

   ```powershell
   dotnet build "Samples/Plugin SDK/BasicPluginTemplate/BasicPluginTemplate.csproj" -c Debug -f net481
   ```

4. Use `-c Release` for a release build, or replace the project path with another plugin sample. Follow that sample's instructions for role creation and activation.

`PluginReportsSample`, `PluginConfigurationSample`, and `CustomReportSample` support `SkipPluginRegistration=true` to build without updating local registration. For example, from the repository root:

```powershell
dotnet build "Samples/Plugin SDK/PluginReportsSample/PluginReportsSample.csproj" -c Debug -f net481 -p:SkipPluginRegistration=true
```

That property is specific to those three projects. Other samples' post-build targets register their modules.

## Build and deployment dependencies

Server modules reference `Genetec.Sdk.dll` and `Genetec.Sdk.Plugin.dll`. Samples containing client user interface code also reference `Genetec.Sdk.Workspace.dll` and, where needed, `Genetec.Sdk.Controls.dll`. The existing samples resolve these references through `GSC_SDK`.

Project files declare their non-SDK dependencies. `dotnet build` restores the packages automatically:

| Sample | Declared NuGet packages |
|--------|-------------------------|
| `PluginConfigurationSample` | `Microsoft.Bcl.AsyncInterfaces`, `Newtonsoft.Json` |
| `PluginDatabaseSample` and `PluginReportsSample` | `Microsoft.Data.SqlClient` |
| `CustomActionSample` | `Prism.Core`, `System.Resources.Extensions` |

Database samples also require SQL Server and a configured role database. Install the .NET Framework 4.8.1 runtime on computers running the existing samples. For modern server modules, Security Center supplies the supported host runtime. Keep client UI modules on .NET Framework and deploy required non-SDK dependencies beside each registered module. Keep **Copy Local** set to `False` for Genetec SDK assemblies; the host supplies them. See [Shared sample helpers](../Shared/README.md#choosing-an-assembly-resolver) for dependency probing and placement.

## Building modern .NET plugins

Choose your `ServerModule` target framework based on the Security Center version deployed on the role server. .NET 8 hosting is available from Security Center 5.13, and .NET 10 hosting is available from Security Center 5.14.1. A `ClientModule` must target .NET Framework because Security Desk and Config Tool host it in-process.

| Security Center on the role server | Supported server module targets | SDK references for a modern module |
|------------------------------------|----------------------------------|-------------------------------------|
| 5.12 and earlier | .NET Framework | Use .NET Framework SDK assemblies |
| 5.13 and 5.14.0 | .NET Framework or .NET 8 | Use SDK 5.13 or later containing compatible `net8.0-windows` assemblies |
| 5.14.1 and later | .NET Framework, .NET 8, or .NET 10 | Use compatible modern SDK assemblies; use SDK 5.14.1 or later for `net10.0-windows` assemblies |

Use Visual Studio 2022 version 17.8 or later for a .NET 8 server module and Visual Studio 2026 or later for .NET 10. Command-line builds require a .NET SDK supporting the selected target. Set `GSC_SDK_CORE` to the corresponding modern SDK directory. A .NET 8 module cannot reference SDK assemblies built for .NET 10, even when the role server supports .NET 10.

Creating a modern server module requires project and reference changes. The existing samples have no .NET 8 or .NET 10 plugin configuration. Build a separate modern server project and keep client UI code in a .NET Framework project using the setup and registration instructions linked below.

### Security Center 5.14 and later

Deploy and register the `ServerModule` on each server assigned to the role. Add a `ClientModule` on the workstations only if your integration provides a Security Desk or Config Tool extension.

### Security Center 5.13

For a modern .NET `ServerModule`, also deploy and register a .NET Framework `Client` discovery assembly on the role server. This assembly allows Security Center to discover the plugin type. A `ClientModule` for user interface extensions is a separate component.

For project setup, migration steps, and the custom-privilege deployment exception, see [Building .NET plugins](https://github.com/Genetec/DAP/wiki/plugin-sdk-net8) and [Deploying plugins and Workspace modules](https://github.com/Genetec/DAP/wiki/plugin-sdk-deployment).

## Creating a plugin

To create a plugin for Security Center, follow these steps:

1. Create a new class that inherits from the `Plugin` class found in `Genetec.Sdk.Plugin.dll`.
2. Ensure your plugin class has a default constructor.
3. Add the `PluginProperty` attribute to your class, specifying a class that inherits from `PluginDescriptor`.
4. Implement the required abstract members in the plugin class and the `PluginDescriptor` class.
5. Define a unique `PluginGuid` in the `PluginDescriptor` class.

The example shows the structure of the plugin and descriptor classes:

```csharp
using System;
using Genetec.Sdk.EventsArgs;
using Genetec.Sdk.Plugin;

[PluginProperty(typeof(YourPluginDescriptor))]
public class YourPlugin : Plugin
{
    public YourPlugin()
    {
        // Default public constructor
    }

    // Optional lifecycle callbacks
    protected override void OnPluginStart()
    {
        // Plugin startup logic
    }

    protected override void OnPluginLoaded()
    {
        // Plugin loaded logic
    }

    // Implement required query handling
    protected override void OnQueryReceived(ReportQueryReceivedEventArgs args)
    {
        // Handle queries
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Dispose of managed resources owned by this plugin.
        }

        // Release unmanaged resources owned by this plugin.
    }

    // Other overrides and custom methods as needed
}

public class YourPluginDescriptor : PluginDescriptor
{
    // Implement abstract properties
    public override string Name => "Your Plugin Name";
    public override string Description => "Description of your plugin";
    public override Guid PluginGuid => new Guid("YOUR-UNIQUE-GUID-HERE");

    // This example has no plugin-specific default configuration.
    public override string SpecificDefaultConfig => null;

    // Override other virtual properties as needed
}
```

### Plugin members and identifiers

- **Required plugin methods:**
   - `OnQueryReceived(ReportQueryReceivedEventArgs args)`: implement query handling for the reports your plugin supports.
   - `Dispose(bool disposing)`: implement cleanup for resources owned by your plugin.

- **Optional lifecycle callbacks:**
   - `OnPluginLoaded()`: override this callback for setup before the plugin starts.
   - `OnPluginStart()`: override this callback to initialize resources and start background tasks.

- **Required descriptor properties:**
   - `Name`: the plugin name shown in Security Center.
   - `Description`: a brief description of the plugin's functionality.
   - `PluginGuid`: a unique identifier that must differ from other plugins.
   - `SpecificDefaultConfig`: the plugin-specific default configuration.

- **Unique PluginGuid:**
   - Each plugin must have its own unique `PluginGuid`. This GUID is used by Security Center to identify and manage your plugin.
   - Generate a new GUID for each plugin you create. You can use tools like Visual Studio's **Create GUID** tool or online GUID generators.
   - Never reuse a `PluginGuid` from another plugin, as this can cause conflicts in Security Center.

Remember to replace `YourPlugin`, `YourPluginDescriptor`, and `"YOUR-UNIQUE-GUID-HERE"` with appropriate names and a unique GUID for your plugin.

## Post-build registration

The project file includes a post-build event that automatically registers the plugin with Security Center on the development machine for testing and use.

### Post-build command

The project file defines this post-build command:

```bat
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\Genetec\Security Center\Plugins\$(ProjectName)" /v Enabled /t REG_SZ /d "True" /f
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\Genetec\Security Center\Plugins\$(ProjectName)" /v ServerModule /t REG_SZ /d "$(TargetPath)" /f
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\Genetec\Security Center\Plugins\$(ProjectName)" /v AddFoldersToAssemblyProbe /t REG_SZ /d "True" /f

REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Genetec\Security Center\Plugins\$(ProjectName)" /v Enabled /t REG_SZ /d "True" /f
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Genetec\Security Center\Plugins\$(ProjectName)" /v ServerModule /t REG_SZ /d "$(TargetPath)" /f
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Genetec\Security Center\Plugins\$(ProjectName)" /v AddFoldersToAssemblyProbe /t REG_SZ /d "True" /f
```

This command performs the following actions:

- Adds registry entries for the plugin under both 32-bit and 64-bit registry hives:
   - `HKEY_LOCAL_MACHINE\SOFTWARE\Genetec\Security Center\Plugins\$(ProjectName)`
   - `HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Genetec\Security Center\Plugins\$(ProjectName)`

- Sets three key values for each registry entry:
   - `Enabled`: set to `True` to enable the plugin.
   - `ServerModule`: set to the full path of the built plugin DLL.
   - `AddFoldersToAssemblyProbe`: set to `True` to allow Security Center to probe for additional assemblies in the plugin's folder.

### Registration requirements

- This post-build event requires administrative privileges to modify the registry. Run Visual Studio as an administrator.
- The `$(ProjectName)` and `$(TargetPath)` are MSBuild variables that automatically use your project's name and the built DLL's path.
- If you rename your project or change the output path, these registry entries will be updated accordingly in subsequent builds.
- Remove or modify these registry entries if you uninstall or move your plugin.

### Assembly reload considerations

When the Genetec Server service starts, it loads plugin assemblies. Loaded assembly files can prevent rebuilding the plugin, and loading an updated version typically requires restarting the service:

- **File usage**: the Genetec Server process opens and loads the plugin assembly files. This can prevent other processes, including your development environment, from modifying these files.
- **Restart requirement**: for Security Center to load an updated version of your plugin, the Genetec Server service typically needs to be restarted. This allows it to release the existing assembly files and load the new versions.

### Effect on development

This approach to handling plugin assemblies affects your development process in the following ways:

- **Compilation errors**: attempting to rebuild your plugin while the Genetec Server service is running may result in build process failures. This occurs because the build cannot replace the existing assembly file that's being loaded by the Genetec Server process.
- **Delayed updates**: changes to your plugin won't take effect in Security Center until the Genetec Server service is restarted, allowing it to load the new assembly versions.

### Managing plugin development

To effectively develop plugins given these considerations:

1. Stop the Genetec Server service before rebuilding your plugin.
2. Rebuild your solution.
3. Start the Genetec Server service to test your changes.

Consider automating service management in your build process, but use caution in shared environments.

### Build events

Pre-build event to stop the service:

```bat
net stop "GenetecServer"
```

Post-build event to start the service:

```bat
net start "GenetecServer"
```

### Troubleshooting file access errors during compilation

If you encounter errors related to file access during compilation:

1. Ensure the Genetec Server service is not running.
2. Check for any other processes that might be accessing your assembly files.
3. In rare cases, a system restart might be necessary to fully release all file handles.

## Debugging plugins

To debug a plugin, launch a debugger from its code or attach to its running process.

### Using `Debugger.Launch()`

Add `Debugger.Launch()` to trigger debugger attachment when execution reaches the call, such as during plugin initialization.

Common places to add `Debugger.Launch()`:

- In the plugin's public default constructor
- In the `OnPluginLoaded` method
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

    // You can also place it in OnPluginLoaded or other methods as needed
}
```

When `Debugger.Launch()` executes, it prompts you to select a debugger for the process.

The `#if DEBUG` block includes the call only when `DEBUG` is defined. Otherwise, the call is excluded from the build, preventing its debugger prompts and runtime overhead.

Build your plugin in `Release` mode for production deployments, with `DEBUG` undefined so the `#if DEBUG` block is excluded.

### Attaching to `GenetecPlugin.exe`

For each instance of a plugin running, Security Center starts a separate `GenetecPlugin.exe` process. You can attach your debugger to this process to debug your plugin.

Steps to attach the debugger:

1. In Visual Studio, select **Debug > Attach to Process**.
2. Find `GenetecPlugin.exe` in the process list.
3. If multiple `GenetecPlugin.exe` processes are listed, identify the one running your plugin.

To determine which `GenetecPlugin.exe` process corresponds to your plugin:

1. Open Windows Task Manager.
2. Select the **Details** tab.
3. Find the `GenetecPlugin.exe` processes.
4. If the **Command line** column is not visible, add it.
5. Identify your plugin's process using the plugin name in the **Command line** column.

Example of a command line argument:

```text
"GenetecPlugin.exe" /CustomActionSample_1098577fb9949c6812beb3c8c3bdc9ff "33800"
```

In this example, `CustomActionSample` is the name of the plugin.

Select the correct process in Visual Studio's *Attach to Process* dialog box and click **Attach**.

Attaching to your plugin's process in Security Center lets you set breakpoints, step through its code, and inspect variables and the call stack at runtime.

Build your plugin in `Debug` mode and generate program database (PDB) files. Make these files available to the debugger.

## Database support for plugins

Plugins can use Security Center's database infrastructure through the `IPluginDatabaseSupport` interface to manage their database operations.

The [PluginDatabaseSample](./PluginDatabaseSample/) adds a row to the `Logs` table each time its database enters `DatabaseState.Connected`. A reconnect adds another row. To see these entries, run this query in the plugin role's database:

```sql
SELECT TOP (10) Timestamp, LogLevel, Message
FROM dbo.Logs
ORDER BY Timestamp DESC;
```

### Implementing `IPluginDatabaseSupport`

Use this interface to provide a `DatabaseManager` for the plugin.

#### Overview

The `IPluginDatabaseSupport` interface enables database operations for plugins and provides a standardized way for plugins to interact with their associated databases.

#### Purpose

The `IPluginDatabaseSupport` interface indicates that a plugin requires database support and defines how Security Center interacts with the plugin's database operations.

#### Interface definition

```csharp
public interface IPluginDatabaseSupport
{
    DatabaseManager DatabaseManager { get; }
}
```

#### `DatabaseManager` property

**DatabaseManager property**: this property provides access to the `DatabaseManager`, which handles all database-related operations for the plugin.

#### Implementation

To implement `IPluginDatabaseSupport` in your plugin:

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

2. Create a custom `DatabaseManager` class:

```csharp
public class YourDatabaseManager : DatabaseManager
{
    // Implement required methods such as GetSpecificCreationScript, DatabaseCleanup, etc.
    // ... (as explained in following sections)
}
```

#### Database resource management

Dispose of database resources such as `SqlConnection` and `SqlCommand` instances. Use the `DatabaseConfiguration` provided by the system to create database connections. Handle errors in your `DatabaseManager`.

#### Integration with Security Center

Security Center recognizes that a plugin implementing `IPluginDatabaseSupport` requires database support. It calls the plugin's `DatabaseManager` methods for operations such as database creation, upgrades, or cleanup.

#### Example usage

Wait for `DatabaseState.Connected` in `DatabaseManager.OnDatabaseStateChanged` before accessing the plugin database. `OnPluginStart` does not guarantee that the database is ready.

The [PluginDatabaseSample database manager](PluginDatabaseSample/SampleDatabaseManager.cs) overrides `OnDatabaseStateChanged`, stores the current state, and raises its own `DatabaseStateChanged` event. The [sample plugin](PluginDatabaseSample/SamplePlugin.cs) subscribes in `OnPluginLoaded` and inserts a log entry when the state becomes `Connected`.

#### Considerations

- **Performance**: optimize your database operations for efficiency.
- **Scalability**: design your database schema and operations to work efficiently as your plugin's data grows.
- **Upgrades and migrations**: when releasing a new version of your plugin, provide and test upgrade paths from each supported earlier database schema version.
- **Testing**: thoroughly test your plugin to ensure proper functionality of all database operations.

### Implementing `GetSpecificCreationScript`

Provide the SQL script that creates the plugin database structure.

#### Overview

The `GetSpecificCreationScript` method provides the SQL script used to initialize your plugin's database structure, including tables, stored procedures, and other database objects specific to your plugin.

#### Purpose

The script returned by `GetSpecificCreationScript` defines the plugin's initial database schema and creates any required stored procedures, functions, or other database objects.

#### Implementation

In your `DatabaseManager` derived class, override the `GetSpecificCreationScript` method:

```csharp
public override string GetSpecificCreationScript(string databaseName)
{
    return Resources.CreationScript;
}
```

##### Method parameters and return value

- The method takes a `databaseName` parameter, which you can use if your script needs to reference the database name dynamically.
- It returns a string containing the entire SQL script for creating your database objects.
- The script is typically stored as a resource in your project, as shown by `Resources.CreationScript`.

#### Creation script design

Include all required tables, indexes, stored procedures, functions, and other database objects in the creation script. Make the script idempotent so it can run multiple times without error. Use `IF NOT EXISTS` checks before creating objects.

#### Example creation script

Example creation script:

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

The main application typically calls `GetSpecificCreationScript` when the plugin is installed for the first time or when the application verifies or repairs the database structure.

#### Considerations

- Keep the creation script up to date so new databases are created with the schema required by the current plugin version.
- Use `DatabaseUpgradeItem`s to migrate existing databases from supported earlier schema versions.
- Check the script's execution time, especially for larger databases or complex structures.
- Test the creation script thoroughly, including on different database server versions when applicable.

### Implementing `DatabaseUpgradeItem`

Define the database schema changes for an upgrade and their execution order.

#### Overview

`DatabaseUpgradeItem` lets you define and execute database schema upgrades across versions of your plugin.

#### Purpose

`DatabaseUpgradeItem` defines the database schema changes for a plugin upgrade and their execution order to maintain database compatibility across plugin versions.

#### Implementation

Define upgrade items and their SQL scripts in your plugin's `DatabaseManager`.

##### Defining upgrade items

In your `DatabaseManager` derived class, override the `GetDatabaseUpgradeItems` method to define your upgrade items:

```csharp
public override IEnumerable<DatabaseUpgradeItem> GetDatabaseUpgradeItems()
{
    // Upgrade from version 50001 to 50002
    yield return new DatabaseUpgradeItem(50001, 50002, Resources.UpgradeScript_50002);

    // Upgrade from version 50002 to 50003
    yield return new DatabaseUpgradeItem(50002, 50003, Resources.UpgradeScript_50003);

    // You can add more upgrade items as your plugin evolves
}
```

##### Creating upgrade scripts

1. Create SQL scripts for each upgrade step. These scripts should contain all necessary SQL commands to migrate the database schema from the source version to the target version.
2. Store the scripts as resources in your project, as shown by `Resources.UpgradeScript_XXXXX`.

#### Upgrade script design

Use these guidelines when designing upgrade scripts:

- Define an upgrade item for each incremental version change. This supports upgrades in sequence and simplifies testing and troubleshooting.
- Make upgrade scripts idempotent so running them again does not change the result beyond the initial application. This helps prevent issues if an interrupted upgrade must be retried.
- Test each upgrade path thoroughly, including paths that skip versions, such as 50001 to 50003.

#### Integration with Security Center

By implementing `DatabaseUpgradeItem`, your plugin integrates with the application's database upgrade mechanism. This allows the system to:

- Determine the current version of your plugin's database schema
- Apply necessary upgrades in the correct order when updating your plugin
- Ensure database compatibility when the plugin is updated

#### Considerations

- Upgrades are typically applied automatically by the main application when your plugin is updated.
- Define the earliest database schema version your plugin supports upgrading to the current schema.
- Consider data migration needs in addition to schema changes. Some upgrades may require moving or transforming existing data.

#### Example upgrade script

Example upgrade script:

```sql
-- Upgrade script from version 50001 to 50002

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

### Implementing `DatabaseCleanupThreshold`

Define data retention settings and cleanup operations.

#### Overview

`DatabaseCleanupThreshold` lets you define rules for automatically cleaning up old plugin data to help maintain database performance and manage storage.

#### Purpose

`DatabaseCleanupThreshold` defines what data to clean up and how long to retain it. It provides a mechanism for the main application to manage database maintenance across multiple plugins.

#### Implementation

Define cleanup thresholds and their cleanup logic in your plugin's `DatabaseManager`.

##### Defining cleanup thresholds

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

##### Implementing cleanup logic

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

#### Cleanup configuration and handling

Use descriptive names for cleanup thresholds and defaults for `defaultIsEnabled` and `defaultRetentionPeriod` that suit the plugin's data retention needs. Implement cleanup operations efficiently to minimize their effect on system performance. Handle errors in cleanup methods and log cleanup activities for troubleshooting.

#### Integration with Security Center

Implementing `DatabaseCleanupThreshold` allows the plugin to:

- View and modify retention periods for different types of data
- Schedule cleanup operations according to the plugin configuration

#### Considerations

- Security Center triggers cleanup operations; the plugin does not trigger them directly.
- Your plugin should be prepared to handle cleanup requests at any time.
- Account for the effect of cleanup operations on active plugin processes.
