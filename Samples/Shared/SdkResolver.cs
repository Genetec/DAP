// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;

public static class SdkResolver
{
    private static string s_probingPath;

    public static void Initialize()
    {
        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        s_probingPath = GetProbingPath();
    }

    private static string GetProbingPath()
    {
        string sdkFolder = Environment.GetEnvironmentVariable(RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework") ? "GSC_SDK" : "GSC_SDK_CORE");

        return Directory.Exists(sdkFolder)
            ? sdkFolder
            : GetInstallationFolders().OrderBy(tuple => tuple.Version).Select(tuple => tuple.Folder).Where(Directory.Exists).LastOrDefault();

        IEnumerable<(Version Version, string Folder)> GetInstallationFolders()
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
    }

    private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        foreach (string assemblyFile in GetAssemblyPaths(s_probingPath).Where(File.Exists))
        {
            try
            {
                return Assembly.LoadFile(assemblyFile);
            }
            catch
            {
            }
        }

        return null;

        IEnumerable<string> GetAssemblyPaths(string probingPath)
        {
            var assemblyName = new AssemblyName(args.Name);
            yield return Path.Combine(probingPath, $"{assemblyName.Name}.dll");
            yield return Path.Combine(probingPath, $"{assemblyName.Name}.exe");
        }
    }
}