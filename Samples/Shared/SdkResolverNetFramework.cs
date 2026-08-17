// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

#if NETFRAMEWORK

namespace Genetec.Dap.CodeSamples;

using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

public static class SdkResolver
{
    private static readonly string s_probingPath = GetProbingPath();
    private static readonly ConcurrentDictionary<string, Assembly> s_loadedAssemblies = new();

    public static void Initialize()
    {
        if (string.IsNullOrEmpty(s_probingPath))
            throw new InvalidOperationException("SDK probing path could not be found.");

        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

        if (Directory.Exists(s_probingPath))
        {
            Environment.CurrentDirectory = s_probingPath;

            // Native SDK binaries (codecs, hardware decoders, ...) are found by the OS loader through its standard
            // search path, not through AssemblyResolve. SDK layouts that ship them per architecture place them in
            // the x64\ or x86\ subfolder instead of the SDK root, so make the subfolder matching the process
            // architecture visible to the OS loader as well. The SDK root stays the current directory for layouts
            // without architecture subfolders.
            string architectureFolder = GetArchitectureFolder(s_probingPath);
            if (Directory.Exists(architectureFolder))
            {
                string path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
                Environment.SetEnvironmentVariable("PATH", architectureFolder + Path.PathSeparator + path);
            }
        }
    }

    private static string GetArchitectureFolder(string probingPath) =>
        Path.Combine(probingPath, Environment.Is64BitProcess ? "x64" : "x86");

    private static string GetProbingPath()
    {
        string sdkFolder = Environment.GetEnvironmentVariable("GSC_SDK");

        return Directory.Exists(sdkFolder)
            ? sdkFolder
            : GetInstallationFolders().OrderBy(tuple => tuple.Version).Select(tuple => tuple.Folder).Where(Directory.Exists).LastOrDefault();
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

                    if (subKey.GetValue("Installation Path") is string path) //SDK installation PATH
                    {
                        yield return (version, path);
                    }
                    else if (subKey.GetValue("InstallDir") is string dir) //Security Center installation PATH
                    {
                        yield return (version, dir);
                    }
                }
            }
        }
    }

    private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        // args.Name is the full display name; check the suffix on the simple name
        if (new AssemblyName(args.Name).Name.EndsWith(".XmlSerializers", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (s_loadedAssemblies.TryGetValue(args.Name, out Assembly cachedAssembly))
        {
            return cachedAssembly;
        }

        foreach (var assemblyFile in GetAssemblyPaths(s_probingPath, args.Name).Where(File.Exists))
        {
            try
            {
                Assembly assembly = Assembly.LoadFile(assemblyFile);
                s_loadedAssemblies.TryAdd(args.Name, assembly);
                return assembly;
            }
            catch
            {
                // Continue to the next file if loading fails
            }
        }

        s_loadedAssemblies.TryAdd(args.Name, null);
        return null;
    }

    private static IEnumerable<string> GetAssemblyPaths(string probingPath, string assemblyName)
    {
        var parsedAssemblyName = new AssemblyName(assemblyName);

        if (parsedAssemblyName.CultureInfo != null && !string.IsNullOrEmpty(parsedAssemblyName.CultureInfo.Name))
        {
            yield return Path.Combine(probingPath, parsedAssemblyName.CultureInfo.Name, $"{parsedAssemblyName.Name}.dll");
        }

        // Prefer the copy that matches the process architecture (x64\ or x86\) over the SDK root. SDK layouts that
        // ship the mixed-mode interop assemblies (SRTP, codecs, decoders, ...) per architecture place them in those
        // subfolders, and a root copy is not guaranteed to match the process architecture. The SDK root remains
        // the fallback for layouts that have no architecture subfolders.
        string architectureFolder = GetArchitectureFolder(probingPath);
        yield return Path.Combine(architectureFolder, $"{parsedAssemblyName.Name}.dll");
        yield return Path.Combine(architectureFolder, $"{parsedAssemblyName.Name}.exe");

        yield return Path.Combine(probingPath, $"{parsedAssemblyName.Name}.dll");
        yield return Path.Combine(probingPath, $"{parsedAssemblyName.Name}.exe");
    }
}

#endif