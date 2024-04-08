namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Genetec.Sdk.Diagnostics.Logging.Core;

    public static class AssemblyResolver
    {
        private static readonly Logger Logger = Logger.CreateClassLogger(typeof(AssemblyResolver));

        private static readonly List<string> ProbingPaths = new List<string>();

        static AssemblyResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            var directory = new FileInfo(Assembly.GetCallingAssembly().Location).Directory;
            if (directory != null)
            {
                ProbingPaths.Add(directory.FullName);
            }
        }

        public static void Initialize()
        {
            // intentionally empty. This is just meant to ensure the static constructor has run.
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);

            return ProbingPaths.Select(location => Path.Combine(location, $"{assemblyName.Name}.dll"))
                .Where(File.Exists)
                .Select(LoadAssembly)
                .FirstOrDefault(assembly => assembly != null);

            static Assembly LoadAssembly(string file)
            {
                Logger.TraceInformation($"Loading assembly {file}");
                try
                {
                    return Assembly.LoadFrom(file);
                }
                catch (Exception ex)
                {
                    Logger.TraceError($"An error occurred while trying to load assembly {file}: {ex.Message}");
                    return null;
                }
            }
        }
    }
}