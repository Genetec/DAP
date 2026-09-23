// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples.Server.ReportHandlers;
using Genetec.Dap.CodeSamples.Server.ReportHandlers.AccessControl;
using Genetec.Dap.CodeSamples.Server.ReportHandlers.Intrusion;
using Genetec.Dap.CodeSamples.Server.ReportHandlers.Video;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.EventsArgs;
using Genetec.Sdk.Plugin;
using Genetec.Sdk.Plugin.Interfaces;

/// <summary>
/// A plugin that receives report records over local HTTP and answers Security Center's native
/// reports from them. It exposes one ingestion endpoint per supported report domain.
/// </summary>
[PluginProperty(typeof(SamplePluginDescriptor))]
public class SamplePlugin : Plugin, IPluginDatabaseSupport
{
    // The static constructor initializes the AssemblyResolver to ensure that
    // the plugin can dynamically resolve and load required assemblies at runtime.
    static SamplePlugin() => AssemblyResolver.Initialize();

    // The plugin database stores the ingested events.
    private readonly SampleDatabaseManager m_databaseManager = new();

    // Holds the queries that are currently being processed.
    private readonly ConcurrentDictionary<(Guid QueryId, int MessageId), CancellationTokenSource> m_queries = new();

    // Holds the report handlers for each supported query type.
    private readonly Dictionary<ReportQueryType, IReportHandler> m_reportHandlers = new();

    private HttpIngestionServer m_server;
    private RoleConfiguration m_configuration = new();
    private Role m_role;

    public DatabaseManager DatabaseManager => m_databaseManager;

    public override List<Guid> SupportedCustomReports => new() { CustomEventReport.Id };

    // The queries this plugin answers: the whole access-control activity family, zone, intrusion,
    // video, health, and the two trail reports.
    public sealed override List<ReportQueryType> SupportedQueries => new()
    {
        ReportQueryType.Custom,
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
        ReportQueryType.HealthStatistics
    };

    protected override void OnPluginLoaded()
    {
        m_role = (Role)Engine.GetEntity(PluginGuid);
        m_configuration = RoleConfiguration.Deserialize(m_role.SpecificConfiguration);

        RegisterReportHandlers(m_role);

        // Construct the ingestor and server before subscribing to configuration changes, because
        // the change handler reads m_server.
        var ingestor = new EventIngestor(Engine, m_databaseManager, Logger);
        m_server = new HttpIngestionServer(ingestor.IngestAsync);
        m_role.FieldsChanged += OnRoleFieldsChanged;
    }

    protected override void OnPluginStart()
    {
        StartListening(m_configuration);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Unsubscribe before disposing the server, so a configuration change cannot reach a
            // disposed server.
            if (m_role is not null)
            {
                m_role.FieldsChanged -= OnRoleFieldsChanged;
            }

            m_server?.Dispose();

            foreach (CancellationTokenSource query in m_queries.Values)
            {
                query.Cancel();
            }

            m_reportHandlers.Clear();
        }
    }

    // Registers one report handler per supported query type. A single AccessControlReportHandler
    // answers the whole access-control activity family, and one IntrusionDetectionReportHandler
    // and VideoEventReportHandler each answer their two report types.
    private void RegisterReportHandlers(Role role)
    {
        m_reportHandlers.Add(ReportQueryType.Custom, new CustomEventsReportHandler(Engine, role, m_databaseManager));
        m_reportHandlers.Add(ReportQueryType.ActivityTrails, new ActivityTrailsReportHandler(Engine, role, m_databaseManager));
        m_reportHandlers.Add(ReportQueryType.AuditTrails, new AuditTrailsReportHandler(Engine, role, m_databaseManager));

        var accessControl = new AccessControlReportHandler(Engine, role, m_databaseManager);
        m_reportHandlers.Add(ReportQueryType.CardholderActivity, accessControl);
        m_reportHandlers.Add(ReportQueryType.CredentialActivity, accessControl);
        m_reportHandlers.Add(ReportQueryType.DoorActivity, accessControl);
        m_reportHandlers.Add(ReportQueryType.AreaActivity, accessControl);
        m_reportHandlers.Add(ReportQueryType.ElevatorActivity, accessControl);
        m_reportHandlers.Add(ReportQueryType.UnitActivity, accessControl);

        m_reportHandlers.Add(ReportQueryType.ZoneActivity, new ZoneActivityReportHandler(Engine, role, m_databaseManager));

        var intrusion = new IntrusionDetectionReportHandler(Engine, role, m_databaseManager);
        m_reportHandlers.Add(ReportQueryType.IntrusionAreaActivity, intrusion);
        m_reportHandlers.Add(ReportQueryType.IntrusionUnitActivity, intrusion);

        var video = new VideoEventReportHandler(Engine, role, m_databaseManager);
        m_reportHandlers.Add(ReportQueryType.CameraEvent, video);
        m_reportHandlers.Add(ReportQueryType.VideoMotionEvent, video);

        m_reportHandlers.Add(ReportQueryType.HealthEvent, new HealthEventsReportHandler(Engine, role, m_databaseManager));
        m_reportHandlers.Add(ReportQueryType.HealthStatistics, new HealthStatisticsReportHandler(Engine, role, m_databaseManager));
    }

    // Starts the HTTP server on the specified configuration's port and reports the outcome as
    // the role's state. The configuration is committed to m_configuration only on success, so a
    // failed bind leaves the last good port in place and a later save of that port is seen as a
    // change worth retrying.
    private void StartListening(RoleConfiguration configuration)
    {
        int port = configuration.Port;
        try
        {
            m_server.Start(port);
            m_configuration = configuration;
            ModifyPluginState(new PluginStateEntry("Listener", $"Listening for local events at http://127.0.0.1:{port}"));
        }
        catch (HttpListenerException exception)
        {
            // The role turns red: an ingestion plugin that cannot listen cannot do its job.
            // The usual cause is another process already bound to the port.
            ModifyPluginState(new PluginStateEntry("Listener", $"Cannot listen on port {port}: {exception.Message}") { IsError = true });
        }
    }

    // Applies configuration changes saved from Config Tool. The role entity raises FieldsChanged
    // when its SpecificConfiguration is written. The listener is restarted when the port changed
    // or a previous start left it not listening.
    private void OnRoleFieldsChanged(object sender, FieldsChangedEventArgs e)
    {
        RoleConfiguration configuration = RoleConfiguration.Deserialize(m_role.SpecificConfiguration);

        if (configuration.Port != m_configuration.Port || !m_server.IsListening)
        {
            StartListening(configuration); // Start stops the previous listener before binding the new port
        }
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
                ReportError error = await handler.HandleAsync(args, tokenSource.Token);

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
                Logger.TraceError(ex, $"Failed to answer {args.Query.ReportQueryType} query {args.Query.QueryId}");
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
