// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

#if NETCOREAPP

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
    private static string s_probingPath;
    private static AssemblyDependencyResolver s_dependencyResolver;
    private static readonly ConcurrentDictionary<string, Lazy<Assembly>> s_loaders = new(StringComparer.OrdinalIgnoreCase);

    public static void Initialize()
    {
        s_probingPath = GetProbingPath();

        if (string.IsNullOrEmpty(s_probingPath))
            throw new InvalidOperationException("SDK probing path could not be found.");

        var sdkDll = Path.Combine(s_probingPath, "Genetec.Sdk.dll");
        s_dependencyResolver = File.Exists(sdkDll) ? new AssemblyDependencyResolver(sdkDll) : null;

        AssemblyLoadContext.Default.Resolving += OnAssemblyResolve;
    }

    private static Assembly OnAssemblyResolve(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        string key = assemblyName.FullName;
        Lazy<Assembly> lazy = s_loaders.GetOrAdd(key, _ => new Lazy<Assembly>(() => LoadAssembly(context, assemblyName)));

        Assembly assembly = lazy.Value;

        if (assembly is null)
            s_loaders.TryRemove(key, out _);

        return assembly;
    }

    private static Assembly LoadAssembly(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        // 1) Try AssemblyDependencyResolver
        var path = s_dependencyResolver.ResolveAssemblyToPath(assemblyName);
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
            return context.LoadFromAssemblyPath(path);

        // 2) Try direct probing folder
        if (!string.IsNullOrEmpty(s_probingPath))
        {
            foreach (var candidate in GetAssemblyPaths(s_probingPath, assemblyName).Where(File.Exists))
                return context.LoadFromAssemblyPath(candidate);
        }

        // Let the runtime continue its normal chain
        return null;
    }

    private static IEnumerable<string> GetAssemblyPaths(string probingPath, AssemblyName assemblyName)
    {
        yield return Path.Combine(probingPath, $"{assemblyName.Name}.dll");
        yield return Path.Combine(probingPath, $"{assemblyName.Name}.exe");
    }

    private static string GetProbingPath()
    {
        var sdkFolder = Environment.GetEnvironmentVariable("GSC_SDK_CORE");
        if (!string.IsNullOrEmpty(sdkFolder) && Directory.Exists(sdkFolder))
            return sdkFolder;

        foreach (var (_, folder) in GetInstallationFolders().OrderByDescending(t => t.Version))
        {
            var net8 = Path.Combine(folder, "net8.0-windows");
            if (Directory.Exists(net8)) return net8;

            var net6 = Path.Combine(folder, "net6.0-windows");
            if (Directory.Exists(net6)) return net6;
        }

        return null;
    }

    private static IEnumerable<(Version Version, string Folder)> GetInstallationFolders()
    {
        foreach (var root in new[] { @"SOFTWARE\Genetec\Security Center\", @"SOFTWARE\Wow6432Node\Genetec\Security Center\" })
        {
            using var key = Registry.LocalMachine.OpenSubKey(root);
            if (key is null) continue;

            foreach (var name in key.GetSubKeyNames())
            {
                if (!Version.TryParse(name, out var version)) continue;
                using var sub = key.OpenSubKey(name);
                if (sub is null) continue;

                if (sub.GetValue("Installation Path") is string path) yield return (version, path);
                else if (sub.GetValue("InstallDir") is string dir) yield return (version, dir);
            }
        }
    }
}

#endif