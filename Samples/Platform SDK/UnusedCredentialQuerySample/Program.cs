// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries.AccessControl;
using Sdk.Queries;

class Program
{
    static Program() => SdkResolver.Initialize();

    static async Task Main()
    {
        const string serverAddress = "localhost";
        const string adminUsername = "admin";
        const string adminPassword = "";

        using var engine = new Engine();

        ConnectionStateCode status = await engine.LogOnAsync(serverAddress, adminUsername, adminPassword);

        if (status == ConnectionStateCode.Success)
        {
            List<Credential> credentialsToDeactivate = await FetchUnusedCredentials(TimeSpan.FromDays(30));
                
            Console.WriteLine($"{credentialsToDeactivate.Count} credentials have not been used in the last 30 days.");

            await DeactivateCredentials(credentialsToDeactivate);
        }
        else
        {
            Console.WriteLine($"Logon failed: {status}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();

        async Task<List<Credential>> FetchUnusedCredentials(TimeSpan unusedDuration)
        {
            var query = (UnusedCredentialQuery)engine.ReportManager.CreateReportQuery(ReportType.UnusedCredentials);
            query.TimeRange.SetTimeRange(-unusedDuration);

            QueryCompletedEventArgs queryResults = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

            List<Guid> credentialGuids = queryResults.Data.AsEnumerable()
                .Select(row => row.Field<Guid>(UnusedEntityQuery.UnusedEntityGuidColumnName))
                .ToList();

            await PrefetchEntities(credentialGuids);

            return credentialGuids.Select(engine.GetEntity).OfType<Credential>().ToList();
        }

        async Task DeactivateCredentials(IEnumerable<Credential> credentials)
        {
            await engine.TransactionManager.ExecuteTransactionAsync(() =>
            {
                foreach (Credential credential in credentials.Where(credential => credential.Status.State == CredentialState.Active))
                {
                    Console.WriteLine($"Deactivating {credential.Name}");
                    credential.Status.Deactivate();
                }
            });
        }

        async Task PrefetchEntities(IEnumerable<Guid> entityGuids)
        {
            foreach (List<Guid> batch in entityGuids.Split(2000))
            {
                var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
                query.EntityGuids.AddRange(batch);
                query.DownloadAllRelatedData = true;

                await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
            }
        }
    }
}