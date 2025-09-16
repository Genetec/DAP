// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

#if NETFRAMEWORK

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;

public static class SdkResolver
{
    private static readonly string s_probingPath = GetProbingPath();

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
        foreach (var assemblyFile in GetAssemblyPaths(s_probingPath, args.Name).Where(File.Exists))
        {
            try
            {
                return Assembly.LoadFile(assemblyFile);
            }
            catch
            {
                // Continue to the next file if loading fails
            }
        }

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