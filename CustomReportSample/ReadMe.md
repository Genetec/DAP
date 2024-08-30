
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