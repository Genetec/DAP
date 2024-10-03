// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

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

public class SdkResolver : AssemblyLoadContext
{
    private static readonly string s_probingPath = GetProbingPath();
    private static readonly SdkResolver s_instance = new();
    private static readonly ConcurrentDictionary<string, Assembly> s_loadedAssemblies = new();
    private static readonly HashSet<string> s_assemblyResolutionInProgress = new();

    public static void Initialize()
    {
        Default.Resolving += OnAssemblyResolve;
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        return ResolveAssembly(assemblyName);
    }

    private static Assembly OnAssemblyResolve(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        return s_instance.ResolveAssembly(assemblyName);
    }

    private Assembly ResolveAssembly(AssemblyName assemblyName)
    {
        if (s_loadedAssemblies.TryGetValue(assemblyName.Name, out Assembly loadedAssembly))
        {
            return loadedAssembly;
        }

        if (!s_assemblyResolutionInProgress.Add(assemblyName.Name))
        {
            return null; // Prevent recursive resolution
        }

        try
        {
            foreach (var assemblyFile in GetAssemblyPaths(s_probingPath, assemblyName).Where(File.Exists))
            {
                try
                {
                    Assembly assembly = LoadFromAssemblyPath(assemblyFile);
                    s_loadedAssemblies[assemblyName.Name] = assembly;
                    return assembly;
                }
                catch (BadImageFormatException)
                {
                    // Skip assemblies that are not compatible
                }
                catch (Exception)
                {
                    // Log the exception if needed
                }
            }
        }
        finally
        {
            s_assemblyResolutionInProgress.Remove(assemblyName.Name);
        }

        return null;
    }

    private static string GetProbingPath()
    {
        string sdkFolder = Environment.GetEnvironmentVariable("GSC_SDK_CORE");

        return Directory.Exists(sdkFolder)
            ? sdkFolder
            : GetInstallationFolders()
                .OrderBy(tuple => tuple.Version)
                .Select(tuple => Path.Combine(tuple.Folder, "net8.0-windows"))
                .Where(Directory.Exists)
                .LastOrDefault();
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

    private static IEnumerable<string> GetAssemblyPaths(string probingPath, AssemblyName assemblyName)
    {
        yield return Path.Combine(probingPath, $"{assemblyName.Name}.dll");
        yield return Path.Combine(probingPath, $"{assemblyName.Name}.exe");
    }
}

#endif