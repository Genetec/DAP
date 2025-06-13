// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Genetec.Sdk.Entities;
    using Sdk;
    using Sdk.Queries;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            const string server = "localhost";
            const string username = "admin";
            const string password = "";

            using var engine = new Engine();

            ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

            if (state == ConnectionStateCode.Success)
            {
                await PrefetchEntities(engine);

                List<Guid> doorGuids = engine.GetEntities(EntityType.Door).Select(door => door.Guid).ToList();

                DateTime currentTime = DateTime.UtcNow;

                List<(AccessPoint, Credential, AccessResult Result)> accessResults = engine.GetEntities(EntityType.Credential)
                    .SelectMany(credential => engine.AccessVerifier.GetAccessResults(doorGuids, currentTime, credential.Guid, Guid.Empty, Guid.Empty)
                    .Select(result => (engine.GetEntity<AccessPoint>(result.AccessPoint), engine.GetEntity<Credential>(credential.Guid), result.Result)))
                    .ToList();

                using var fileStream = new FileStream($"AccessMatrix_{currentTime.ToLocalTime():yyyyMMdd_HHmmss}.csv", FileMode.Create, FileAccess.Write);
                using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);

                GenerateAccessMatrixCsv(engine, accessResults, streamWriter);

                Console.WriteLine($"File created: {fileStream.Name}");
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static async Task PrefetchEntities(IEngine engine)
        {
            var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
            query.EntityTypeFilter.Add(EntityType.Credential);
            query.EntityTypeFilter.Add(EntityType.AccessPoint);
            query.EntityTypeFilter.Add(EntityType.Schedule);
            query.EntityTypeFilter.Add(EntityType.AccessRule);
            query.DownloadAllRelatedData = true;
            query.Page = 1;
            query.PageSize = 1000;

            QueryCompletedEventArgs args;
            do
            {
                args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
                query.Page++;
            } while (args.Error == ReportError.TooManyResults || args.Data.Rows.Count > query.PageSize);
        }

        static void GenerateAccessMatrixCsv(IEngine engine, ICollection<(AccessPoint AccessPoint, Credential Credential, AccessResult Result)> accessResults, TextWriter textWriter)
        {
            var accessPoints = accessResults.Select(tuple => tuple.AccessPoint).Distinct().ToList();

            var resultLookup = accessResults.ToDictionary(tuple => (tuple.AccessPoint, tuple.Credential), tuple => tuple.Result);

            textWriter.Write("Credential");
            foreach (var accessPoint in accessPoints)
            {
                textWriter.Write($",{engine.GetEntity(accessPoint.AccessPointGroup).Name} ({accessPoint.Name})");
            }
            textWriter.WriteLine();

            foreach (var credential in accessResults.Select(tuple => tuple.Credential).Distinct())
            {
                textWriter.Write(credential.Name);

                foreach (var accessPoint in accessPoints)
                {
                    textWriter.Write(resultLookup.TryGetValue((accessPoint, credential), out var result) ? $",{result}" : ",");
                }

                textWriter.WriteLine();
            }
        }
    }
}
