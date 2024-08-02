// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples.Server
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Genetec.Sdk.Diagnostics.Logging.Core;

    internal static class AssemblyResolver
    {
        private static readonly string[] s_probingPaths =
        {
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
        };

        private static readonly Logger s_logger = Logger.CreateClassLogger(typeof(AssemblyResolver));

        static AssemblyResolver() => AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        public static void Initialize()
        {
            // Intentionally empty. Its sole purpose is to ensure that the static constructor has run.
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return s_probingPaths.Select(location => Path.Combine(location, $"{args.Name}.dll"))
                                 .Where(File.Exists)
                                 .Select(LoadAssemblyFile)
                                 .FirstOrDefault(assembly => assembly != null);

            Assembly LoadAssemblyFile(string file)
            {
                s_logger.TraceInformation($"Loading assembly {file}");
                try
                {
                    return Assembly.LoadFile(file);
                }
                catch (Exception ex)
                {
                    s_logger.TraceError($"An error occurred while trying to load assembly {file}: {ex.Message}");
                    return null;
                }
            }
        }
    }
}
