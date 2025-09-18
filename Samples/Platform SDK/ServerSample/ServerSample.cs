// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;

public class ServerSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        // Load servers and roles into the entity cache
        await LoadEntities(engine, token, EntityType.Role, EntityType.Server);

        // Retrieve servers from the entity cache
        List<Server> servers = engine.GetEntities(EntityType.Server).OfType<Server>().ToList();

        // Retrieve roles from the entity cache
        List<Role> roles = engine.GetEntities(EntityType.Role).OfType<Role>().ToList();

        Console.WriteLine($"\n{servers.Count} servers loaded");
        Console.WriteLine($"{roles.Count} roles loaded\n");

        DisplayServerHierarchy(servers, roles, engine);
    }

    private void DisplayServerHierarchy(List<Server> servers, List<Role> roles, Engine engine)
    {
        // Display main servers first
        var mainServers = servers.Where(server => server.IsMainServer).ToList();
        var failoverServers = servers.Where(server => !server.IsMainServer).ToList();

        if (mainServers.Any())
        {
            Console.WriteLine("\nMAIN SERVERS:");
            foreach (var server in mainServers)
            {
                DisplayServerInfo(server);
                DisplayRolesOnServer(server, roles, engine);
                Console.WriteLine();
            }
        }

        if (failoverServers.Any())
        {
            Console.WriteLine("\nFAILOVER SERVERS:");
            foreach (var server in failoverServers)
            {
                DisplayServerInfo(server);
                DisplayRolesOnServer(server, roles, engine);
                Console.WriteLine();
            }
        }

        if (!servers.Any())
        {
            Console.WriteLine("No servers found in the system.");
        }
    }

    private void DisplayServerInfo(Server server)
    {
        Console.WriteLine($"\nServer: {server.FullyQualifiedName}");
        Console.WriteLine($"  Type: {(server.IsMainServer ? "Main Server" : "Failover Server")}");
        Console.WriteLine($"  Time Zone: {server.TimeZone}");

        try
        {
            Console.WriteLine($"  Current Time: {server.GetCurrentTime():f}");
        }
        catch (Exception)
        {
            Console.WriteLine($"  Current Time: Not available (server may be offline)");
        }


        Console.WriteLine($"  Version: {server.Version}");
        Console.WriteLine($"  Public IP: {server.PublicEndPoint ?? "Not Available"}");
        Console.WriteLine($"  Web Port: {server.WebPort}");
        Console.WriteLine($"  Secure Web Port: {server.SecureWebPort}");

        if (server.PrivateEndPoints.Any())
        {
            Console.WriteLine("  Private IP Addresses:");
            foreach (var endpoint in server.PrivateEndPoints)
            {
                Console.WriteLine($"    - {endpoint}");
            }
        }
        else
        {
            Console.WriteLine("  Private IP Addresses: None available");
        }
    }

    private void DisplayRolesOnServer(Server server, List<Role> roles, Engine engine)
    {
        // Find roles running on this server
        var rolesOnServer = roles.Where(r => r.RoleServers.Contains(server.Guid) || r.CurrentServer == server.Guid).ToList();

        if (rolesOnServer.Any())
        {
            Console.WriteLine("  Roles running on this server:");
            foreach (Role role in rolesOnServer)
            {
                bool isCurrent = role.CurrentServer == server.Guid;
                string status = role.IsOnline ? "Online" : "Offline";
                string currentIndicator = isCurrent ? " (Current)" : "";

                Console.WriteLine($"    - {role.Name} ({role.Type})");
                Console.WriteLine($"      Status: {status}{currentIndicator}");
                Console.WriteLine($"      Running State: {role.RunningState}");

                if (!string.IsNullOrEmpty(role.Description))
                {
                    Console.WriteLine($"      Description: {role.Description}");
                }

                try
                {
                    // Display network binding for the role
                    if (role.CurrentNetworkBinding?.IpAddress != null)
                    {
                        Console.WriteLine($"      IP Address: {role.CurrentNetworkBinding.IpAddress}");
                    }
                }
                catch (SdkException ex) when (ex.ErrorCode == SdkError.InvalidOperation)
                {
                    Console.WriteLine($"      IP Address: Not available (role may be offline)");
                }

                // Display database information if available
                if (!string.IsNullOrEmpty(role.DatabaseServer))
                {
                    Console.WriteLine($"      Database Information:");
                    Console.WriteLine($"        Server: {role.DatabaseServer}");
                    Console.WriteLine($"        Name: {role.DatabaseName}");
                    Console.WriteLine($"        Encrypted Connections: {role.EnableEncryptionToDatabase}");
                    Console.WriteLine($"        Validate Certificate: {!role.TrustServerCertificate}");
                }

                Console.WriteLine();
            }
        }
        else
        {
            Console.WriteLine("  No roles currently running on this server");
        }
    }
}