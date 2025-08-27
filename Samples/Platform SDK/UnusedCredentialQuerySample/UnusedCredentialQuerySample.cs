// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries;
using Genetec.Sdk.Queries.AccessControl;

namespace Genetec.Dap.CodeSamples;

public class UnusedCredentialQuerySample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        var credentialsToDeactivate = await FetchUnusedCredentials(engine, TimeSpan.FromDays(30));
        
        Console.WriteLine($"{credentialsToDeactivate.Count} credentials have not been used in the last 30 days.");

        await DeactivateCredentials(engine, credentialsToDeactivate);
    }

    async Task<List<Credential>> FetchUnusedCredentials(Engine engine, TimeSpan unusedDuration)
    {
        var query = (UnusedCredentialQuery)engine.ReportManager.CreateReportQuery(ReportType.UnusedCredentials);
        query.TimeRange.SetTimeRange(-unusedDuration);

        QueryCompletedEventArgs queryResults = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

        List<Guid> credentialGuids = queryResults.Data.AsEnumerable()
                                                      .Select(row => row.Field<Guid>(UnusedEntityQuery.UnusedEntityGuidColumnName))
                                                      .ToList();

        await PrefetchEntities(engine, credentialGuids);

        return credentialGuids.Select(engine.GetEntity).OfType<Credential>().ToList();
    }

    async Task DeactivateCredentials(Engine engine, IEnumerable<Credential> credentials)
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

    async Task PrefetchEntities(Engine engine, IEnumerable<Guid> entityGuids)
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