
# Plugin for Security Center
This sample project demonstrates how to create a plugin for Security Center, showcasing the fundamental structure and classes required for plugin development.
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
1.  Install the latest Genetec Security Center SDK. This will creates the necessary environment variables automatically.
   -   `GSC_SDK`: Points to the location of the Genetec SDK for .NET Framework 4.8.1
   -   `GSC_SDK_CORE`: Points to the location of the Genetec SDK for .NET 8
3.  Clone this repository to your local machine.
4.  Open the solution in Visual Studio.
5.  Build the project.

# Understanding and Implementing IPluginDatabaseSupport

## Overview

The `IPluginDatabaseSupport` interface is a crucial component in the plugin architecture that enables database operations for plugins. It provides a standardized way for plugins to interact with their associated databases.

## Purpose

The primary purposes of IPluginDatabaseSupport are:
1. To indicate that a plugin requires database support.
2. To provide a standardized way for Security Center to interact with the plugin's database operations.

## Interface Definition

```csharp
public interface IPluginDatabaseSupport
{
    DatabaseManager DatabaseManager { get; }
}
```

## Key Concepts

**DatabaseManager Property**: This property provides access to the DatabaseManager, which handles all database-related operations for the plugin.

## Implementation

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
    // ... (as explained in previous sections)
}
```

## Best Practices

1. **Resource Management**: Implement proper disposal of database resources such as SqlConnection and SqlCommand instances.

2. **Connection Management**: Use the `DatabaseConfiguration` provided by the system to create database connections.

3. **Error Handling**: Implement robust error handling in your DatabaseManager.

## Integration with Security Center

By implementing IPluginDatabaseSupport:

1. Security Center recognizes that your plugin requires database support.
2. It calls your DatabaseManager methods at appropriate times (e.g., for creation, upgrades, or cleanup).

## Example Usage

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

## Considerations

1. **Performance**: Optimize your database operations for efficiency.

2. **Scalability**: Design your database schema and operations to work efficiently as your plugin's data grows.

3. **Upgrades and Migrations**: When releasing new versions of your plugin, ensure that your upgrade scripts can handle upgrading from any previous version.

4. **Testing**: Thoroughly test your plugin to ensure proper functionality of all database operations.

By correctly implementing IPluginDatabaseSupport, you ensure that your plugin can efficiently manage its database operations. This interface is key to integrating your plugin's database needs with the broader application ecosystem.

# Understanding and Implementing GetSpecificCreationScript

## Overview

The `GetSpecificCreationScript` method is a crucial component in the initialization of your plugin's database. It provides the SQL script necessary to create the initial database structure, including tables, stored procedures, and other database objects specific to your plugin.

## Purpose

The primary purposes of GetSpecificCreationScript are:
1. To define the initial database schema for your plugin
2. To create any necessary stored procedures, functions, or other database objects

## Implementation

In your `DatabaseManager` derived class, override the `GetSpecificCreationScript` method:

```csharp
public override string GetSpecificCreationScript(string databaseName)
{
    return Resources.CreationScript;
}
```

### Key Points:

1. The method takes a `databaseName` parameter, which you can use if your script needs to reference the database name dynamically.
2. It returns a string containing the entire SQL script for creating your database objects.
3. Typically, this script is stored as a resource in your project (as seen in the `Resources.CreationScript` reference).

## Best Practices

1. **Comprehensive Script**: Ensure your creation script includes all necessary database objects (tables, indexes, stored procedures, functions, etc.) for your plugin to function correctly.

2. **Idempotent Script**: Write your script to be idempotent, meaning it can be run multiple times without error. Use `IF NOT EXISTS` checks before creating objects.

3. **Performance Considerations**: If creating large tables or numerous indexes, consider the performance impact during installation.

4. **Error Handling**: Include error handling in your script where appropriate.

## Example Creation Script

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

-- Create or update version info
IF NOT EXISTS (SELECT * FROM sys.objects 
               WHERE object_id = OBJECT_ID(N'[dbo].[MyPlugin_VersionInfo]') 
               AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MyPlugin_VersionInfo](
        [Version] INT NOT NULL,
        [InstallDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    )
    INSERT INTO [dbo].[MyPlugin_VersionInfo] ([Version]) VALUES (1)
END
ELSE
BEGIN
    UPDATE [dbo].[MyPlugin_VersionInfo] SET [Version] = 1
END
```

## Integration with Security Center

The `GetSpecificCreationScript` method is typically called by the main application when:
- Your plugin is being installed for the first time
- The application is verifying or repairing the database structure

By providing this script, you ensure that your plugin's database is properly initialized and structured, regardless of when or where it's installed.

## Considerations

