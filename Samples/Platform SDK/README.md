
# Platform SDK Samples

The Platform SDK provides the foundation for all Security Center integrations, offering core functionality for authentication, entity management, and system interactions. This collection of samples demonstrates the essential patterns and capabilities that serve as building blocks for Media SDK, Workspace SDK, and Plugin SDK development.

## Understanding the SDK Components

### The Engine Class

The `Engine` class is the central component you'll see in every Platform SDK sample. It serves as the main entry point for all SDK operations and provides:

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

### Entity Management

All Platform SDK samples work with Security Center's client-side entity cache:

- **Entity Cache**: A local storage system that samples use to access Security Center objects efficiently
- **Entity Loading**: Samples retrieve and cache entities using queries to avoid repeated server requests
- **Entity Relationships**: Samples can navigate between related entities (e.g., from cameras to their associated doors)

## The SampleBase Pattern

Most Platform SDK samples inherit from the `SampleBase` class, which implements the Template Method pattern to provide consistent infrastructure across all samples.

### What SampleBase Provides

**The SampleBase class handles common SDK operations so individual samples can focus on demonstrating specific features:**

1. **SDK Initialization**: Automatically calls `SdkResolver.Initialize()` to configure assembly loading
2. **Connection Management**: Handles server connection and authentication with comprehensive error handling
3. **Event Handling**: Sets up connection state monitoring and provides user feedback
4. **Cancellation Support**: Implements Ctrl+C handling for graceful shutdown
5. **Resource Cleanup**: Ensures proper disposal of SDK resources when samples exit

### How Samples Use SampleBase

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

### The LoadEntities Helper Method

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

## Configuration and Connection

### Connection Parameters in Samples
All samples use hardcoded connection parameters for demonstration purposes:

```csharp
// Standard connection pattern across all samples
const string server = "localhost";
const string username = "admin";  
const string password = "";
```

This hardcoded approach makes samples easy to run but demonstrates basic connection concepts. The samples focus on SDK feature demonstration rather than production-ready configuration management.

### Authentication Patterns in Samples
All samples follow consistent authentication patterns:
- Check `ConnectionStateCode` before proceeding with operations
- Display connection status and errors to the user
- Handle authentication failures gracefully with clear error messages

## Prerequisites

- **.NET Framework 4.8.1 or .NET 8**: Platform SDK supports both frameworks (.NET 8 requires Security Center SDK 5.12.2+)
- **Security Center SDK**: Installed with environment variables configured:
  - `GSC_SDK`: Points to SDK location for .NET Framework
  - `GSC_SDK_CORE`: Points to SDK location for .NET 8 (if using .NET 8)
- **Visual Studio 2022**: Version 17.6 or later for development
- **Security Center**: Installed and running on your system
- **Valid Security Center License**: All samples include the development SDK certificate

### .NET 8 Compatibility

The Platform SDK samples support both **.NET Framework 4.8.1** and **.NET 8**, but .NET 8 support requires **Security Center 5.12.2 or later**.

#### Build Configuration Model

The projects use explicit build configurations for framework targeting:

- `Debug` / `Release`: build .NET Framework 4.8.1.
- `Debug_NET8` / `Release_NET8`: build .NET 8 for Windows.

.NET 8 builds require Security Center SDK 5.12.2 or later. If `GSC_SDK_CORE` is not set to a compatible SDK folder, the `_NET8` configurations fail with a clear build error instead of silently falling back to .NET Framework.

#### Environment Variables

The build system checks these environment variables:

- **`GSC_SDK`**: Points to .NET Framework SDK (legacy, works with all SC versions)
- **`GSC_SDK_CORE`**: Points to .NET 8 SDK (requires SC 5.12.2+)

#### Technical Implementation

The Platform SDK samples use conditional target frameworks in their `.csproj` files to map each build configuration to one framework:

**Configuration-Based Framework Selection:**
```xml
<TargetFramework>net481</TargetFramework>
<TargetFramework Condition="'$(Configuration)' == 'Debug_NET8' OR '$(Configuration)' == 'Release_NET8'">net8.0-windows</TargetFramework>
<Configurations>Debug;Release;Debug_NET8;Release_NET8</Configurations>
```

**Framework-Specific Dependencies:**
- **.NET Framework 4.8.1**: Uses direct reference to `Genetec.Sdk.dll` from `$(GSC_SDK)` path
- **.NET 8**: References SDK from `$(GSC_SDK_CORE)` path plus compatibility packages:
  - `Microsoft.Windows.Compatibility` - Legacy Windows API support
  - `System.ServiceModel.Primitives` - WCF communication support
  - `Microsoft.Bcl.AsyncInterfaces` - Async/await compatibility
  - `Microsoft.WindowsDesktop.App.WPF` - WPF framework reference

