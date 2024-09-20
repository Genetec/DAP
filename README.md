# Genetec DAP (Development Acceleration Program)

This repository contains sample projects using the Security Center SDK. The samples demonstrate various features and capabilities of the **Security Center SDK**.

## Introduction to Security Center

Security Center is Genetec's unified security platform that blends IP security systems within a single intuitive interface to simplify operations. It combines access control, video surveillance, automatic license plate recognition, communications, and analytics into one solution, enabling organizations to enhance their security operations and gain valuable insights.

## Getting Started

### 1. **Join the Development Acceleration Program (DAP)**:
Visit [Genetec's DAP](https://www.genetec.com/partners/sdk-dap) and join the program. This will provide you with access to the SDK documentation, installer, and a development license for Security Center.

### 2. **Set up a Development Environment**:

- **Install Security Center**: Ensure you have Security Center installed. You can download the installer from the [Genetec Portal](https://portal.genetec.com).
  
- **Activate Development License**: Activate the Security Center license provided by the DAP. This license allows your integration to connect to Security Center.

- **Install Security Center SDK**: The SDK contains the necessary libraries to build and run custom integrations. Download and install the Security Center SDK from the [Genetec Portal](https://portal.genetec.com). This will create the necessary environment variables automatically:
   -   `GSC_SDK`: Points to the location of the Genetec SDK for .NET Framework 4.8.1
   -   `GSC_SDK_CORE`: Points to the location of the Genetec SDK for .NET 8
   
- **Development Tools**:
   - **Visual Studio**: Ensure you have Visual Studio 2022 (or later) installed.
   - **.NET Framework 4.8.1**: The sample projects can be built using .NET Framework 4.8.1, which is supported by all versions of Security Center.
   - **.NET 8**: The sample projects can be built using .NET 8, but only with **Security Center SDK 5.12.2 or later**.

- **Build and Run the Samples**: Once your environment is ready, you can open the sample projects in Visual Studio, build them and run them.

### 3. **Explore the Samples**:
After setting up your environment, you can explore the sample projects in this repository. Each sample demonstrates a different feature or capability of the SDK, such as video management, access control, and system events.

## Sample Project Structure

The sample projects in this repository are structured as follows:

- **Target Frameworks**: The samples support .NET Framework 4.8.1. Some samples may also support .NET 8 for use with Security Center SDK 5.12.2 or later.

- **Project Output**: The sample projects typically compile to one of the following:
  - Executable files (.exe) for standalone applications
  - Class libraries (.dll) for Workspace modules and Plugins

- **SDK References**: 
  - For .NET Framework 4.8.1: The projects reference Security Center assemblies from the `$(GSC_SDK)` directory.
  - For .NET 8: The projects reference Security Center assemblies from the `$(GSC_SDK_CORE)` directory.

- **Additional Features**: 
  - Some projects include a post-build step to copy certificate files to the output directory.
  - Projects may share common code through the use of shared project items.

## Targeting .NET Framework or .NET 8

The sample projects support both .NET Framework 4.8.1 and .NET 8, depending on the Security Center SDK version. Here's how to work with different target frameworks:

1. **Check the current target frameworks:**
   - Edit the project file (.csproj) or view its properties in Visual Studio.
   - Look for the `<TargetFrameworks>` element.

2. **Target .NET Framework 4.8.1:**
   - Ensure your project file includes:
     ```xml
     <TargetFrameworks>net481</TargetFrameworks>
     ```

3. **Target .NET 8:**
   - Ensure your project file includes:
     ```xml
     <TargetFrameworks>net8.0-windows</TargetFrameworks>
     ```

4. **Target both .NET Framework 4.8.1 and .NET 8:**
   - Modify your project file to include:
     ```xml
     <TargetFrameworks>net481;net8.0-windows</TargetFrameworks>
     ```

5. **Build for a specific framework:**
   - In Visual Studio: Use the Configuration Manager to set up different build configurations for each framework (see detailed guide below).
   - Command line: Specify the target framework when building:
     ```
     dotnet build -f net481
     ```
     or
     ```
     dotnet build -f net8.0-windows
     ```

Remember to use the appropriate version of the Security Center SDK that matches your target framework. The .NET 8 target requires Security Center SDK 5.12.2 or later.

### Using Configuration Manager for Multiple Target Frameworks

1. **Open Configuration Manager:**
   - In Visual Studio, go to the "Build" menu.
   - Select "Configuration Manager" near the bottom of the dropdown.

2. **Create New Configuration:**
   - In the Configuration Manager dialog, click on the "Active solution configuration" dropdown.
   - Select "New" at the bottom of the list.
   - Name your new configuration (e.g., "Debug-net481" for .NET Framework 4.8.1 debug build).
   - Choose which existing configuration to copy settings from (usually "Debug").
   - Click "OK" to create the new configuration.

3. **Set Project Properties for the New Configuration:**
   - Right-click on your project in the Solution Explorer.
   - Select "Properties" at the bottom of the context menu.
   - In the project properties, ensure the new configuration is selected in the "Configuration" dropdown at the top.
   - Go to the "Build" tab.
   - In the "Conditional compilation symbols" field, add "NETFRAMEWORK" (without quotes) for .NET Framework builds.
   - In the "Target framework" dropdown, select ".NET Framework 4.8.1".
   - Save the changes.

4. **Repeat for .NET 8:**
   - Follow steps 2 and 3 to create a new configuration for .NET 8 (e.g., "Debug-net8").
   - In the project properties for this configuration, set the target framework to ".NET 8.0".
   - Instead of "NETFRAMEWORK", use "NET8_0" in the conditional compilation symbols.

5. **Set Up Release Configurations:**
   - Repeat steps 2-4 to create Release configurations for both frameworks.

6. **Use the Configurations:**
   - In Visual Studio's main toolbar, use the "Solution Configurations" dropdown to switch between your new configurations.
   - When you build the project, it will use the settings for the selected configuration.

7. **Conditional Code:**
   - You can now use conditional compilation in your code:
     ```csharp
     #if NETFRAMEWORK
         // .NET Framework specific code
     #elif NET8_0
         // .NET 8 specific code
     #endif
     ```

## Documentation

Complete documentation for the Security Center SDK can be found on the [Genetec Developer Portal](https://developer.genetec.com). You will need to create an account to access the documentation.

## License

Please refer to the [LICENSE](LICENSE) file for information about permissions and limitations for using these SDK samples.

## Contributing

While this repository is primarily for reference purposes, contributions such as bug reports, feature requests, and code improvements are welcome. Please follow the guidelines outlined in the [CONTRIBUTING](CONTRIBUTING.md) file.

## Support

If you encounter any issues or have questions regarding the Security Center SDK or the provided samples, please reach out to the Genetec support team through the [Genetec Technical Assistance Portal](https://portal.genetec.com/).