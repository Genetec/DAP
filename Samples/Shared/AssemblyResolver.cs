// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using System.IO;
using System.Reflection;
using Sdk.Diagnostics.Logging.Core;

public static class AssemblyResolver
{
    private static readonly string s_location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    private static readonly Logger s_logger = Logger.CreateClassLogger(typeof(AssemblyResolver));

    static AssemblyResolver() => AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

    public static void Initialize()
    {
        // intentionally empty. This is just meant to ensure the static constructor has run.
    }

    private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        var assemblyName = new AssemblyName(args.Name);

        if (assemblyName.Name.EndsWith(".XmlSerializers", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        string assemblyPath = Path.Combine(s_location, $"{assemblyName.Name}.dll");

        if (!File.Exists(assemblyPath))
        {
            return null;
        }

        s_logger.TraceInformation($"Loading assembly {assemblyPath}");

        try
        {
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            s_logger.TraceInformation($"Assembly loaded {assembly.ManifestModule.FullyQualifiedName}");
            return assembly;
        }
        catch (Exception ex)
        {
            s_logger.TraceError($"An error occurred while trying to load assembly {assemblyPath}: {ex.Message}");
            return null;
        }
    }
}