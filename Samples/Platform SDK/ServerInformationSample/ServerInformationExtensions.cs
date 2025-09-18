// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System.Collections.Generic;
using System.Threading.Tasks;
using Genetec.Sdk.Entities.Utilities;

/// <summary>
/// Extension methods for IServerInformation to provide modern async/await pattern
/// instead of the traditional Begin/End asynchronous programming model (APM).
/// These methods make file system operations more readable and easier to use.
/// </summary>
public static class ServerInformationExtensions
{
    /// <summary>
    /// Gets the drives on the server asynchronously
    /// </summary>
    /// <param name="serverInfo">The server information service</param>
    /// <returns>A task containing the enumerable of drive information</returns>
    public static Task<IEnumerable<DriveItemInfo>> GetDrivesAsync(this IServerInformation serverInfo)
    {
        return Task.Factory.FromAsync(serverInfo.BeginGetDrives, serverInfo.EndGetDrives, null);
    }

    /// <summary>
    /// Gets the directories in the specified path asynchronously
    /// </summary>
    /// <param name="serverInfo">The server information service</param>
    /// <param name="parentDirectoryName">Path of the parent directory to browse</param>
    /// <returns>A task containing the enumerable of directory information</returns>
    public static Task<IEnumerable<DirectoryItemInfo>> GetDirectoriesAsync(this IServerInformation serverInfo, string parentDirectoryName)
    {
        return Task.Factory.FromAsync(
            (callback, state) => serverInfo.BeginGetDirectories(parentDirectoryName, callback, state),
            serverInfo.EndGetDirectories,
            null);
    }

    /// <summary>
    /// Gets the files in the specified directory asynchronously
    /// </summary>
    /// <param name="serverInfo">The server information service</param>
    /// <param name="directoryName">Path of the directory to browse for files</param>
    /// <returns>A task containing the enumerable of file information</returns>
    public static Task<IEnumerable<FileItemInfo>> GetFilesAsync(this IServerInformation serverInfo, string directoryName)
    {
        return Task.Factory.FromAsync(
            (callback, state) => serverInfo.BeginGetFiles(directoryName, callback, state),
            serverInfo.EndGetFiles,
            null);
    }

    /// <summary>
    /// Gets product files matching the specified search patterns asynchronously
    /// </summary>
    /// <param name="serverInfo">The server information service</param>
    /// <param name="searchPatterns">List of search patterns (e.g., "*.dll", "*.exe")</param>
    /// <returns>A task containing the enumerable of file information</returns>
    public static Task<IEnumerable<FileItemInfo>> GetProductFilesAsync(this IServerInformation serverInfo, List<string> searchPatterns)
    {
        return Task.Factory.FromAsync(
            (callback, state) => serverInfo.BeginGetProductFiles(searchPatterns, callback, state),
            serverInfo.EndGetProductFiles,
            null);
    }
}