// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries;

/// <summary>
/// Expands the entity selection of a query before it is translated to SQL.
/// Records store individual entity GUIDs, so group and area selections must be replaced with
/// the entities they contain.
/// </summary>
internal static class QueryEntityExpander
{
    /// <summary>
    /// Expands direct query entities and included expansion entities, then applies entities
    /// contained by the excluded expansion entities. The result distinguishes a finite inclusion
    /// set from a system-wide selection with finite exclusions.
    /// </summary>
    public static Task<QueryEntitySelection> ExpandAccessControlSelectionAsync(
        IEngine engine,
        IEnumerable<Guid> queryEntities,
        IEnumerable<Guid> includedExpansionEntities,
        IEnumerable<Guid> excludedExpansionEntities)
        => ExpandSelectionAsync(engine, queryEntities, includedExpansionEntities, excludedExpansionEntities, AreaExpansionMode.AccessControl);

    public static Task<QueryEntitySelection> ExpandZoneSelectionAsync(
        IEngine engine,
        IEnumerable<Guid> queryEntities,
        IEnumerable<Guid> includedExpansionEntities,
        IEnumerable<Guid> excludedExpansionEntities)
        => ExpandSelectionAsync(engine, queryEntities, includedExpansionEntities, excludedExpansionEntities, AreaExpansionMode.Zone);

    public static Task<QueryEntitySelection> ExpandIntrusionSelectionAsync(
        IEngine engine,
        IEnumerable<Guid> queryEntities,
        IEnumerable<Guid> excludedExpansionEntities)
        => ExpandSelectionAsync(engine, queryEntities, Array.Empty<Guid>(), excludedExpansionEntities, AreaExpansionMode.Intrusion);

    private static async Task<QueryEntitySelection> ExpandSelectionAsync(
        IEngine engine,
        IEnumerable<Guid> queryEntities,
        IEnumerable<Guid> includedExpansionEntities,
        IEnumerable<Guid> excludedExpansionEntities,
        AreaExpansionMode areaExpansionMode)
    {
        IReadOnlyCollection<Guid> direct = await ExpandAsync(engine, queryEntities, areaExpansionMode);
        IReadOnlyCollection<Guid> included = await ExpandAsync(engine, includedExpansionEntities, areaExpansionMode);

        bool isUnrestricted = direct is null || included is null;
        IReadOnlyCollection<Guid> excluded = await ExpandAsync(engine, excludedExpansionEntities, areaExpansionMode);
        if (excluded is null)
        {
            return QueryEntitySelection.Restricted(Array.Empty<Guid>());
        }

        if (isUnrestricted)
        {
            return QueryEntitySelection.Unrestricted(excluded);
        }

        var selection = new HashSet<Guid>(direct);
        selection.UnionWith(included);
        selection.ExceptWith(excluded);
        return QueryEntitySelection.Restricted(selection);
    }

    private static async Task<IReadOnlyCollection<Guid>> ExpandAsync(IEngine engine, IEnumerable<Guid> entityIds, AreaExpansionMode areaExpansionMode)
    {
        var expanded = new HashSet<Guid>();
        var visited = new HashSet<Guid>();
        var pending = new Queue<Guid>(entityIds ?? Enumerable.Empty<Guid>());

        while (pending.Count > 0)
        {
            // Load the entities of this level into the entity cache
            List<Guid> missing = pending
                .Where(entityId => entityId != SystemConfiguration.SystemConfigurationGuid && engine.GetEntity(entityId) is null)
                .ToList();
            if (missing.Count > 0)
            {
                var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
                foreach (Guid entityId in missing)
                {
                    query.EntityGuids.Add(entityId);
                }

                query.DownloadAllRelatedData = true;
                await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
            }

            int count = pending.Count;
            for (int i = 0; i < count; i++)
            {
                Guid entityId = pending.Dequeue();
                if (!visited.Add(entityId))
                {
                    continue;
                }

                if (entityId == SystemConfiguration.SystemConfigurationGuid)
                {
                    return null;
                }

                if (areaExpansionMode == AreaExpansionMode.AccessControl && engine.GetEntity(entityId) is CardholderGroup group)
                {
                    if (group.Guid == CardholderGroup.AllCardholdersGuid)
                    {
                        return null;
                    }

                    foreach (Guid child in group.Children)
                    {
                        pending.Enqueue(child);
                    }
                }
                else if (engine.GetEntity(entityId) is Area area)
                {
                    switch (areaExpansionMode)
                    {
                        case AreaExpansionMode.AccessControl:
                            expanded.Add(area.Guid);
                            expanded.UnionWith(area.AllDoors);
                            expanded.UnionWith(area.CaptiveAccessPoints);
                            expanded.UnionWith(area.EntryAccessPoints);
                            expanded.UnionWith(area.ExitAccessPoints);
                            expanded.UnionWith(area.Elevators);
                            break;

                        case AreaExpansionMode.Zone:
                            expanded.UnionWith(area.Zones);
                            break;

                        case AreaExpansionMode.Intrusion:
                            expanded.UnionWith(area.IntrusionAreas);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(areaExpansionMode), areaExpansionMode, null);
                    }

                    foreach (Guid childArea in area.CaptiveAreas)
                    {
                        pending.Enqueue(childArea);
                    }
                }
                else
                {
                    expanded.Add(entityId);
                }
            }
        }

        return expanded;
    }

    private enum AreaExpansionMode
    {
        AccessControl,
        Zone,
        Intrusion
    }
}

internal sealed class QueryEntitySelection
{
    private QueryEntitySelection(bool isUnrestricted, IReadOnlyCollection<Guid> included, IReadOnlyCollection<Guid> excluded)
    {
        IsUnrestricted = isUnrestricted;
        Included = included;
        Excluded = excluded;
    }

    public bool IsUnrestricted { get; }
    public IReadOnlyCollection<Guid> Included { get; }
    public IReadOnlyCollection<Guid> Excluded { get; }

    public static QueryEntitySelection Restricted(IReadOnlyCollection<Guid> included)
        => new(false, included, Array.Empty<Guid>());

    public static QueryEntitySelection Unrestricted(IReadOnlyCollection<Guid> excluded)
        => new(true, Array.Empty<Guid>(), excluded);
}