- The creation script should create a database structure that is compatible with the current version of your plugin.
- Any future changes to the database structure should be handled through `DatabaseUpgradeItem`s, not by modifying this creation script.
- Be mindful of the script's execution time, especially for larger databases or complex structures.
- Test your creation script thoroughly, including on different database server versions if applicable.

By properly implementing the `GetSpecificCreationScript` method, you ensure that your plugin's database is correctly initialized, providing a solid foundation for your plugin's data management needs.

# Understanding and Implementing DatabaseUpgradeItem

## Overview

`DatabaseUpgradeItem` is a crucial component for managing database schema evolution in your plugin. It allows you to define and execute database upgrades smoothly as your plugin evolves across different versions.

## Purpose

The primary purposes of DatabaseUpgradeItem are:
1. To define database schema changes required when upgrading from one version to another
2. To provide a structured way to apply these changes in the correct order
3. To ensure database compatibility across different versions of your plugin

## Implementation

### Defining Upgrade Items

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

### Creating Upgrade Scripts

1. Create SQL scripts for each upgrade step. These scripts should contain all necessary SQL commands to migrate the database schema from the source version to the target version.
2. Store these scripts as resources in your project (as seen in the `Resources.UpgradeScript_XXXXX` references).

## Best Practices

1. **Version Numbering**: Use a consistent and meaningful version numbering scheme. In the example, five-digit numbers are used (50001, 500002, etc.).

2. **Incremental Upgrades**: Define upgrade items for each incremental version change. This allows for step-by-step upgrades and makes it easier to test and troubleshoot.

3. **Idempotent Scripts**: Write your upgrade scripts to be idempotent (can be run multiple times without changing the result beyond the initial application). This helps prevent issues if an upgrade is interrupted and needs to be rerun.

4. **Backwards Compatibility**: When possible, make schema changes in a backwards-compatible manner. This can help if you need to support multiple versions of your plugin simultaneously.

5. **Testing**: Thoroughly test each upgrade path, including upgrades that skip versions (e.g., 50001 to 500003).

6. **Documentation**: Maintain clear documentation of what each upgrade does and why it was necessary.

7. **Error Handling**: Include error handling and logging in your upgrade scripts to make troubleshooting easier.

## Integration with Security Center

By implementing `DatabaseUpgradeItem`, your plugin integrates with the application's database upgrade mechanism. This allows the system to:

- Determine the current version of your plugin's database schema
- Apply necessary upgrades in the correct order when updating your plugin
- Ensure database compatibility when the plugin is updated

## Considerations

- Upgrades are typically applied automatically by the main application when your plugin is updated.
- Your plugin should be prepared to work with any version of the database schema from the minimum supported version up to the current version.
- Consider data migration needs in addition to schema changes. Some upgrades may require moving or transforming existing data.

## Example Upgrade Script

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

-- Update version number in your version tracking table
UPDATE [dbo].[VersionInfo]
SET [Version] = 500002
WHERE [PluginId] = 'YourPluginId'
```

By properly implementing `DatabaseUpgradeItem`, you ensure that your plugin's database schema can evolve smoothly alongside your plugin's functionality, maintaining compatibility and performance across different versions.

# Understanding and Implementing DatabaseCleanupThreshold

## Overview

`DatabaseCleanupThreshold` is a crucial component in managing database maintenance for your plugin. It allows you to define rules for automatic cleanup of old data, helping to maintain optimal database performance and manage storage efficiently.

## Purpose

The primary purposes of DatabaseCleanupThreshold are:
1. To define what data should be cleaned up
2. To specify how long data should be retained before cleanup
3. To provide a mechanism for the main application to manage database maintenance across multiple plugins

## Implementation

### Defining Cleanup Thresholds

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

### Implementing Cleanup Logic

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

## Best Practices

1. **Meaningful Names**: Choose clear, descriptive names for your cleanup thresholds.
2. **Appropriate Defaults**: Set sensible default values for `defaultIsEnabled` and `defaultRetentionPeriod`.
3. **Efficient Cleanup**: Implement cleanup operations efficiently to minimize impact on system performance.
4. **Error Handling**: Implement robust error handling in your cleanup methods.
5. **Logging**: Log cleanup activities for troubleshooting and auditing purposes.

## Integration with Security Center

By implementing `DatabaseCleanupThreshold`, your plugin integrates with the application's centralized database maintenance system. This allows administrators to:

- View and modify retention periods for different types of data
- Enable or disable specific cleanup tasks
- Schedule cleanup operations according to system-wide policies

## Considerations

- The cleanup operation is triggered by the main application, not your plugin directly.
- Your plugin should be prepared to handle cleanup requests at any time.
- Consider the impact of cleanup operations on any active processes in your plugin.

By properly implementing `DatabaseCleanupThreshold`, you ensure that your plugin's database remains efficient and well-maintained, integrating smoothly with the overall system's data management strategy.