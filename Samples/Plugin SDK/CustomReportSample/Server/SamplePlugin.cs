// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server;

using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.EventsArgs;
using Genetec.Sdk.Plugin;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

// Server-side plugin for the custom report sample.
[PluginProperty(typeof(SamplePluginDescriptor))]
public class SamplePlugin : Plugin
{
    // Holds the queries that are currently being processed.
    private readonly ConcurrentDictionary<(Guid QueryId, int MessageId), CancellationTokenSource> m_queries = new();

    private CustomReportHandler m_reportHandler;

    // Returns the supported queries for this plugin.
    public sealed override List<ReportQueryType> SupportedQueries => new()
    {
        ReportQueryType.Custom
    };

    // Returns the supported custom reports for this plugin.
    public override List<Guid> SupportedCustomReports => new() { CustomReportId.Value };

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
        }
    }

    protected override void OnPluginLoaded()
    {
        var role = (Role)Engine.GetEntity(PluginGuid);
        m_reportHandler = new CustomReportHandler(Engine, role);

        ModifyPluginState(new PluginStateEntry("PluginState", "Plugin started"));
    }

    // Cancels the query if it is currently being processed.
    protected override void OnQueryCancelled(ReportQueryCancelledEventArgs args)
    {
        if (args.SystemsToCancel.Contains(PluginGuid) && m_queries.TryGetValue((args.QueryId, args.MessageId), out CancellationTokenSource cancellationTokenSource))
        {
            cancellationTokenSource.Cancel();
        }
    }

    // Handles the query received event.
    protected override async void OnQueryReceived(ReportQueryReceivedEventArgs args)
    {
        (Guid QueryId, int MessageId) key = (args.Query.QueryId, args.MessageId);
        using var tokenSource = new CancellationTokenSource();

        if (m_queries.TryAdd(key, tokenSource))
        {
            try
            {
                ReportError error = await m_reportHandler.HandleAsync(args, tokenSource.Token);

                if (error != ReportError.None)
                {
                    SendQueryCompleted(false, error, Severity.Warning, $"Query completed with error: {error}");
                }
                else
                {
                    SendQueryCompleted(true);
                }
            }
            catch (OperationCanceledException)
            {
                SendQueryCompleted(true);
            }
            catch (Exception ex)
            {
                SendQueryCompleted(false, ReportError.Unknown, Severity.Error, ex.Message);
            }
            finally
            {
                m_queries.TryRemove(key, out _);
            }
        }

        void SendQueryCompleted(bool successful, ReportError reportError = default, Severity severity = default, string errorMessage = null)
        {
            Engine.ReportManager.SendQueryCompleted(args.MessageId, args.QuerySource, PluginGuid, successful, reportError, severity, errorMessage);
        }
    }
}
