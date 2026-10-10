
# Platform SDK samples

The Platform SDK provides authentication, entity management, and system operations for Security Center integrations. These samples demonstrate patterns used in Media SDK, Workspace SDK, and Plugin SDK development.

## SDK components

The samples use the `Engine` class and Security Center entities.

### The Engine class

The `Engine` class is the entry point for SDK operations in these samples. It provides:

- **Connection Management**: All samples use the Engine to authenticate and maintain connections to Security Center
- **Entity Access**: Samples access the entity cache and retrieve Security Center objects through the Engine
- **Service Managers**: The Engine exposes specialized managers that samples use for different operations:
  - `LoginManager`: Used for authentication and connection state monitoring
  - `ReportManager`: Used for executing queries and retrieving data
  - `TransactionManager`: Used for creating, modifying, and deleting entities
  - `ActionManager`: Used for executing system actions and automation

```csharp
// How samples typically use the Engine
using var engine = new Engine();
var connectionState = await engine.LoginManager.LogOnAsync(server, username, password);
if (connectionState == ConnectionStateCode.Success)
{
    // Sample can now perform operations
}
```

### Entity management

All Platform SDK samples work with Security Center's client-side entity cache:

- **Entity cache**: a local storage system that samples use to access Security Center objects efficiently.
- **Entity loading**: samples retrieve and cache entities using queries to avoid repeated server requests.
- **Entity relationships**: samples can navigate between related entities, for example, from cameras to their associated doors.

## The SampleBase pattern

Most Platform SDK samples inherit from the `SampleBase` class, which implements the Template Method pattern to provide consistent infrastructure across all samples.

### What SampleBase provides

The `SampleBase` class handles these operations so each sample can demonstrate a feature:

- **SDK initialization**: automatically calls `SdkResolver.Initialize()` to configure assembly loading
- **Connection management**: handles server connection and authentication with error handling
- **Event handling**: sets up connection state monitoring and provides user feedback
- **Cancellation support**: implements Ctrl+C handling for graceful shutdown
- **Resource cleanup**: disposes of SDK resources when samples exit

### Using SampleBase

```csharp
// Pattern used by most Platform SDK samples
public class CardholderSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        // SampleBase has already connected the Engine
        // Sample implements its specific logic here
        await LoadEntities(engine, token, EntityType.Cardholder);
        
        var cardholders = engine.GetEntities(EntityType.Cardholder).OfType<Cardholder>();
        foreach (var cardholder in cardholders)
        {
            Console.WriteLine($"Cardholder: {cardholder.Name}");
        }
    }
}

// Program.cs entry point
await new CardholderSample().RunAsync();
```

### The LoadEntities method

SampleBase provides the `LoadEntities` method that demonstrates efficient entity loading patterns:

```csharp
// How samples load entities into the cache
await LoadEntities(engine, token, EntityType.Camera, EntityType.Door);

// Entities are then available from the cache
var cameras = engine.GetEntities(EntityType.Camera).OfType<Camera>();
var doors = engine.GetEntities(EntityType.Door).OfType<Door>();
```

**What LoadEntities does:**
- **Paged Loading**: Loads entities in manageable pages (1000 per page) to handle large datasets
- **Cancellation Support**: Respects cancellation tokens for responsive user interaction
- **Related Data**: Downloads all related entity data for complete object relationships

## Configuration and connection

Configure connection values and handle authentication results before performing SDK operations.

### Connection parameters in samples
All samples use hardcoded connection parameters for demonstration purposes:

```csharp
// Standard connection pattern across all samples
const string server = "localhost";
const string username = "admin";  
const string password = "";
```

The samples use hardcoded connection parameters to keep the examples focused on SDK concepts. Use a separate configuration mechanism in production applications.

### Authentication patterns in samples
All samples follow consistent authentication patterns:
- Check `ConnectionStateCode` before proceeding with operations
- Display connection status and errors to the user
- Handle authentication failures gracefully with clear error messages

## Prerequisites

- **Runtime target**: these samples target .NET Framework 4.8.1 or .NET 8 for Windows. The Platform SDK also supports standalone .NET 10 applications; the samples have no .NET 10 configuration.
- **Security Center SDK**: installed with these environment variables configured:
  - `GSC_SDK`: points to the SDK location for .NET Framework.
  - `GSC_SDK_CORE`: points to the SDK location when building for .NET 8.
- **Build tools**: Visual Studio 2022 version 17.8 or later, or the .NET 8 SDK or a later compatible SDK for command-line builds. All samples compile with C# 12, including `net481` builds.
- **.NET Framework targeting pack**: install the .NET Framework 4.8.1 targeting pack when building `net481`.
- **Security Center**: a running system that you can connect to.
- **Valid Security Center license**: all samples include the development SDK certificate.

