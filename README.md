# Genetec DAP (Development Acceleration Program)

  

This repository contains sample projects using the Security Center SDK. The samples demonstrate various features and capabilities of the **Security Center SDK**.

  

## Introduction to Security Center

  

Security Center is Genetec's unified security platform that blends IP security systems within a single intuitive interface to simplify operations. It combines access control, video surveillance, automatic license plate recognition, communications, and analytics into one solution, enabling organizations to enhance their security operations and gain valuable insights.

  

## Getting Started

  

### 1. **Join the Development Acceleration Program (DAP)**:

Visit [Genetec's DAP](https://www.genetec.com/partners/sdk-dap) and join the program. This will provide you with access to the SDK documentation, installer, and a development license for Security Center.

  

### 2. **Set up a Development Environment**:

  

-  **Install Security Center**: Ensure you have Security Center installed. You can download the installer from the [Genetec Portal](https://www.genetec.com/portal).

-  **Activate Development License**: Activate the Security Center license provided by the DAP. This license allows your integration to connect to Security Center.

  

-  **Install Security Center SDK**: The SDK contains the necessary libraries to build and run custom integrations. Download and install the Security Center SDK from the [Genetec Portal](https://www.genetec.com/portal).

   The SDK installer automatically:
   - Creates environment variables (`GSC_SDK` for .NET Framework, `GSC_SDK_CORE` for .NET 8)
   - Writes the installation path in Windows registry
   - Copies the Security Center SDK assemblies to the SDK directories
   
   See [Referencing Security Center SDK Assemblies](https://github.com/Genetec/DAP/wiki/Referencing-Security-Center-SDK-Assemblies) for assembly referencing and runtime resolution details. 

-  **Development Tools**:

-  **Visual Studio**: Ensure you have Visual Studio 2022 version 17.6 (or later) installed.

-  **.NET Framework 4.8.1**: The sample projects can be built using .NET Framework 4.8.1, which is supported by all versions of Security Center.

-  **.NET 8**: Some sample projects can be built using .NET 8. Platform SDK samples require **Security Center SDK 5.12.2 or later**; Plugin SDK .NET 8 support applies to server modules in **Security Center 5.13 or later**; Genetec Web Player samples target .NET 8 and do not use the .NET Security Center SDK.

  

-  **Build and Run the Samples**: Once your environment is ready, you can open the sample projects in Visual Studio, build them and run them.

### 3. **Explore the Samples**:

After setting up your environment, you can explore the sample projects in this repository. Each sample demonstrates a different feature or capability of the SDK, such as video management, access control, and system events.

  

## Security Center SDKs

  
The sample projects in this repository are organized into four Security Center SDKs (each building upon the foundational Platform SDK) and a separate collection of Genetec Web Player samples:

  

### Platform SDK Samples (`/Platform SDK/`)

  

The **core samples** that demonstrate fundamental Security Center SDK functionality. These samples provide the foundation that all other SDKs build upon, including entity management, event monitoring, and reporting.

  

**Key capabilities demonstrated:**

- Login and entity loading using the `SampleBase` class

- Entity creation, modification, and deletion

- Reports queries

- Event monitoring and alarm processing

- Custom fields 

- Transaction management

  

**Start here for:** Basic SDK concepts, entity operations, reporting, and core Security Center functionality.

  

### Media SDK Samples (`/Media SDK/`)

  

Video and audio processing samples that **extend the Platform SDK** with specialized media functionality. These samples demonstrate streaming, playback, PTZ control, and media management capabilities.

  

**Key capabilities demonstrated:**

- Video streaming and playback operations

- PTZ camera coordination and control

- Video export and format conversion

- Audio transmission and processing

- Overlay graphics and visual enhancements

  

**Dependencies:** Built on Platform SDK foundations for entity management and SDK connection patterns.

See the [Media SDK README](Samples/Media%20SDK/README.md).

  

### Workspace SDK Samples (`/Workspace SDK/`)

  

Client-side user interface extensions that **leverage Platform SDK entities and services** to create custom components for Security Desk and Config Tool applications.

  

**Key capabilities demonstrated:**

- Custom Security Desk tasks and pages

- Dashboard widgets and tile components

- Config Tool configuration pages

- Options extensions and UI integrations

  

**Dependencies:** Uses Platform SDK for entity access, authentication, and core SDK functionality within the client applications.

  

See the [Workspace SDK README](Samples/Workspace%20SDK/README.md).

  

### Plugin SDK Samples (`/Plugin SDK/`)

  

Server-side plugin development samples that **build upon Platform SDK infrastructure** to create custom roles with database support, failover capabilities, and deep system integration.

  

**Key capabilities demonstrated:**

- Server-side processing and custom role creation

- Database integration with upgrade and cleanup support

- Background services and business logic implementation

- Custom report generation and data management

 
**Dependencies:** Inherits Platform SDK patterns for entity management, queries, and core SDK functionality while adding server-side role capabilities.

  

See the [Plugin SDK README](Samples/Plugin%20SDK/README.md).

  

### Genetec Web Player Samples (`/Samples/Genetec Web Player/`)

  

Hosting samples for the **Genetec Web Player** (GWP), the JavaScript video player that ships with the Media Gateway. Unlike the SDK samples, these projects do not connect through the .NET Security Center SDK; they demonstrate three different application shells that load `gwp.js` from a Media Gateway and supply it with opaque camera tokens.

  

**Hosting models demonstrated:**

- WPF desktop application that hosts GWP in an embedded `WebView2` control, with token retrieval performed natively in .NET
- ASP.NET Core Minimal API application that serves a static page and proxies token requests through a server-side endpoint
- ASP.NET Core Razor Pages application that adds production-ready CSP nonce support and server-rendered configuration on top of the Minimal API pattern

  

**Prerequisites differ from the SDK samples**: GWP samples require a reachable Media Gateway, a trusted (or development) Media Gateway certificate, and CORS configuration that allows the hosting page's origin. See each sample's README for details.

  
## Sample Project Structure

  

The sample projects in this repository are structured as follows:

  

-  **Target Frameworks**: Platform SDK samples build .NET Framework 4.8.1 by default and .NET 8 through the `_NET8` configurations. Media SDK, Workspace SDK, and Plugin SDK samples target .NET Framework 4.8.1. Genetec Web Player samples target .NET 8.

  

-  **Project Output**: The sample projects typically compile to one of the following:

- Executable files (.exe) for standalone applications

- Class libraries (.dll) for Workspace modules and Plugins

  

-  **SDK References**:

- For .NET Framework 4.8.1: The Security Center SDK projects reference assemblies from the `$(GSC_SDK)` directory.

- For Platform SDK .NET 8 builds: The projects reference Security Center assemblies from the `$(GSC_SDK_CORE)` directory.

- Genetec Web Player samples do not reference the .NET Security Center SDK.

  

-  **Additional Features**:

- Some projects include a post-build step to copy certificate files to the output directory.

- Projects may share common code through the use of shared project items.

  

## Targeting .NET Framework or .NET 8

The samples do not all use the same framework-targeting model. Use the configurations already defined in the solution and project files.

### Platform SDK configuration model

Platform SDK sample projects target .NET Framework 4.8.1 by default and use explicit `_NET8` configurations for .NET 8:

```xml
<TargetFramework>net481</TargetFramework>
<TargetFramework Condition="'$(Configuration)' == 'Debug_NET8' OR '$(Configuration)' == 'Release_NET8'">net8.0-windows</TargetFramework>
<Configurations>Debug;Release;Debug_NET8;Release_NET8</Configurations>
```

Use `Debug` or `Release` to build `net481`. Use `Debug_NET8` or `Release_NET8` to build `net8.0-windows`.

```bash
dotnet build "Samples/Platform SDK/CardholderSample/CardholderSample.csproj" -c Debug
dotnet build "Samples/Platform SDK/CardholderSample/CardholderSample.csproj" -c Debug_NET8
```

These examples are scoped to a Platform SDK project so they do not run Workspace SDK or Plugin SDK post-build registration steps. For solution-wide builds, run from an elevated shell or elevated Visual Studio instance because some Workspace SDK and Plugin SDK samples write development registration entries under `HKEY_LOCAL_MACHINE`.

The `_NET8` configurations require Security Center SDK 5.12.2 or later and `GSC_SDK_CORE` pointing to the SDK folder that contains `Genetec.Sdk.dll`. Do not use `-f net8.0-windows` with the default `Debug` or `Release` configurations; select an `_NET8` configuration instead.

### Other sample groups

The Media SDK, Workspace SDK, and Plugin SDK sample projects in this repository target .NET Framework 4.8.1. The solution maps `Debug_NET8` and `Release_NET8` back to `Debug` and `Release` for those projects so that the solution configuration can focus on Platform SDK framework selection. The Genetec Web Player samples target .NET 8 and do not use the .NET Security Center SDK.

## SDK Framework Support Matrix

The following table shows which .NET frameworks are supported by each SDK or sample group:

| SDK | .NET Framework 4.8.1 | .NET 8 | Notes |
|-----|:-------------------:|:------:|-------|
| **Genetec Web Player** | ❌ | ✅ | Targets .NET 8 only; ASP.NET Core or WPF + WebView2 |
| **Platform SDK** | ✅ | ✅ | .NET 8 requires Security Center SDK 5.12.2+ |
| **Media SDK** | ✅ | ❌ | .NET 8 support planned for future release |
| **Workspace SDK** | ✅ | ❌ | Client applications use .NET Framework |
| **Plugin SDK** | ✅ | ✅ | .NET 8 support for the `ServerModule` requires Security Center 5.13+. The `ClientModule` (Config Tool / Security Desk UI) targets .NET Framework only. See [Building .NET plugins](https://github.com/Genetec/DAP/wiki/plugin-sdk-net8). |

**Important**: Only Platform SDK samples in this repository are configured to multi-target. The Plugin, Workspace, and Media SDK samples target .NET Framework 4.8.1 exclusively, even where the SDK itself supports additional runtimes (see Plugin SDK row above).

  

### Using Visual Studio configurations

Open `Samples/Genetec.Dap.CodeSamples.sln` and select one of the existing solution configurations:

- `Debug` / `Release`: Platform SDK samples build `net481`.
- `Debug_NET8` / `Release_NET8`: Platform SDK samples build `net8.0-windows`; non-Platform SDK samples continue to build their `Debug` / `Release` configurations.

Run Visual Studio as an administrator when building the full solution if you want Workspace SDK and Plugin SDK development registration to succeed.

  

## Documentation

  

Complete documentation for the Security Center SDK can be found on the [Genetec Developer Portal](https://developer.genetec.com). You will need to create an account to access the documentation.

  

## License

  

Please refer to the [LICENSE](LICENSE) file for information about permissions and limitations for using these SDK samples.

  

## Contributing

  

While this repository is primarily for reference purposes, contributions such as bug reports, feature requests, and code improvements are welcome. Please follow the guidelines outlined in the [CONTRIBUTING](CONTRIBUTING.md) file.

  

## Support

  

If you encounter any issues or have questions regarding the Security Center SDK or the provided samples, please reach out to the Genetec support team through the [Genetec Technical Assistance Portal](https://www.genetec.com/portal).
