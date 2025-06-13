// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples
{
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
                var credentialsToDeactivate = await FetchUnusedCredentials(TimeSpan.FromDays(30));
                
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
                    foreach (var credential in credentials.Where(credential => credential.Status.State == CredentialState.Active))
                    {
                        Console.WriteLine($"Deactivating {credential.Name}");
                        credential.Status.Deactivate();
                    }
                });
            }

            async Task PrefetchEntities(IEnumerable<Guid> entityGuids)
            {
                foreach (var batch in entityGuids.Split(2000))
                {
                    var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
                    query.EntityGuids.AddRange(batch);
                    query.DownloadAllRelatedData = true;

                    await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
                }
            }
        }
    }
}