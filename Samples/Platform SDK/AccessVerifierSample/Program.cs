﻿// Copyright 2024 Genetec Inc.
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
        List<AccessPoint> accessPoints = accessResults.Select(tuple => tuple.AccessPoint).Distinct().ToList();

        Dictionary<(AccessPoint AccessPoint, Credential Credential), AccessResult> resultLookup = accessResults.ToDictionary(tuple => (tuple.AccessPoint, tuple.Credential), tuple => tuple.Result);

        textWriter.Write("Credential");
        foreach (AccessPoint accessPoint in accessPoints)
        {
            textWriter.Write($",{engine.GetEntity(accessPoint.AccessPointGroup).Name} ({accessPoint.Name})");
        }
        textWriter.WriteLine();

        foreach (Credential credential in accessResults.Select(tuple => tuple.Credential).Distinct())
        {
            textWriter.Write(credential.Name);

            foreach (AccessPoint accessPoint in accessPoints)
            {
                textWriter.Write(resultLookup.TryGetValue((accessPoint, credential), out AccessResult result) ? $",{result}" : ",");
            }

            textWriter.WriteLine();
        }
    }
}