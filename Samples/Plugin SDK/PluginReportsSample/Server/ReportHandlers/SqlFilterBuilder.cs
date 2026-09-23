// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.Data.SqlClient;
using Genetec.Sdk;
using Genetec.Sdk.Queries;

/// <summary>
/// Helpers to translate report query filters into parameterized SQL conditions.
/// </summary>
internal static class SqlFilterBuilder
{
    /// <summary>
    /// Returns a TOP clause when the query carries a maximum result count.
    /// One extra row is read so the handler can detect that more results were available
    /// and report <see cref="Genetec.Sdk.ReportError.TooManyResults"/>.
    /// </summary>
    public static string Top(int maximumResultCount)
        => maximumResultCount > 0 ? $" TOP ({(long)maximumResultCount + 1})" : string.Empty;

    /// <summary>
    /// Adds an IN condition backed by a single parameter containing the GUID list.
    /// No condition is added when the list is empty, meaning the filter is not restricted.
    /// </summary>
    public static void AddGuidFilter(ICollection<string> conditions, SqlCommand command, string column, IReadOnlyCollection<Guid> values, string parameterPrefix)
    {
        if (values.Count == 0)
        {
            return;
        }

        conditions.Add($"{column} IN ({AddGuidList(command, values, parameterPrefix)})");
    }

    /// <summary>
    /// Adds the GUIDs as one VARCHAR(MAX) parameter and returns a GUID subquery for IN or NOT IN.
    /// StringToGuidList is supplied by plugin database creation. Reusing the subquery across
    /// columns reuses the same parameter, regardless of the number of selected entities.
    /// </summary>
    public static string AddGuidList(SqlCommand command, IReadOnlyCollection<Guid> values, string parameterPrefix)
    {
        string name = $"@{parameterPrefix}";
        command.Parameters.Add(name, SqlDbType.VarChar, -1).Value = string.Join(",", values);
        return $"SELECT [GUID] FROM dbo.StringToGuidList({name})";
    }

    /// <summary>
    /// Adds a "column IN (1, 2, ...)" condition for a list of integer values, such as enumeration values.
    /// No condition is added when the list is empty, meaning the filter is not restricted.
    /// </summary>
    public static void AddIntFilter(ICollection<string> conditions, string column, IReadOnlyCollection<int> values)
    {
        if (values.Count == 0)
        {
            return;
        }

        conditions.Add($"{column} IN ({string.Join(", ", values)})");
    }

    /// <summary>
    /// Adds a parameterized string condition using the requested search mode.
    /// SQL wildcard characters in the supplied value are treated as literal characters.
    /// </summary>
    public static void AddStringFilter(ICollection<string> conditions, SqlCommand command, string column, string value, StringSearchMode searchMode, string parameterName)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        string comparison;
        string parameterValue;
        bool usesLike = true;
        string escapedValue = EscapeLikePattern(value);

        switch (searchMode)
        {
            case StringSearchMode.StartsWith:
                comparison = "LIKE";
                parameterValue = $"{escapedValue}%";
                break;

            case StringSearchMode.Contains:
                comparison = "LIKE";
                parameterValue = $"%{escapedValue}%";
                break;

            case StringSearchMode.EndsWith:
                comparison = "LIKE";
                parameterValue = $"%{escapedValue}";
                break;

            case StringSearchMode.Is:
                comparison = "=";
                parameterValue = value;
                usesLike = false;
                break;

            case StringSearchMode.DoesNotStartWith:
                comparison = "NOT LIKE";
                parameterValue = $"{escapedValue}%";
                break;

            case StringSearchMode.DoesNotEndWith:
                comparison = "NOT LIKE";
                parameterValue = $"%{escapedValue}";
                break;

            case StringSearchMode.DoesNotContain:
                comparison = "NOT LIKE";
                parameterValue = $"%{escapedValue}%";
                break;

            case StringSearchMode.IsNot:
                comparison = "<>";
                parameterValue = value;
                usesLike = false;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(searchMode), searchMode, "Unsupported string search mode.");
        }

        conditions.Add($"{column} {comparison} {parameterName}{(usesLike ? " ESCAPE '\\'" : string.Empty)}");
        command.Parameters.AddWithValue(parameterName, parameterValue);
    }

    /// <summary>
    /// Adds the event type condition of an activity query.
    /// Built-in event types are stored as their enumeration value; custom event IDs are stored negated.
    /// </summary>
    public static void AddEventTypeFilter(ICollection<string> conditions, string column, ActivityReportQuery query)
    {
        List<int> eventTypes = query.Events.Where(eventType => eventType != EventType.None).Select(eventType => (int)eventType)
            .Concat(query.CustomEvents.Select(customEventId => -customEventId))
            .ToList();

        AddIntFilter(conditions, column, eventTypes);
    }

    /// <summary>
    /// Appends the conditions to the statement as a WHERE/AND chain.
    /// </summary>
    public static void AppendConditions(StringBuilder sql, IReadOnlyList<string> conditions)
    {
        for (int i = 0; i < conditions.Count; i++)
        {
            sql.Append(i == 0 ? " WHERE " : " AND ").Append(conditions[i]);
        }
    }

    private static string EscapeLikePattern(string value)
        => value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_").Replace("[", "\\[");
}
