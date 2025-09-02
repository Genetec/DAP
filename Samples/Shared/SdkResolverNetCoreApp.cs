// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

#if NET8_0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Win32;
using System.Collections.Concurrent;

public static class SdkResolver
{
    private static readonly string s_probingPath = GetProbingPath();
    private static readonly AssemblyDependencyResolver s_dependencyResolver = new(Path.Combine(s_probingPath, "Genetec.Sdk.dll"));
    private static readonly ConcurrentDictionary<string, Assembly> s_loadedAssemblies = new();
    private static readonly HashSet<string> s_assemblyResolutionInProgress = new();

    public static void Initialize()
    {
        AssemblyLoadContext.Default.Resolving += OnAssemblyResolve;
    }

    private static Assembly OnAssemblyResolve(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        if (s_loadedAssemblies.TryGetValue(assemblyName.Name, out Assembly loadedAssembly))
        {
            return loadedAssembly; // Assembly already loaded
        }

        if (!s_assemblyResolutionInProgress.Add(assemblyName.Name))
        {
            return null; // Recursive resolution detected
        }

        try
        {
            string assemblyPath = s_dependencyResolver.ResolveAssemblyToPath(assemblyName);
            if (File.Exists(assemblyPath) && TryLoadAssembly(assemblyPath, out Assembly resolveAssembly))
            {
                return resolveAssembly;
            }

            foreach (string assemblyFile in GetAssemblyPaths(s_probingPath, assemblyName).Where(File.Exists))
            {
                if (TryLoadAssembly(assemblyFile, out resolveAssembly))
                {
                    return resolveAssembly;
                }
            }
        }
        finally
        {
            s_assemblyResolutionInProgress.Remove(assemblyName.Name);
        }

        return null;

        bool TryLoadAssembly(string assemblyPath, out Assembly resolveAssembly)
        {
            try
            {
                resolveAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
                s_loadedAssemblies[assemblyName.Name] = resolveAssembly;
                return true;
            }
            catch (Exception)
            {
            }

            resolveAssembly = default;
            return false;
        }

        static IEnumerable<string> GetAssemblyPaths(string probingPath, AssemblyName assemblyName)
        {
            yield return Path.Combine(probingPath, $"{assemblyName.Name}.dll");
            yield return Path.Combine(probingPath, $"{assemblyName.Name}.exe");
        }
    }

    private static string GetProbingPath()
    {
        string sdkFolder = Environment.GetEnvironmentVariable("GSC_SDK_CORE");

        if (Directory.Exists(sdkFolder))
        {
            return sdkFolder;
        }

        // Get installation folders ordered by version
        var orderedFolders = GetInstallationFolders()
            .OrderByDescending(tuple => tuple.Version)
            .ToList();

        // Try to find folders in order of preference
        foreach (var (_, folder) in orderedFolders)
        {
            // Check for net8.0-windows first
            string net8Path = Path.Combine(folder, "net8.0-windows");
            if (Directory.Exists(net8Path))
            {
                return net8Path;
            }

            // Then check for net6.0-windows
            string net6Path = Path.Combine(folder, "net6.0-windows");
            if (Directory.Exists(net6Path))
            {
                return net6Path;
            }
        }

        // If neither is found, return null
        return null;
    }

    private static IEnumerable<(Version Version, string Folder)> GetInstallationFolders()
    {
        foreach (string root in new[] { @"SOFTWARE\Genetec\Security Center\", @"SOFTWARE\Wow6432Node\Genetec\Security Center\" })
        {
            using RegistryKey key = Registry.LocalMachine.OpenSubKey(root);
            if (key is null)
            {
                continue;
            }

            foreach (string name in key.GetSubKeyNames())
            {
                if (Version.TryParse(name, out Version version))
                {
                    using RegistryKey subKey = key.OpenSubKey(name);
                    if (subKey is null)
                    {
                        continue;
                    }

                    if (subKey.GetValue("Installation Path") is string path)
                    {
                        yield return (version, path);
                    }
                    else if (subKey.GetValue("InstallDir") is string dir)
                    {
                        yield return (version, dir);
                    }
                }
            }
        }
    }
}

#endif
