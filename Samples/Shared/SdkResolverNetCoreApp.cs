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
using System.Text.Json;
using Microsoft.Win32;
using System.Collections.Concurrent;

public static class SdkResolver
{
    private static string s_probingPath;
    private static Dictionary<string, string> s_packageAssemblyPaths;
    private static readonly ConcurrentDictionary<string, Lazy<Assembly>> s_loaders = new(StringComparer.OrdinalIgnoreCase);

    public static void Initialize()
    {
        s_probingPath = GetProbingPath();

        if (string.IsNullOrEmpty(s_probingPath))
            throw new InvalidOperationException("SDK probing path could not be found.");

        s_packageAssemblyPaths = BuildPackageAssemblyIndex(s_probingPath);

        AssemblyLoadContext.Default.Resolving += OnAssemblyResolve;
    }

    private static Dictionary<string, string> BuildPackageAssemblyIndex(string probingPath)
    {
        var index = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var nugetCache = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
        if (string.IsNullOrWhiteSpace(nugetCache))
            nugetCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");

        bool hasNugetCache = Directory.Exists(nugetCache);

        foreach (var depsFile in Directory.EnumerateFiles(probingPath, "*.deps.json"))
        {
            try
            {
                using var stream = File.OpenRead(depsFile);
                using var doc = JsonDocument.Parse(stream);

                if (!doc.RootElement.TryGetProperty("targets", out var targets))
                    continue;

                foreach (var target in targets.EnumerateObject())
                {
                    foreach (var package in target.Value.EnumerateObject())
                    {
                        if (!package.Value.TryGetProperty("runtime", out var runtime))
                            continue;

                        foreach (var runtimeEntry in runtime.EnumerateObject())
                        {
                            var runtimeName = runtimeEntry.Name;

                            if (Path.IsPathRooted(runtimeName))
                                continue;

                            var dllName = Path.GetFileNameWithoutExtension(Path.GetFileName(runtimeName));
                            if (index.ContainsKey(dllName))
                                continue;

                            // Check if DLL is already in the SDK folder
                            var localPath = Path.Combine(probingPath, Path.GetFileName(runtimeName));
                            if (File.Exists(localPath))
                            {
                                index[dllName] = localPath;
                                continue;
                            }

                            // Try to find in NuGet cache
                            if (hasNugetCache)
                            {
                                var packageParts = package.Name.Split('/');
                                if (packageParts.Length == 2)
                                {
                                    var packagePath = Path.Combine(nugetCache, packageParts[0].ToLowerInvariant(), packageParts[1].ToLowerInvariant(), runtimeName);
                                    if (File.Exists(packagePath))
                                    {
                                        index[dllName] = packagePath;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // Skip malformed deps.json files
            }
            catch (IOException)
            {
                // Skip files that cannot be read
            }
            catch (UnauthorizedAccessException)
            {
                // Skip files that cannot be read
            }
        }

        return index;
    }

    private static Assembly OnAssemblyResolve(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        string key = assemblyName.FullName ?? assemblyName.Name;

        if (assemblyName.Name.EndsWith(".XmlSerializers", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        Lazy<Assembly> lazy = s_loaders.GetOrAdd(key, _ => new Lazy<Assembly>(() => LoadAssembly(context, assemblyName)));

        Assembly assembly = lazy.Value;

        if (assembly is null)
            s_loaders.TryRemove(key, out _);

        return assembly;
    }

    private static Assembly LoadAssembly(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        if (s_packageAssemblyPaths == null)
            return null;

        // 1) Try the package assembly index (built from all .deps.json files)
        if (s_packageAssemblyPaths.TryGetValue(assemblyName.Name, out var resolvedPath) && File.Exists(resolvedPath))
        {
            try
            {
                return context.LoadFromAssemblyPath(resolvedPath);
            }
            catch (BadImageFormatException)
            {
                // Fall through to next resolution strategy
            }
        }

        // 2) Try direct probing folder
        if (!string.IsNullOrEmpty(s_probingPath))
        {
            foreach (var candidate in GetAssemblyPaths(s_probingPath, assemblyName).Where(File.Exists))
            {
                try
                {
                    return context.LoadFromAssemblyPath(candidate);
                }
                catch (BadImageFormatException)
                {
                    // Ignore and try next candidate
                }
            }
        }

        // Let the runtime continue its normal chain
        return null;
    }

    private static IEnumerable<string> GetAssemblyPaths(string probingPath, AssemblyName assemblyName)
    {
        if (assemblyName.CultureInfo != null && !string.IsNullOrEmpty(assemblyName.CultureInfo.Name))
        {
            yield return Path.Combine(probingPath, assemblyName.CultureInfo.Name, $"{assemblyName.Name}.dll");
        }

        yield return Path.Combine(probingPath, $"{assemblyName.Name}.dll");
        yield return Path.Combine(probingPath, $"{assemblyName.Name}.exe");

        if (Environment.Is64BitProcess)
        {
            yield return Path.Combine(probingPath, "x64", $"{assemblyName.Name}.dll");
            yield return Path.Combine(probingPath, "x64", $"{assemblyName.Name}.exe");
        }
        else
        {
            yield return Path.Combine(probingPath, "x86", $"{assemblyName.Name}.dll");
            yield return Path.Combine(probingPath, "x86", $"{assemblyName.Name}.exe");
        }
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
