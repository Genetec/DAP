# Genetec DAP (Development Acceleration Program)

  

This repository contains Security Center SDK samples for entity management, reports, events, media, plugin roles, and client extensions. It also includes Genetec™ Web Player hosting samples.

  

## Introduction to Security Center

  

Security Center combines access control, video surveillance, automatic license plate recognition, communications, and analytics. Use its SDKs to build standalone integrations, server-side roles, and extensions for Security Desk and Config Tool.

  

## SDK runtime support

Choose the SDK and runtime for your integration before selecting a sample. The SDK runtime requirements below differ from the targets configured in this repository's projects.

| SDK | .NET Framework | .NET 8 | .NET 10 |
|-----|----------------|--------|---------|
| Platform SDK, standalone applications | Supported; SDK assemblies target .NET Framework 4.8 | Supported from Security Center SDK 5.12.2; reference `net8.0-windows` SDK assemblies | Use Security Center SDK 5.14.1 or later for `net10.0-windows` SDK assemblies. A .NET 10 application can also reference compatible .NET 8 SDK assemblies from SDK 5.12.2 or later. |
| Media SDK | Supported; the samples target .NET Framework 4.8.1 | Not supported | Not supported |
| Workspace SDK | Required for modules loaded by Security Desk or Config Tool | Not supported for client modules | Not supported for client modules |
| Plugin SDK, server modules | Supported | Requires Security Center 5.13 or later on the role server and compatible modern SDK assemblies | Requires Security Center 5.14.1 or later on the role server and compatible modern SDK assemblies |

A plugin's `ClientModule` uses the Workspace SDK and must target .NET Framework, even when its `ServerModule` targets .NET 8 or .NET 10. On Security Center 5.13, a modern server module also requires a .NET Framework `Client` discovery assembly on the server. See [Building modern .NET plugins](#building-modern-net-plugins) for deployment guidance.

