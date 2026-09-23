// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server;

using Genetec.Dap.CodeSamples.Ingestion;
using Genetec.Sdk;
using Genetec.Sdk.Diagnostics.Logging.Core;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Plugin.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
// Genetec.Sdk.Entities also defines a Stream entity, so disambiguate the request body stream.
using Stream = System.IO.Stream;

/// <summary>
/// Turns a POST to a domain endpoint into a stored event. It owns the routing table (one path per
/// report domain), the per-domain validation, and the database write.
/// </summary>
public sealed class EventIngestor
{
    private readonly IEngine m_engine;
    private readonly SampleDatabaseManager m_databaseManager;
    private readonly Logger m_logger;
    private readonly Dictionary<string, Func<Stream, CancellationToken, Task<(HttpStatusCode, string)>>> m_routes;

    public EventIngestor(IEngine engine, SampleDatabaseManager databaseManager, Logger logger)
    {
        m_engine = engine;
        m_databaseManager = databaseManager;
        m_logger = logger;

        m_routes = new Dictionary<string, Func<Stream, CancellationToken, Task<(HttpStatusCode, string)>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["/access-control-events"] = IngestAccessControlEventAsync,
            ["/zone-activities"] = IngestZoneActivityAsync,
            ["/intrusion-events"] = IngestIntrusionEventAsync,
            ["/video-events"] = IngestVideoEventAsync,
            ["/health-events"] = IngestHealthEventAsync,
            ["/health-statistics"] = IngestHealthStatisticAsync,
            ["/activity-trails"] = IngestActivityTrailAsync,
            ["/audit-trails"] = IngestAuditTrailAsync,
            ["/custom-events"] = IngestCustomEventAsync
        };
    }

    /// <summary>The set of ingestion endpoints, for logging and diagnostics.</summary>
    public IReadOnlyCollection<string> Routes => m_routes.Keys.ToList();

    /// <summary>
    /// Dispatches a request to the matching domain endpoint. Returns 404 for an unknown path,
    /// 405 for a non-POST method, 503 when the database is unavailable, and otherwise the result
    /// of the domain handler.
    /// </summary>
    public async Task<(HttpStatusCode Status, string Error)> IngestAsync(string method, string path, Stream body, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!m_routes.TryGetValue(path.TrimEnd('/'), out Func<Stream, CancellationToken, Task<(HttpStatusCode, string)>> handler))
        {
            return (HttpStatusCode.NotFound, $"Unknown resource. POST events to one of: {string.Join(", ", m_routes.Keys)}");
        }

        if (method != "POST")
        {
            return (HttpStatusCode.MethodNotAllowed, "Only POST is supported.");
        }

        if (m_databaseManager.State != DatabaseState.Connected)
        {
            return (HttpStatusCode.ServiceUnavailable, "The plugin database is not available.");
        }

        return await handler(body, cancellationToken).ConfigureAwait(false);
    }

    private async Task<(HttpStatusCode, string)> IngestAccessControlEventAsync(Stream body, CancellationToken cancellationToken)
    {
        if (!IngestionParsing.TryDeserialize(body, out AccessControlEventIngestion e, out string error)) return Bad(error);
        if (!Require(e.Source, "source", out error)) return Bad(error);
        if (!IngestionParsing.TryResolveEventId(e.EventType, e.CustomEventId, IsCustomEventDefined, out int eventType, out error)) return Bad(error);
        if (!IngestionParsing.TryParseTimestamp(e.Timestamp, out DateTime timestamp, out error)) return Bad(error);

        await m_databaseManager.InsertAccessControlEventAsync(e, eventType, timestamp, cancellationToken).ConfigureAwait(false);
        return Accepted("access control event");
    }

    private async Task<(HttpStatusCode, string)> IngestCustomEventAsync(Stream body, CancellationToken cancellationToken)
    {
        if (!IngestionParsing.TryDeserialize(body, out CustomEventIngestion e, out string error)) return Bad(error);
        if (!Require(e.Source, "source", out error)) return Bad(error);
        if (e.CustomEventId is null || e.CustomEventId <= 0) return Bad("A positive customEventId is required.");
        var definition = CustomEventReport.GetDefinitions(m_engine).SingleOrDefault(item => item.Id == e.CustomEventId);
        if (definition is null) return Bad("The customEventId is not defined in Security Center.");
        var source = m_engine.GetEntity(e.Source.Value, true);
        if (source is null) return Bad("The source entity does not exist or is not accessible.");
        if (source.EntityType != definition.SourceEntityType)
            return Bad($"The custom event requires a source of type {definition.SourceEntityType}.");
        if (string.IsNullOrWhiteSpace(e.Timestamp)) return Bad("The timestamp field is required.");
        if (!IngestionParsing.TryParseTimestamp(e.Timestamp, out DateTime timestamp, out error)) return Bad(error);
        if (e.Message is null) return Bad("The message field is required (an empty string is allowed).");

        await m_databaseManager.InsertCustomEventAsync(e, timestamp, cancellationToken).ConfigureAwait(false);
        return Accepted("custom event");
    }

    private async Task<(HttpStatusCode, string)> IngestZoneActivityAsync(Stream body, CancellationToken cancellationToken)
    {
        if (!IngestionParsing.TryDeserialize(body, out ZoneActivityIngestion e, out string error)) return Bad(error);
        if (!Require(e.Zone, "zone", out error)) return Bad(error);
        if (!IngestionParsing.TryResolveEventId(e.EventType, e.CustomEventId, IsCustomEventDefined, out int eventType, out error)) return Bad(error);
        if (!IngestionParsing.TryParseTimestamp(e.Timestamp, out DateTime timestamp, out error)) return Bad(error);
        if (!IngestionParsing.TryParseLocalTimestamp(e.LocalTimestamp, timestamp, e.TimeZoneId, out DateTime localTimestamp, out error)) return Bad(error);

        await m_databaseManager.InsertZoneActivityAsync(e, eventType, timestamp, localTimestamp, cancellationToken).ConfigureAwait(false);
        return Accepted("zone activity");
    }

    private async Task<(HttpStatusCode, string)> IngestIntrusionEventAsync(Stream body, CancellationToken cancellationToken)
    {
        if (!IngestionParsing.TryDeserialize(body, out IntrusionEventIngestion e, out string error)) return Bad(error);
        if (!IsNonEmpty(e.IntrusionUnit) && !IsNonEmpty(e.IntrusionArea) && !IsNonEmpty(e.Source)) return Bad("Provide a nonempty intrusionUnit, intrusionArea, or source.");
        if (!IngestionParsing.TryResolveEventId(e.EventType, e.CustomEventId, IsCustomEventDefined, out int eventType, out error)) return Bad(error);
        if (!IngestionParsing.TryParseTimestamp(e.Timestamp, out DateTime timestamp, out error)) return Bad(error);

        await m_databaseManager.InsertIntrusionEventAsync(e, eventType, timestamp, cancellationToken).ConfigureAwait(false);
        return Accepted("intrusion event");
    }

    private async Task<(HttpStatusCode, string)> IngestVideoEventAsync(Stream body, CancellationToken cancellationToken)
    {
        if (!IngestionParsing.TryDeserialize(body, out VideoEventIngestion e, out string error)) return Bad(error);
        if (!Require(e.Camera, "camera", out error)) return Bad(error);
        // Video events are built-in only (the report's event-type column is unsigned), so no custom event id is resolved.
        if (!IngestionParsing.TryResolveEventId(e.EventType, null, IsCustomEventDefined, out int eventType, out error)) return Bad(error);
        if (!IngestionParsing.TryParseTimestamp(e.Timestamp, out DateTime eventTime, out error)) return Bad(error);
        if (!IngestionParsing.TryParseImage(e.Thumbnail, out byte[] thumbnail, out error)) return Bad(error);

        await m_databaseManager.InsertVideoEventAsync(e, eventType, eventTime, thumbnail, cancellationToken).ConfigureAwait(false);
        return Accepted("video event");
    }

    private async Task<(HttpStatusCode, string)> IngestHealthEventAsync(Stream body, CancellationToken cancellationToken)
    {
        if (!IngestionParsing.TryDeserialize(body, out HealthEventIngestion e, out string error)) return Bad(error);
        if (!Require(e.Source, "source", out error)) return Bad(error);
        if (!IngestionParsing.TryParseTimestamp(e.Timestamp, out DateTime timestamp, out error)) return Bad(error);

        await m_databaseManager.InsertHealthEventAsync(e, timestamp, cancellationToken).ConfigureAwait(false);
        return Accepted("health event");
    }

    private async Task<(HttpStatusCode, string)> IngestHealthStatisticAsync(Stream body, CancellationToken cancellationToken)
    {
        if (!IngestionParsing.TryDeserialize(body, out HealthStatisticsIngestion e, out string error)) return Bad(error);
        if (!Require(e.Source, "source", out error)) return Bad(error);
        // "Last error" is not an occurrence time; when omitted it is a sentinel (never), not "now".
        if (!IngestionParsing.TryParseTimestamp(e.LastErrorTimestamp, DateTime.MinValue, out DateTime lastError, out error)) return Bad(error);

        await m_databaseManager.InsertHealthStatisticAsync(e, lastError, cancellationToken).ConfigureAwait(false);
        return Accepted("health statistic");
    }

    private async Task<(HttpStatusCode, string)> IngestActivityTrailAsync(Stream body, CancellationToken cancellationToken)
    {
        if (!IngestionParsing.TryDeserialize(body, out ActivityTrailIngestion e, out string error)) return Bad(error);
        if (!IngestionParsing.TryParseTimestamp(e.Timestamp, out DateTime timestamp, out error)) return Bad(error);

        await m_databaseManager.InsertActivityTrailAsync(e, timestamp, cancellationToken).ConfigureAwait(false);
        return Accepted("activity trail");
    }

    private async Task<(HttpStatusCode, string)> IngestAuditTrailAsync(Stream body, CancellationToken cancellationToken)
    {
        if (!IngestionParsing.TryDeserialize(body, out AuditTrailIngestion e, out string error)) return Bad(error);
        if (!IngestionParsing.TryParseTimestamp(e.Timestamp, out DateTime timestamp, out error)) return Bad(error);

        await m_databaseManager.InsertAuditTrailAsync(e, timestamp, cancellationToken).ConfigureAwait(false);
        return Accepted("audit trail");
    }

    // Returns whether a custom event with the specified ID is defined in Security Center.
    private bool IsCustomEventDefined(int customEventId)
    {
        var configuration = (SystemConfiguration)m_engine.GetEntity(SystemConfiguration.SystemConfigurationGuid);
        return configuration.CustomEventService.CustomEvents.Any(customEvent => customEvent.Id == customEventId);
    }

    private static bool Require(Guid? value, string field, out string error)
    {
        if (value is null || value == Guid.Empty)
        {
            error = $"The {field} field is required.";
            return false;
        }

        error = null;
        return true;
    }

    private static bool IsNonEmpty(Guid? value) => value.HasValue && value.Value != Guid.Empty;

    private (HttpStatusCode, string) Accepted(string description)
    {
        m_logger?.TraceDebug($"Stored {description}");
        return (HttpStatusCode.NoContent, null);
    }

    private static (HttpStatusCode, string) Bad(string error) => (HttpStatusCode.BadRequest, error);
}
