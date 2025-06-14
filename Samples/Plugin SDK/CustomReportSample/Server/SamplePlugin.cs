// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples.Server;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Genetec.Dap.CodeSamples.Server.ReportHandlers.Custom;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.EventsArgs;
using Genetec.Sdk.Plugin;
using ReportHandlers;
using ReportHandlers.AccessControl;
using ReportHandlers.Video;

// Server-side plugin for the custom report sample.
[PluginProperty(typeof(SamplePluginDescriptor))]
public class SamplePlugin : Plugin
{
    // Holds the queries that are currently being processed.
    private readonly ConcurrentDictionary<(Guid QueryId, int MessageId), CancellationTokenSource> m_queries = new();

    // Holds the report handlers for each supported query type.
    private readonly Dictionary<ReportQueryType, IReportHandler> m_reportHandlers = new();

    // Returns the supported queries for this plugin.
    public sealed override List<ReportQueryType> SupportedQueries => new()
    {
        ReportQueryType.ActivityTrails,
        ReportQueryType.AuditTrails,
        ReportQueryType.CardholderActivity,
        ReportQueryType.CredentialActivity,
        ReportQueryType.DoorActivity,
        ReportQueryType.AreaActivity,
        ReportQueryType.ElevatorActivity,
        ReportQueryType.UnitActivity,
        ReportQueryType.ZoneActivity,
        ReportQueryType.IntrusionAreaActivity,
        ReportQueryType.IntrusionUnitActivity,
        ReportQueryType.CameraEvent,
        ReportQueryType.VideoMotionEvent,
        ReportQueryType.HealthEvent,
        ReportQueryType.HealthStatistics,
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

        m_reportHandlers.Add(ReportQueryType.AuditTrails, new AuditTrailsReportHandler(Engine, role));
        m_reportHandlers.Add(ReportQueryType.ActivityTrails, new ActivityTrailsReportHandler(Engine, role));

        var accessControlReportHandler = new AccessControlReportHandler(Engine, role);
        m_reportHandlers.Add(ReportQueryType.CardholderActivity, accessControlReportHandler);
        m_reportHandlers.Add(ReportQueryType.CredentialActivity, accessControlReportHandler);
        m_reportHandlers.Add(ReportQueryType.DoorActivity, accessControlReportHandler);
        m_reportHandlers.Add(ReportQueryType.AreaActivity, accessControlReportHandler);
        m_reportHandlers.Add(ReportQueryType.ElevatorActivity, accessControlReportHandler);
        m_reportHandlers.Add(ReportQueryType.UnitActivity, accessControlReportHandler);

        var zoneActivityReportHandler = new ZoneActivityReportHandler(Engine, role);
        m_reportHandlers.Add(ReportQueryType.ZoneActivity, zoneActivityReportHandler);

        var intrusionDetectionReportHandler = new IntrusionDetectionReportHandler(Engine, role);
        m_reportHandlers.Add(ReportQueryType.IntrusionAreaActivity, intrusionDetectionReportHandler);
        m_reportHandlers.Add(ReportQueryType.IntrusionUnitActivity, intrusionDetectionReportHandler);

        var videoEventReportHandler = new VideoEventReportHandler(Engine, role);
        m_reportHandlers.Add(ReportQueryType.CameraEvent, videoEventReportHandler);
        m_reportHandlers.Add(ReportQueryType.VideoMotionEvent, videoEventReportHandler);

        m_reportHandlers.Add(ReportQueryType.HealthEvent, new HealthEventsReportHandler(Engine, role));
        m_reportHandlers.Add(ReportQueryType.HealthStatistics, new HealthStatisticsReportHandler(Engine, role));

        m_reportHandlers.Add(ReportQueryType.Custom, new CustomReportHandler(Engine, role));

        ModifyPluginState(new PluginStateEntry("PluginState", "Plugin started"));
    }

    protected override void OnPluginStart()
    {
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
        if (!m_reportHandlers.TryGetValue(args.Query.ReportQueryType, out IReportHandler handler))
        {
            // If the query type is not supported, send a query completed event with a successful status.
            SendQueryCompleted(true);
            return;
        }
      
        (Guid QueryId, int MessageId) key = (args.Query.QueryId, args.MessageId);
        using var tokenSource = new CancellationTokenSource();
      
        if (m_queries.TryAdd(key, tokenSource))
        {
            try
            {
                await handler.HandleAsync(args, tokenSource.Token);
                SendQueryCompleted(true);
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