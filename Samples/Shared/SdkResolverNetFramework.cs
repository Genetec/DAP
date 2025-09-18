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
        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
    }

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
        if (args.Name.EndsWith(".resources") || args.Name.EndsWith(".xmlserializers"))
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
        yield return Path.Combine(probingPath, $"{parsedAssemblyName.Name}.dll");
        yield return Path.Combine(probingPath, $"{parsedAssemblyName.Name}.exe");

        if (Environment.Is64BitProcess)
        {
            yield return Path.Combine(probingPath, "x64", $"{parsedAssemblyName.Name}.dll");
            yield return Path.Combine(probingPath, "x64", $"{parsedAssemblyName.Name}.exe");
        }
        else
        {
            yield return Path.Combine(probingPath, "x86", $"{parsedAssemblyName.Name}.dll");
            yield return Path.Combine(probingPath, "x86", $"{parsedAssemblyName.Name}.exe");
        }
    }
}

#endif