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
6. **Resource Cleanup**: Ensures proper disposal of SDK resources when samples exit

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

## Running the Samples

### Prerequisites
Before running any Platform SDK sample, ensure you have:
- Security Center installed and running on your system
- SDK installed with proper environment variables configured
- Valid Security Center user credentials with appropriate privileges

### Running a Sample
1. **Update Connection Parameters**: Edit the hardcoded connection values in the sample to match your Security Center setup
2. **Build and Run**: Use Visual Studio or `dotnet run` to execute the sample
3. **Observe the Output**: Each sample provides console output explaining what it's demonstrating

### Common Issues When Running Samples
- **Connection Failed**: Verify the server address and ensure Security Center is running
- **Access Denied**: Check that the user account has privileges for the entities the sample tries to access
- **No Entities Loaded**: Verify that the requested entity types exist in your Security Center system

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
- **Workspace SDK**: These samples the Platform SDK within Security Desk and Config Tool client applications  
- **Plugin SDK**: These samples extend Platform SDK for server-side role development
