// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries;

namespace Genetec.Dap.CodeSamples;

public class EntityMappingSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        await LoadCameras(engine);

        foreach (Camera camera in engine.GetEntities(EntityType.Camera).OfType<Camera>())
        {
            if (!TryGetMapping(engine, camera, out string data))
            {
                SaveMapping(engine, camera, "DATA");
            }

            Console.WriteLine($"{camera.Name,-20}: {data}");
        }
    }

    private async Task LoadCameras(Engine engine)
    {
        var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
        query.DownloadAllRelatedData = true;
        query.EntityTypeFilter.Add(EntityType.Camera);
        query.EntityTypeFilter.Add(EntityType.VideoUnit);
        await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
    }

    private void SaveMapping(Engine engine, Camera camera, string data)
    {
        if (engine.GetEntity(camera.Unit) is VideoUnit unit)
        {
            List<EntityMapping> mappings = unit.ArchiverRole.EntityMappings;

            EntityMapping mapping = mappings.FirstOrDefault(mapping => mapping.LocalEntityId == camera.Guid);

            if (mapping is null)
            {
                mappings.Add(new EntityMapping { LocalEntityId = camera.Guid, RoleId = unit.ArchiverRoleGuid, XmlInfo = data });
            }
            else
            {
                mapping.XmlInfo = data;
            }

            unit.ArchiverRole.EntityMappings = mappings;
        }
    }

    private bool TryGetMapping(Engine engine, Camera camera, out string data)
    {
        if (engine.GetEntity(camera.Unit) is VideoUnit unit)
        {
            EntityMapping mapping = unit.ArchiverRole.EntityMappings.FirstOrDefault(mapping => mapping.LocalEntityId == camera.Guid);

            if (mapping != null)
            {
                data = mapping.XmlInfo;
                return true;
            }
        }

        data = default;
        return false;
    }
}