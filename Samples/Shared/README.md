# Shared sample helpers

This folder contains source files used by the Security Center SDK samples. Projects import [Shared.projitems](Shared.projitems) to compile these files into their own assemblies. Changes to a shared file affect each importing project when it is rebuilt.

Run a sample project to use these helpers. The shared project has no executable entry point.

## Files

The shared files provide connection setup, assembly loading, and collection helpers:

| File | Purpose |
|---|---|
| [SampleBase.cs](SampleBase.cs) | Runs console samples with connection setup, logon status output, cancellation, and engine disposal. Also provides a helper for paged entity loading. |
| [SdkResolverNetFramework.cs](SdkResolverNetFramework.cs) | Locates and loads installed SDK assemblies for .NET Framework samples. |
| [SdkResolverNetCoreApp.cs](SdkResolverNetCoreApp.cs) | Locates and loads installed SDK assemblies for modern .NET samples, including package dependencies described by SDK dependency files. |
| [AssemblyResolver.cs](AssemblyResolver.cs) | Resolves requested DLLs from the directory containing the assembly that includes this helper. |
| [CollectionExtensions.cs](CollectionExtensions.cs) | Adds the items in an enumerable sequence to a collection. |
| [EnumerableExtensions.cs](EnumerableExtensions.cs) | Splits a sequence into lists of a requested size, including a final partial list when needed. |
| [IsExternalInitPolyfill.cs](IsExternalInitPolyfill.cs) | Supplies the compiler type used for init-only properties when targeting .NET Framework. |

## Running a sample with SampleBase

The base class initializes the SDK resolver, creates the engine, and logs on to Security Center. After a successful logon, it calls the sample's implementation of `RunAsync` with the connected engine and a cancellation token. Pressing **Ctrl+C** requests cancellation; the sample's operations must observe the token to stop.

To configure and run an existing sample that inherits from `SampleBase`:

1. Set the `server`, `username`, and `password` values in `SampleBase.RunAsync()`.
2. Build and run the sample project using the instructions in the [Platform SDK README](../Platform%20SDK/README.md#running-the-samples).

The connection parameters are hardcoded for convenience and to keep the examples focused on SDK concepts. These values are shared by the samples that inherit from the base class. Production applications should use a separate configuration mechanism.

The `LoadEntities` helper requests entities by type in pages of 1,000, with related-data downloads enabled. It checks for cancellation before each query and continues until a page contains fewer than 1,000 rows.

## Choosing an assembly resolver

Use `SdkResolver` for standalone samples that load SDK assemblies from an installed SDK or Security Center installation. The two source files define the same class, with conditional compilation selecting the implementation for the application's target framework.

- For .NET Framework, set `GSC_SDK` to the SDK directory.
- For modern .NET, set `GSC_SDK_CORE` to a Windows-specific SDK framework directory compatible with the application, such as `net8.0-windows`. A .NET 10 application can also use SDK assemblies built for .NET 8.

Both implementations also support discovery through Windows registration data. Set the environment variable explicitly when choosing between installed SDK versions. Build-time assembly references are configured separately in the consuming project; see the [repository README](../../README.md#targeting-net-framework-or-net-8).

Set **Copy Local** to `False` for SDK references and load the SDK assemblies from the installation on the target machine. Deploy the application's non-SDK dependencies with the application. See [Referencing SDK assemblies](https://github.com/Genetec/DAP/wiki/platform-sdk-referencing-assemblies) in the DAP wiki for reference and deployment requirements.

Samples that inherit from `SampleBase` initialize the resolver through the base class. For other standalone samples, initialize it from startup code that does not reference SDK types, before invoking code that does.

The .NET Framework resolver changes the process's current directory to the resolved installation directory. Use `AppContext.BaseDirectory` to construct paths relative to your application's location.

Security Center supplies and loads the SDK assemblies for hosted plugins and Workspace modules. Use `AssemblyResolver` for additional dependencies when a module needs custom loading behavior. Call its `Initialize` method from a static constructor before accessing types that depend on it. The helper searches its own assembly's directory; searching additional directories or applying custom version-selection rules requires changing the helper.

For plugins and Workspace modules, `AddFoldersToAssemblyProbe=True` adds lookup of dependencies beside registered modules and requires the complete assembly identity to match. A custom resolver is optional when that lookup handles the dependencies. Both mechanisms can coexist. See [About plugin assembly resolution](https://github.com/Genetec/DAP/wiki/plugin-sdk-assembly-resolution) in the DAP wiki for dependency matching and placement guidance.
