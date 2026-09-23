// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers;

using System;
using Microsoft.Data.SqlClient;

/// <summary>
/// Name-based read helpers, so the record mappings reference columns by name
/// instead of by position in the SELECT statement.
/// </summary>
internal static class SqlDataReaderExtensions
{
    public static Guid GetGuid(this SqlDataReader reader, string column)
        => reader.GetGuid(reader.GetOrdinal(column));

    /// <summary>
    /// Reads a nullable UNIQUEIDENTIFIER column, returning <see cref="Guid.Empty"/> when the
    /// value is NULL. The report renders an empty GUID as "no entity".
    /// </summary>
    public static Guid GetGuidOrDefault(this SqlDataReader reader, string column)
    {
        int ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? Guid.Empty : reader.GetGuid(ordinal);
    }

    public static int GetInt32(this SqlDataReader reader, string column)
        => reader.GetInt32(reader.GetOrdinal(column));

    public static long GetInt64(this SqlDataReader reader, string column)
        => reader.GetInt64(reader.GetOrdinal(column));

    public static float GetFloat(this SqlDataReader reader, string column)
        => reader.GetFloat(reader.GetOrdinal(column));

    public static string GetString(this SqlDataReader reader, string column)
        => reader.GetString(reader.GetOrdinal(column));

    /// <summary>
    /// Reads a DATETIME2 column as a UTC <see cref="DateTime"/>. The reader returns
    /// unspecified kinds, and the plugin stores all timestamps in UTC.
    /// </summary>
    public static DateTime GetUtcDateTime(this SqlDataReader reader, string column)
        => DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal(column)), DateTimeKind.Utc);

    public static string GetStringOrNull(this SqlDataReader reader, string column)
    {
        int ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    public static byte[] GetBytesOrNull(this SqlDataReader reader, string column)
    {
        int ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : (byte[])reader[ordinal];
    }
}