### SDK and runtime compatibility

Choose compatible SDK assemblies for the application's target:

| Application target | Security Center SDK requirement | Assembly directory |
|--------------------|---------------------------------|--------------------|
| `net481` | SDK .NET Framework assemblies | SDK installation root, selected through `GSC_SDK` |
| `net8.0-windows` | SDK 5.12.2 or later containing .NET 8 assemblies | `net8.0-windows`, selected through `GSC_SDK_CORE` |
| `net10.0-windows`, your own application | Use SDK 5.14.1 or later for .NET 10 assemblies, or compatible .NET 8 assemblies from SDK 5.12.2 or later | `net10.0-windows` or compatible `net8.0-windows`, selected through `GSC_SDK_CORE` |

Install the .NET Framework 4.8.1 targeting pack for `net481`, or the .NET 8 SDK for `net8.0-windows`. Run `dotnet --list-sdks` to check the installed .NET SDKs.

A .NET 8 application cannot reference SDK assemblies built for .NET 10. A .NET 10 application can reference compatible SDK assemblies built for .NET 8 or .NET 10. Installing a newer .NET SDK does not change a project's target framework.

To build your own .NET 10 application, use the [repository's .NET 10 build instructions](../../README.md#building-your-own-net-10-application). Retargeting requires changing the project's target, conditional SDK references, and package dependencies. See [Migrating to modern .NET](https://github.com/Genetec/DAP/wiki/platform-sdk-migrating-to-modern-dotnet) for the complete migration procedure and Media SDK restrictions.

#### Build configuration model

The projects use explicit build configurations for framework targeting:

- `Debug` / `Release`: build .NET Framework 4.8.1.
- `Debug_NET8` / `Release_NET8`: build .NET 8 for Windows.

.NET 8 builds require Security Center SDK 5.12.2 or later with `GSC_SDK_CORE` selecting its `net8.0-windows` assemblies. A missing `Genetec.Sdk.dll` produces a build error; an existing DLL from an incompatible target can also cause reference or compilation errors.

#### Environment variables

The build system checks these environment variables:

- **`GSC_SDK`**: Points to the Security Center SDK assemblies for .NET Framework.
- **`GSC_SDK_CORE`**: Points to the Security Center SDK assemblies for modern .NET. For these configurations, use the SDK's `net8.0-windows` folder.

For example, select the SDK 5.13 installation for the current PowerShell terminal:

```powershell
$env:GSC_SDK = 'C:\Program Files (x86)\Genetec Security Center 5.13 SDK'
$env:GSC_SDK_CORE = Join-Path $env:GSC_SDK 'net8.0-windows'
```

Replace the installation path with your SDK's path. Check that `Genetec.Sdk.dll` exists in the selected directory. Another SDK installation can change `GSC_SDK_CORE` to a `net10.0-windows` directory; choose a compatible .NET 8 installation before building an `_NET8` configuration. Restart Visual Studio after changing persistent environment variables.

#### Project configuration

The Platform SDK samples use conditional target frameworks in their `.csproj` files to map each build configuration to one framework:

```xml
<TargetFramework>net481</TargetFramework>
<TargetFramework Condition="'$(Configuration)' == 'Debug_NET8' OR '$(Configuration)' == 'Release_NET8'">net8.0-windows</TargetFramework>
<Configurations>Debug;Release;Debug_NET8;Release_NET8</Configurations>
```

The framework-specific dependencies are:

- **.NET Framework 4.8.1**: uses a direct reference to `Genetec.Sdk.dll` from the `$(GSC_SDK)` path.
- **.NET 8**: references the SDK from the `$(GSC_SDK_CORE)` path with these package and framework dependencies:
  - `Microsoft.Windows.Compatibility`: legacy Windows API support
  - `System.ServiceModel.Primitives`: WCF communication support
  - `Microsoft.Bcl.AsyncInterfaces`: async interface types
  - `Microsoft.WindowsDesktop.App.WPF`: WPF framework reference

`dotnet build` restores the NuGet packages declared in each project. Package lists vary by sample; inspect its `PackageReference` entries before changing dependencies. The WPF framework reference requires the .NET 8 Windows Desktop Runtime on machines running these samples, including console applications. For `net481`, install the .NET Framework 4.8.1 runtime on the machine running the sample.

The SDK references set **Copy Local** to `False`. The shared resolver loads SDK assemblies from the installed SDK or Security Center installation. Deploy the application's non-SDK dependencies with it. Choose package versions compatible with the SDK loaded at runtime; the .NET 8 package versions configured here are not a .NET 10 dependency profile. See [Shared sample helpers](../Shared/README.md#choosing-an-assembly-resolver) for assembly loading and deployment guidance.

The build reports the detected SDK paths and fails a .NET 8 build when `GSC_SDK_CORE\Genetec.Sdk.dll` is unavailable. Use the displayed SDK path to correct the installation selection.

## Running the samples

Build the configuration for the selected framework, and then run the sample.

### Building the samples

From an individual sample project folder, such as `Samples/Platform SDK/CardholderSample`, use the commands below.

#### Default .NET Framework build
```powershell
# Builds .NET Framework 4.8.1
dotnet build

# Builds and runs with .NET Framework 4.8.1
dotnet run
```

#### Explicit configuration selection
```powershell
# Build net481 with the default configuration
dotnet build -c Debug
dotnet run -c Debug

# Build net8.0-windows with the _NET8 configuration (requires SC 5.12.2+)
dotnet build -c Debug_NET8
dotnet run -c Debug_NET8
```

### Running a sample

Configure the sample before running it:

1. Edit the connection values in [SampleBase.cs](../Shared/SampleBase.cs) for samples that inherit from `SampleBase`. For other samples, edit their connection code.
2. Use Visual Studio or the dotnet commands above to build and run the sample.
3. Observe the console output explaining what the sample demonstrates.

### Troubleshooting

**Can't find .NET 8 target**

- **Cause**: the project was built with `Debug` or `Release`, which target .NET Framework 4.8.1.
- **Solution**: use `Debug_NET8` or `Release_NET8` with Security Center SDK 5.12.2 or later.

**MSB3277 dependency conflicts**

- **Cause**: the project is resolving incompatible SDK assemblies or packages.
- **Solution**: confirm that `GSC_SDK` and `GSC_SDK_CORE` point to compatible Security Center SDK installations.

The shared build properties demote MSB3277 to a build message. Resolve dependency version conflicts before deployment; a successful build does not establish that the application can load the selected assemblies or log on.

**`Genetec.Sdk.dll` not found**

- **Cause**: the environment variables are not set correctly.
- **Solution**: reinstall the Security Center SDK or check the environment variables.

**Error: `_NET8` configurations require `GSC_SDK_CORE`**

- **Cause**: the `_NET8` build cannot find `GSC_SDK_CORE\Genetec.Sdk.dll`.
- **Solution**: set `GSC_SDK_CORE` to the SDK folder containing `Genetec.Sdk.dll`, or use `Debug` or `Release` to build `net481`.

## Sample categories

The samples are grouped by the operations they demonstrate.

### Entity management samples
These samples show how to work with Security Center's entity system:
- **EntityCacheSample**: Demonstrates entity loading patterns and cache usage
- **CameraSample**: Shows camera entity properties and relationships
- **CardholderSample**: Demonstrates cardholder management and access control entities
- **DoorSample**: Shows door entities and their access control relationships

### Query samples
These samples demonstrate how to retrieve historical data using different query types:
- **ActivityTrailsSample**: Shows how to query system activity and track entity changes
- **AuditTrailsSample**: Demonstrates security audit trail queries and analysis
- **VideoFileQuerySample**: Shows how to query video archives and retrieve file information
- **SequenceQuerySample**: Demonstrates video sequence queries for playback

### Event monitoring samples
These samples show how to subscribe to and handle real-time events:
- **EventMonitoringSample**: Demonstrates real-time event subscription and handling
- **AlarmMonitoringSample**: Shows alarm state monitoring and management
- **AccessEventMonitoringSample**: Demonstrates access control event tracking

### Transaction samples
These samples show how to create and modify entities using transactions:
- **CustomEntitySample**: Shows how to create and configure custom entities
- **EntityCertificatesManagerSample**: Demonstrates certificate management operations
- **TransactionManagerSample**: Shows complex multi-entity transaction patterns

### Other integration samples
These samples demonstrate specialized SDK capabilities:
- **RequestManagerSample**: Shows inter-application communication patterns
- **DiagnosticServerSample**: Demonstrates diagnostic logging and monitoring setup
- **CustomReportQuerySample**: Shows integration with custom plugin reports

## How the Platform SDK relates to other SDKs

The classes and concepts demonstrated in these Platform SDK samples form the foundation for all other Security Center SDK development:

- **Media SDK**: these samples use the Platform SDK for entity management, including cameras and video units, and add media operations such as streaming and playback.
- **Workspace SDK**: these samples use the Platform SDK within Security Desk and Config Tool client applications.
- **Plugin SDK**: these samples extend the Platform SDK for server-side role development.