**Build Configurations:**
Each project supports multiple build configurations:
- `Debug` / `Release`: build .NET Framework 4.8.1.
- `Debug_NET8` / `Release_NET8`: build .NET 8 (requires `GSC_SDK_CORE`, available in Security Center SDK 5.12.2 or later).

**Validation System:**
The build process includes automatic validation that:
- Shows environment variable detection status during compilation
- Fails .NET 8 builds when `GSC_SDK_CORE\Genetec.Sdk.dll` is not available
- Provides clear guidance on resolving compatibility issues

## Running the Samples

### Building the Samples

#### Default .NET Framework Build
```bash
# Builds .NET Framework 4.8.1
dotnet build

# Builds and runs with .NET Framework 4.8.1
dotnet run
```

#### Explicit Configuration Selection
```bash
# Build net481 with the default configuration
dotnet build -c Debug
dotnet run -c Debug

# Build net8.0-windows with the _NET8 configuration (requires SC 5.12.2+)
dotnet build -c Debug_NET8
dotnet run -c Debug_NET8
```

### Running a Sample
1. **Update Connection Parameters**: Edit the hardcoded connection values in the sample to match your Security Center setup
2. **Build and Run**: Use Visual Studio or the dotnet commands above to execute the sample
3. **Observe the Output**: Each sample provides console output explaining what it's demonstrating

### Troubleshooting

**"Can't find .NET 8 target"**
- **Cause**: The project was built with `Debug` or `Release`, which target .NET Framework 4.8.1
- **Solution**: Use `Debug_NET8` or `Release_NET8` with Security Center SDK 5.12.2 or later

**"MSB3277 dependency conflicts"**
- **Cause**: The project is resolving incompatible SDK assemblies or packages
- **Solution**: Confirm that `GSC_SDK` and `GSC_SDK_CORE` point to compatible Security Center SDK installations

**"Genetec.Sdk.dll not found"**
- **Cause**: Environment variables not set correctly
- **Solution**: Reinstall Security Center SDK or check environment variables

**Error: _NET8 configurations require GSC_SDK_CORE**
- **Cause**: Trying to build an `_NET8` configuration without `GSC_SDK_CORE\Genetec.Sdk.dll`
- **Solution**: Set `GSC_SDK_CORE` to the SDK folder containing `Genetec.Sdk.dll`, or use `Debug` / `Release` to build `net481`

## Sample Categories and What They Demonstrate

### Entity Management Samples
These samples show how to work with Security Center's entity system:
- **EntityCacheSample**: Demonstrates entity loading patterns and cache usage
- **CameraSample**: Shows camera entity properties and relationships
- **CardholderSample**: Demonstrates cardholder management and access control entities
- **DoorSample**: Shows door entities and their access control relationships

### Query Samples  
These samples demonstrate how to retrieve historical data using different query types:
- **ActivityTrailsSample**: Shows how to query system activity and track entity changes
- **AuditTrailsSample**: Demonstrates security audit trail queries and analysis
- **VideoFileQuerySample**: Shows how to query video archives and retrieve file information
- **SequenceQuerySample**: Demonstrates video sequence queries for playback

### Event Monitoring Samples
These samples show how to subscribe to and handle real-time events:
- **EventMonitoringSample**: Demonstrates real-time event subscription and handling
- **AlarmMonitoringSample**: Shows alarm state monitoring and management
- **AccessEventMonitoringSample**: Demonstrates access control event tracking

### Transaction Samples
These samples show how to create and modify entities using transactions:
- **CustomEntitySample**: Shows how to create and configure custom entities
- **EntityCertificatesManagerSample**: Demonstrates certificate management operations
- **TransactionManagerSample**: Shows complex multi-entity transaction patterns

### Advanced Integration Samples
These samples demonstrate specialized SDK capabilities:
- **RequestManagerSample**: Shows inter-application communication patterns
- **DiagnosticServerSample**: Demonstrates diagnostic logging and monitoring setup
- **CustomReportQuerySample**: Shows integration with custom plugin reports

## How Platform SDK Relates to Other SDKs

The classes and concepts demonstrated in these Platform SDK samples form the foundation for all other Security Center SDK development:

- **Media SDK**: These samples build on Platform SDK entity management (cameras, video units) and add media-specific operations like streaming and playback
- **Workspace SDK**: These samples use the Platform SDK within Security Desk and Config Tool client applications
- **Plugin SDK**: These samples extend Platform SDK for server-side role development
