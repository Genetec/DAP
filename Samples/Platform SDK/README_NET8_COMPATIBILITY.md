# .NET 8 Compatibility Guide for Platform SDK Samples

## Overview

The Platform SDK samples support both **.NET Framework 4.8.1** and **.NET 8**, but .NET 8 support requires **Security Center 5.12.2 or later**.

## Automatic Framework Selection

The projects automatically detect your Security Center SDK version and configure the appropriate target frameworks:

### ✅ Security Center 5.12.2+ (with .NET 8 SDK)
- **Target Frameworks**: `net481` + `net8.0-windows`
- **Environment Variable**: `GSC_SDK_CORE` points to .NET 8 SDK location
- **Result**: You can build and run samples with both frameworks

### ⚠️ Security Center 5.12.1 or earlier
- **Target Frameworks**: `net481` only
- **Environment Variable**: `GSC_SDK_CORE` not set or path doesn't exist
- **Result**: Only .NET Framework 4.8.1 builds are available

## Building the Samples

### Automatic Detection (Recommended)
```bash
# Builds all available target frameworks for your SC version
dotnet build

# Builds and runs with the best available framework
dotnet run
```

### Explicit Framework Selection
```bash
# Force .NET Framework 4.8.1 (works with any SC version)
dotnet build --framework net481
dotnet run --framework net481

# Force .NET 8 (requires SC 5.12.2+)
dotnet build --framework net8.0-windows
dotnet run --framework net8.0-windows
```

## Environment Variables

The build system checks these environment variables:

- **`GSC_SDK`**: Points to .NET Framework SDK (legacy, works with all SC versions)
- **`GSC_SDK_CORE`**: Points to .NET 8 SDK (requires SC 5.12.2+)

## Error Messages

If you try to build .NET 8 with an incompatible Security Center version:

```
warning: .NET 8 target requires Security Center 5.12.2 or later.
Please install the compatible SDK or use .NET Framework 4.8.1 instead.
```

## Troubleshooting

### "Can't find .NET 8 target"
- **Cause**: Security Center 5.11.x or earlier
- **Solution**: Use `--framework net481` or upgrade to SC 5.12.2+

### "MSB3277 dependency conflicts"
- **Cause**: You manually forced .NET 8 on an incompatible system
- **Solution**: Let the auto-detection handle framework selection

### "Genetec.Sdk.dll not found"
- **Cause**: Environment variables not set correctly
- **Solution**: Reinstall Security Center SDK or check environment variables

## Advanced: Manual Override

For testing or special scenarios, you can override the auto-detection:

```xml
<!-- Force include .NET 8 regardless of detection -->
<PropertyGroup>
  <TargetFrameworks>net481;net8.0-windows</TargetFrameworks>
</PropertyGroup>
```

⚠️ **Warning**: This may cause build errors on incompatible systems.