SDK assembly compatibility and the runtime installed on the role server are separate requirements. A .NET 8 application cannot reference SDK assemblies built for .NET 10. A .NET 10 application can reference compatible SDK assemblies built for .NET 8 or .NET 10. For application and server version compatibility, see [About SDK compatibility](https://github.com/Genetec/DAP/wiki/platform-sdk-compatibility).

## Getting started

Prepare a development environment before building the samples.

### 1. Joining the Development Acceleration Program (DAP)

Visit [Genetec DAP](https://www.genetec.com/partners/sdk-dap) and join the program. Membership provides access to the SDK documentation, installer, and a development license for Security Center.

  

### 2. Setting up a development environment

  

-  **Install Security Center**: Ensure you have Security Center installed. You can download the installer from the [Genetec Portal](https://www.genetec.com/portal).

-  **Activate Development License**: Activate the Security Center license provided by the DAP. This license allows your integration to connect to Security Center.

  

-  **Install Security Center SDK**: The SDK contains the necessary libraries to build and run custom integrations. Download and install the Security Center SDK from the [Genetec Portal](https://www.genetec.com/portal).

   The SDK installer automatically:

   - Creates environment variables (`GSC_SDK` for .NET Framework, `GSC_SDK_CORE` for modern .NET)
   - Writes the installation path in the Windows registry
   - Copies the Security Center SDK assemblies to the SDK directories
   
   See [Referencing Security Center SDK assemblies](https://github.com/Genetec/DAP/wiki/platform-sdk-referencing-assemblies) for assembly references, runtime resolution, and dependency deployment.

#### Development tools

Install the build tools for the framework you select:

- **Visual Studio**: Use Visual Studio 2022 version 17.8 or later for the existing samples, including .NET Framework projects, because they compile with C# 12. Use Visual Studio 2026 or later for a project targeting .NET 10.

-  **.NET Framework targeting pack**: Install the .NET Framework 4.8.1 targeting pack for sample projects targeting `net481`.

-  **.NET 8**: Platform SDK samples require Security Center SDK 5.12.2 or later with `net8.0-windows` assemblies. Set `GSC_SDK_CORE` to that folder. Plugin SDK server modules require Security Center 5.13 or later, but the Plugin samples here target .NET Framework. Genetec Web Player samples target .NET 8 and do not use the .NET Security Center SDK.

-  **.NET SDK**: For command-line builds, install the .NET 8 SDK or a later SDK that supports the sample's target, including when building `net481`. A project targeting .NET 10 requires the .NET 10 SDK or a later compatible SDK. Run `dotnet --list-sdks` to check installed SDKs. Installing a newer SDK does not change the target selected by the project configuration.

  

- **Build and run the samples**: Open the sample projects in Visual Studio, build them, and run them.

### 3. Exploring the samples

Each sample demonstrates an SDK capability, such as video management, access control, or system events.

  

## Security Center SDKs

  
The samples are grouped by SDK: Platform, Media, Workspace, and Plugin. The Media, Workspace, and Plugin SDKs build on the Platform SDK. Genetec Web Player samples form a separate group:

  

### Platform SDK samples (`/Platform SDK/`)

  

The Platform SDK samples demonstrate entity management, event monitoring, and reporting. These capabilities provide a foundation for Media SDK, Workspace SDK, and Plugin SDK integrations.

  

**Key capabilities demonstrated:**

- Logging on and loading entities using the `SampleBase` class

- Entity creation, modification, and deletion

- Report queries

- Event monitoring and alarm processing

- Custom fields 

- Transaction management

  

**Start here for:** Basic SDK concepts, entity operations, reporting, and core Security Center functionality.

See the [Platform SDK README](Samples/Platform%20SDK/README.md) for prerequisites, dependencies, and build commands.

  

### Media SDK samples (`/Media SDK/`)

  

Video and audio processing samples extend the Platform SDK with specialized media functionality. These samples demonstrate streaming, playback, PTZ control, and media management capabilities.

  

**Key capabilities demonstrated:**

- Video streaming and playback operations

- PTZ camera coordination and control

- Video export and format conversion

- Audio transmission and processing

- Overlay graphics and visual enhancements

  

**Dependencies:** Built on Platform SDK foundations for entity management and SDK connection patterns.

See the [Media SDK README](Samples/Media%20SDK/README.md).

  

### Workspace SDK samples (`/Workspace SDK/`)

  

These client-side user interface extensions use Platform SDK entities and services to add components to Security Desk and Config Tool. These modules must target .NET Framework because they run inside the client applications.

  

**Key capabilities demonstrated:**

- Custom Security Desk tasks and pages

- Dashboard widgets and tile components

- Config Tool configuration pages

- Options extensions and UI integrations

  

**Dependencies:** Uses Platform SDK for entity access, authentication, and core SDK functionality within the client applications.

  

See the [Workspace SDK README](Samples/Workspace%20SDK/README.md).

  

### Plugin SDK samples (`/Plugin SDK/`)

  

Server-side plugin development samples build upon Platform SDK infrastructure to create custom roles with database support, failover capabilities, and system integration.

  

**Key capabilities demonstrated:**

- Server-side processing and custom role creation

- Database integration with upgrade and cleanup support

- Background services and business logic implementation

- Custom report generation and data management

 
**Dependencies:** Uses Platform SDK patterns for entity management, queries, and core SDK functionality while adding server-side role capabilities.

  

See the [Plugin SDK README](Samples/Plugin%20SDK/README.md).

  

### Genetec Web Player samples (`/Samples/Genetec Web Player/`)

  

Hosting samples for the **Genetec Web Player** (GWP), the JavaScript video player that ships with the Media Gateway. Unlike the SDK samples, these projects do not connect through the .NET Security Center SDK; they demonstrate three different application shells that load `gwp.js` from a Media Gateway and supply it with opaque camera tokens.

  

**Hosting models demonstrated:**

- Windows Presentation Foundation (WPF) desktop application that hosts GWP in an embedded `WebView2` control, with token retrieval performed natively in .NET
- ASP.NET Core Minimal API application that serves a static page and proxies token requests through a server-side endpoint
- ASP.NET Core Razor Pages application that adds CSP nonce support and server-rendered configuration on top of the Minimal API pattern

  

GWP samples require a reachable Media Gateway, a trusted or development Media Gateway certificate, and CORS configuration that allows the hosting page's origin. See each sample's README for details.

  
## Sample project structure

  

The sample projects in this repository are structured as follows:

  

-  **Target Frameworks**: Platform SDK samples build .NET Framework 4.8.1 by default and .NET 8 through the `_NET8` configurations. Media SDK, Workspace SDK, and Plugin SDK samples target .NET Framework 4.8.1. Genetec Web Player samples target .NET 8.

  

### Project output

The sample projects typically produce these output types:

- Executable files with the EXE extension for standalone applications
- Class libraries with the DLL extension for Workspace modules and plugins

### SDK references

SDK reference paths depend on the target:

- For .NET Framework 4.8.1: the Security Center SDK projects reference assemblies from the `$(GSC_SDK)` directory.
- For Platform SDK .NET 8 builds: the projects reference Security Center assemblies from the `$(GSC_SDK_CORE)` directory.
- Genetec Web Player samples do not reference the .NET Security Center SDK.

### Additional features

Projects can include certificate-copy steps and shared source files:

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

```powershell
dotnet build "Samples/Platform SDK/CardholderSample/CardholderSample.csproj" -c Debug
dotnet build "Samples/Platform SDK/CardholderSample/CardholderSample.csproj" -c Debug_NET8
```

These examples are scoped to a Platform SDK project so they do not run Workspace SDK or Plugin SDK post-build registration steps. For solution-wide builds, run from an elevated terminal or elevated Visual Studio instance because some Workspace SDK and Plugin SDK samples write development registration entries under `HKEY_LOCAL_MACHINE`.

Run these commands from the repository root. For example, to select the installed SDK 5.13 .NET 8 assemblies for the current terminal:

```powershell
$env:GSC_SDK_CORE = 'C:\Program Files (x86)\Genetec Security Center 5.13 SDK\net8.0-windows'
dotnet build "Samples/Platform SDK/CardholderSample/CardholderSample.csproj" -c Debug_NET8
```

Replace the installation path with your SDK's `net8.0-windows` folder. The `_NET8` configurations require Security Center SDK 5.12.2 or later with assemblies targeting .NET 8. Another SDK installation can change `GSC_SDK_CORE` to a `net10.0-windows` folder, which cannot be used by these .NET 8 configurations.

Do not use `-f net8.0-windows` with the default `Debug` or `Release` configurations; select an `_NET8` configuration instead.

### Other sample groups

The Media SDK, Workspace SDK, and Plugin SDK sample projects in this repository target .NET Framework 4.8.1. The solution maps `Debug_NET8` and `Release_NET8` back to `Debug` and `Release` for those projects so that the solution configuration can focus on Platform SDK framework selection. The Genetec Web Player samples target .NET 8 and do not use the .NET Security Center SDK.

## Sample target frameworks

This table describes the projects in this repository. SDK runtime support can extend beyond the targets configured in these samples.

| Sample group | Configured targets | Selection |
|--------------|--------------------|-----------|
| Platform SDK | `net481`, `net8.0-windows` | `Debug`/`Release` or `Debug_NET8`/`Release_NET8` |
| Media SDK | `net481` | `Debug` or `Release` |
| Workspace SDK | `net481` | `Debug` or `Release` |
| Plugin SDK | `net481` | `Debug` or `Release` |
| Genetec Web Player | `net8.0` for web projects; `net8.0-windows` for the desktop project | `Debug` or `Release` |

The Platform SDK samples select one target per build configuration. Installing the .NET 10 SDK does not create a .NET 10 sample configuration. The solution has no `Debug_NET10` or `Release_NET10` configuration.

### Using Visual Studio configurations

Open `Samples/Genetec.Dap.CodeSamples.sln` and select one of the existing solution configurations:

- `Debug` / `Release`: Platform SDK samples build `net481`.
- `Debug_NET8` / `Release_NET8`: Platform SDK samples build `net8.0-windows`; non-Platform SDK samples continue to build their `Debug` / `Release` configurations.

Run Visual Studio as an administrator when building the full solution if you want Workspace SDK and Plugin SDK development registration to succeed.

### Building your own .NET 10 application

For a standalone Platform SDK application targeting .NET 10:

1. Install the .NET 10 SDK and, for Visual Studio builds, Visual Studio 2026 or later.
2. Set the application's `TargetFramework` to `net10.0-windows`. If its SDK references or packages are conditional on `net8.0-windows`, update those conditions to include the new target.
3. Set `GSC_SDK_CORE` to compatible modern SDK assemblies. For SDK 5.14.1 or later with .NET 10 assemblies, select the SDK's `net10.0-windows` folder. A .NET 10 application can also use compatible `net8.0-windows` SDK assemblies.
4. Update the application's package and framework references for the selected SDK.
5. Set **Copy Local** to `False` for Genetec SDK assemblies.
6. Initialize the modern SDK resolver before using SDK types.
7. Include the required non-SDK packages in deployment.
8. From the application project folder, build the target:

   ```powershell
   dotnet build YOUR-PROJECT.csproj -c Debug -f net10.0-windows
   ```

Replace `YOUR-PROJECT.csproj` with your application's project file. These steps require project changes; passing `-f net10.0-windows` to an unchanged sample does not add the target or its SDK references. For project changes and Media SDK restrictions, see [Migrating a Platform SDK application to modern .NET](https://github.com/Genetec/DAP/wiki/platform-sdk-migrating-to-modern-dotnet). For the SDK 5.14 .NET 10 package references and serialization settings, see [Starting an ASP.NET Core application](https://github.com/Genetec/DAP/wiki/platform-sdk-aspnet-hosting#starting-an-aspnet-core-application).

## Build and deployment dependencies

The .NET SDK supplies the compiler and build tools. The Security Center SDK supplies the Genetec assemblies and installation components. Install both for samples that reference the .NET Security Center SDK. Genetec Web Player samples require the .NET SDK and a reachable Media Gateway and do not reference the .NET Security Center SDK.

| Dependency | Build machine | Machine running the sample |
|------------|---------------|----------------------------|
| .NET Framework 4.8.1 | Targeting pack for `net481` projects | .NET Framework 4.8.1 runtime |
| .NET 8 | .NET 8 SDK or a later compatible SDK for .NET 8 projects | .NET 8 Windows Desktop Runtime for standalone Platform samples and the GWP desktop sample; ASP.NET Core 8 runtime for the GWP web samples |
| .NET 10 | .NET 10 SDK or a later compatible SDK for your own .NET 10 project | .NET 10 Windows Desktop Runtime for standalone Platform applications using WPF framework references |
| Security Center SDK assemblies | Installed SDK, selected through `GSC_SDK` or `GSC_SDK_CORE` | Installed SDK or compatible Security Center installation; hosted plugins and Workspace modules use their host's assemblies |
| NuGet packages | `dotnet build` restores the packages declared in each project | Deploy required non-SDK dependencies with the application or module |

The standalone Platform samples reference `Microsoft.WindowsDesktop.App.WPF` for .NET 8, including console projects. Installing only the base .NET runtime does not supply that framework. Check installed runtimes with `dotnet --list-runtimes`. A .NET 10 SDK can build a .NET 8 project, but running that project still requires the .NET 8 runtime and any shared frameworks it references.

Keep **Copy Local** set to `False` for Genetec SDK references. Standalone applications use the shared SDK resolver to load installed SDK assemblies. Deploy non-SDK dependencies with the application rather than relying on the developer's NuGet cache. Hosted plugins and Workspace modules load SDK assemblies supplied by Security Center and require their non-SDK dependencies beside their registered modules. See [Shared sample helpers](Samples/Shared/README.md#choosing-an-assembly-resolver) for resolver and dependency placement.

Security Center installs the runtimes required by its hosted processes. For the packages included with Security Center 5.14.1, see [Security Center installation prerequisites](https://techdocs.genetec.com/r/en-US/Security-Center-Installation-and-Upgrade-Guide-5.14.1.0/Security-Center-5.14.1.0-installation-prerequisites) on Genetec TechDocs.

  

## Building modern .NET plugins

The Plugin SDK samples in this repository target .NET Framework. For a modern .NET integration, choose the project and deployment instructions for the Security Center version where the role will run.

### Security Center 5.14 and later

Build a modern .NET ServerModule for server-side logic and an optional .NET Framework ClientModule for custom UI in Config Tool or Security Desk. Register the ServerModule on the server hosting the role and the ClientModule on workstations that use its UI. Security Center 5.14.0 supports .NET 8 role plugins; .NET 10 role hosting requires 5.14.1 or later.

See [Building plugins for Security Center 5.14 and later](https://github.com/Genetec/DAP/wiki/plugin-sdk-net8#security-center-514-and-later) and [Deploying plugins](https://github.com/Genetec/DAP/wiki/plugin-sdk-deployment#security-center-514-and-later), including the deployment requirement for modules that declare custom privileges.

### Security Center 5.13

Include a .NET Framework Client discovery assembly alongside the modern .NET ServerModule on the server. Config Tool requests the plugin list from that server. Keep any ClientModule targeting .NET Framework for its client-side UI.

See [Building plugins for Security Center 5.13](https://github.com/Genetec/DAP/wiki/plugin-sdk-net8#security-center-513) for the project structures and registration requirements.

## Documentation

  

See the [Genetec Developer Portal](https://developer.genetec.com) for the Security Center SDK documentation. Create an account to access the documentation.

  

## License

  

See [LICENSE](LICENSE) for permissions and limitations on using these SDK samples.

  

## Contributing

  

Contributions such as bug reports, feature requests, and code improvements are welcome. Follow the guidelines in [CONTRIBUTING](CONTRIBUTING.md).

  

## Support

  

For issues or questions about the Security Center SDK or these samples, contact the Genetec support team through the [Genetec Technical Assistance Portal](https://www.genetec.com/portal).
