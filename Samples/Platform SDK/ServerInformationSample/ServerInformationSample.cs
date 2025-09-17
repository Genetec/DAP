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
using Genetec.Sdk.Entities.Utilities;

public class ServerInformationSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        // Load servers into the entity cache
        await LoadEntities(engine, token, EntityType.Server);

        // Retrieve servers from the entity cache
        List<Server> servers = engine.GetEntities(EntityType.Server).OfType<Server>().ToList();

        Console.WriteLine($"\n{servers.Count} servers loaded\n");

        if (!servers.Any())
        {
            Console.WriteLine("No servers found in the system.");
            return;
        }

        foreach (Server server in servers)
        {
            IServerInformation serverInfo = server.GetServerInformation();

            Console.WriteLine("SERVER INFORMATION:");

            try
            {
                Console.WriteLine($"  Server Name: {serverInfo.GetServerName()}");
                Console.WriteLine($"  Is Main Server: {serverInfo.IsMainServer}");
                Console.WriteLine($"  Current Time: {serverInfo.GetCurrentTime().ToLocalTime():f}");
            }
            catch (Exception)
            {
                Console.WriteLine($"  Server Name: {server.FullyQualifiedName}");
                Console.WriteLine($"  Is Main Server: {server.IsMainServer}");
                Console.WriteLine($"  Current Time: {TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, server.TimeZone).ToLocalTime():f}");
            }

            await ListDrives(serverInfo);

            await ListDirectories(serverInfo);

            await ListProductFiles(serverInfo);
        }
    }

    private static async Task ListDrives(IServerInformation serverInfo)
    {
        Console.WriteLine("\nDRIVE INFORMATION:");

        IEnumerable<DriveItemInfo> drives = await serverInfo.GetDrivesAsync();
        foreach (DriveItemInfo drive in drives)
        {
            Console.WriteLine($"  {drive.Name} ({drive.DriveType})");
            Console.WriteLine($"      Label: {drive.VolumeLabel ?? "No label"}");
            Console.WriteLine($"      Format: {drive.DriveFormat ?? "Unknown"}");
            Console.WriteLine($"      Total Size: {drive.TotalSize / 1024 / 1024:N0} MB");
            Console.WriteLine($"      Available: {drive.AvailableFreeSpace / 1024 / 1024:N0} MB");
            Console.WriteLine($"      Used: {(drive.TotalSize - drive.AvailableFreeSpace) / 1024 / 1024:N0} MB");
        }
    }

    private async Task ListDirectories(IServerInformation serverInfo)
    {
        Console.WriteLine("\nDIRECTORY BROWSING:");

        string[] commonPaths = ["C:\\Program Files", "C:\\Program Files (x86)",];

        foreach (string path in commonPaths)
        {
            Console.WriteLine($"\n{path}:");

            if (!serverInfo.DirectoryExists(path))
            {
                Console.WriteLine("    Directory does not exist");
                continue;
            }

            List<DirectoryItemInfo> directories = (await serverInfo.GetDirectoriesAsync(path)).ToList();

            if (directories.Any())
            {
                Console.WriteLine($"    Subdirectories ({directories.Count}):");
                foreach (var dir in directories.Take(5))
                {
                    string hasChildren = dir.HasChildren ? " (has subdirs)" : "";
                    Console.WriteLine($"      {dir.Name}{hasChildren}");
                }
                if (directories.Count > 5)
                    Console.WriteLine($"      ... and {directories.Count - 5} more directories");
            }

            // Get files
            var files = (await serverInfo.GetFilesAsync(path)).ToList();

            if (files.Any())
            {
                Console.WriteLine($"    Files ({files.Count}):");
                foreach (var file in files.Take(3))
                {
                    Console.WriteLine($"      {file.Name} ({file.Length:N0} bytes)");
                }
                if (files.Count > 3)
                    Console.WriteLine($"      ... and {files.Count - 3} more files");
            }

            if (!directories.Any() && !files.Any())
            {
                Console.WriteLine("    (empty directory)");
            }
        }
    }

    private async Task ListProductFiles(IServerInformation serverInfo)
    {
        Console.WriteLine("\nPRODUCT FILES SEARCH:");

        var searchPatterns = new List<string> { "*.dll" };
        var productFiles = await serverInfo.GetProductFilesAsync(searchPatterns);
        var fileList = productFiles.ToList();

        if (fileList.Any())
        {
            Console.WriteLine($"  Found {productFiles.Count()} product files (showing first {fileList.Count}):");
            foreach (var file in fileList)
            {
                Console.WriteLine($"    {file.Name}");
                Console.WriteLine($"        Size: {file.Length:N0} bytes");
                Console.WriteLine($"        Version: {file.VersionInfo?.FileVersion ?? "Unknown"}");
                Console.WriteLine($"        Description: {file.VersionInfo?.FileDescription ?? "No description"}");
            }
        }
        else
        {
            Console.WriteLine("  No product files found");
        }
    }
}