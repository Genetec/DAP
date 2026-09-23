// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Ingestion;

using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using System.Xml;
using Genetec.Sdk;

/// <summary>
/// Shared parsing and validation used by every ingestion endpoint: JSON deserialization, the
/// built-in / custom event resolution and its storage convention, timestamp parsing, and
/// Base64 picture decoding. Each method returns false with a caller-facing error message rather
/// than throwing, so the endpoints can turn a bad field into an HTTP 400.
/// </summary>
public static class IngestionParsing
{
    /// <summary>The decoded size limit of a posted image (picture or thumbnail).</summary>
    public const int MaxImageBytes = 1024 * 1024;

    private static readonly Regex s_iso8601WithOffset = new(
        @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{1,7})?(?:Z|[+-]\d{2}:\d{2})$",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    // A DataContractJsonSerializer is thread-safe once constructed, so one per payload type is
    // cached here instead of built on every request.
    private static class Serializer<T>
    {
        public static readonly DataContractJsonSerializer Instance = new(typeof(T));
    }

    /// <summary>
    /// Deserializes a request body into the payload type, returning false when the body is not
    /// valid JSON for that type.
    /// </summary>
    public static bool TryDeserialize<T>(Stream body, out T payload, out string error)
    {
        try
        {
            payload = (T)Serializer<T>.Instance.ReadObject(body);
        }
        catch (Exception exception) when (exception is SerializationException or XmlException)
        {
            payload = default;
            error = "The request body is not a valid event.";
            return false;
        }

        // ReadObject returns null (without throwing) for a body that is the JSON literal null;
        // treat that as an invalid body with a message rather than a silent failure.
        if (payload is null)
        {
            error = "The request body is empty.";
            return false;
        }

        error = null;
        return true;
    }

    /// <summary>
    /// Resolves the value stored in an event-type column from a payload's eventType (a built-in
    /// <see cref="EventType"/> name) or customEventId. Exactly one must be provided. Built-in
    /// types are stored as their enumeration value; custom event IDs are stored negated, which an
    /// event column resolves back to the custom event's name.
    /// </summary>
    public static bool TryResolveEventId(string eventType, int? customEventId, Func<int, bool> isCustomEventDefined, out int eventId, out string error)
    {
        eventId = 0;

        if (customEventId is int id)
        {
            if (!string.IsNullOrEmpty(eventType))
            {
                error = "Provide either eventType or customEventId, not both.";
                return false;
            }

            if (!isCustomEventDefined(id))
            {
                error = $"Unknown custom event ID: {id}";
                return false;
            }

            eventId = -id; // custom event IDs are stored negated
            error = null;
            return true;
        }

        if (string.IsNullOrEmpty(eventType))
        {
            error = "Provide an eventType or a customEventId.";
            return false;
        }

        // Enum.TryParse also accepts any numeric string, including undefined and negative values;
        // Enum.IsDefined rejects those, so a numeric eventType cannot forge a negated custom-event
        // ID or store an event type that names nothing.
        if (!Enum.TryParse(eventType, ignoreCase: true, out EventType parsed) || !Enum.IsDefined(typeof(EventType), parsed))
        {
            error = $"Unknown event type: {eventType}";
            return false;
        }

        eventId = (int)parsed;
        error = null;
        return true;
    }

    /// <summary>
    /// Parses an ISO 8601 timestamp with an explicit offset as UTC, defaulting to the current
    /// time when the value is empty (the common case for a system that posts events as they happen).
    /// </summary>
    public static bool TryParseTimestamp(string value, out DateTime utc, out string error)
        => TryParseTimestamp(value, DateTime.UtcNow, out utc, out error);

    /// <summary>
    /// Parses an ISO 8601 timestamp with an explicit offset as UTC, using the supplied default
    /// when the value is empty.
    /// Callers pass a default other than "now" for timestamps that are not event-occurrence
    /// times, for example a "last error" time that should be a sentinel when no error occurred.
    /// </summary>
    public static bool TryParseTimestamp(string value, DateTime whenEmpty, out DateTime utc, out string error)
    {
        error = null;

        if (string.IsNullOrEmpty(value))
        {
            utc = whenEmpty;
            return true;
        }

        if (TryParseTimestampOffset(value, out DateTimeOffset timestamp))
        {
            utc = timestamp.UtcDateTime;
            return true;
        }

        utc = default;
        error = $"The timestamp is not a valid ISO 8601 date with an explicit UTC offset: {value}";
        return false;
    }

    /// <summary>
    /// Parses the local zone-activity timestamp without converting its wall-clock value to UTC.
    /// When the value is empty, derives the local value from the UTC event timestamp and the
    /// supplied Windows time-zone identifier.
    /// </summary>
    public static bool TryParseLocalTimestamp(string value, DateTime eventTimestampUtc, string timeZoneId, out DateTime local, out string error)
    {
        string effectiveTimeZoneId = string.IsNullOrWhiteSpace(timeZoneId) ? "UTC" : timeZoneId;

        TimeZoneInfo timeZone;
        try
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(effectiveTimeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            local = default;
            error = $"The timeZoneId is not a recognized Windows time zone: {effectiveTimeZoneId}";
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            local = default;
            error = $"The timeZoneId is invalid: {effectiveTimeZoneId}";
            return false;
        }

        if (string.IsNullOrEmpty(value))
        {
            local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(eventTimestampUtc, DateTimeKind.Utc), timeZone);
            error = null;
            return true;
        }

        if (!TryParseTimestampOffset(value, out DateTimeOffset timestamp))
        {
            local = default;
            error = $"The local timestamp is not a valid ISO 8601 date with an explicit UTC offset: {value}";
            return false;
        }

        TimeSpan expectedOffset = timeZone.GetUtcOffset(timestamp.UtcDateTime);
        if (timestamp.Offset != expectedOffset)
        {
            local = default;
            error = $"The local timestamp offset does not match timeZoneId {effectiveTimeZoneId}.";
            return false;
        }

        local = DateTime.SpecifyKind(timestamp.DateTime, DateTimeKind.Unspecified);
        error = null;
        return true;
    }

    /// <summary>
    /// Decodes a Base64 image, returning null (not an error) when the value is empty, and an
    /// error when it is not valid Base64 or exceeds the size limit.
    /// </summary>
    public static bool TryParseImage(string base64, out byte[] bytes, out string error)
    {
        bytes = null;
        error = null;

        if (string.IsNullOrEmpty(base64))
        {
            return true;
        }

        try
        {
            bytes = Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            error = "The image is not valid Base64.";
            return false;
        }

        if (bytes.Length > MaxImageBytes)
        {
            bytes = null;
            error = "The image exceeds the 1 MB limit.";
            return false;
        }

        return true;
    }

    private static bool TryParseTimestampOffset(string value, out DateTimeOffset timestamp)
    {
        timestamp = default;
        return s_iso8601WithOffset.IsMatch(value)
            && DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out timestamp);
    }
}
