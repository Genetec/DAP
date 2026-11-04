// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using Sdk.Entities;
using Sdk.Entities.Maps;
using Sdk.Workspace.Components;
using Sdk.Workspace.Components.MapObjectViewBuilder;
using Sdk.Workspace.Maps;

public sealed class AedMapObjectViewBuilder : MapObjectViewBuilder
{
    public override string Name => "AED map objects";
    public override Guid UniqueId => new("99458D57-807C-410A-92EB-7033A393AC0B");

    // Lower numbers run first, before the SDK's default custom entity renderer.
    public override int Priority => 0;

    public override IEnumerable<IMapObjectView> CreateViews(IEnumerable<MapObject> mapObjects, MapContext context)
    {
        var views = new List<IMapObjectView>();

        foreach (var mapObject in mapObjects)
        {
            // Replace only existing placements of the AED type defined by ConfigPageSample.
            if (mapObject is CustomEntityMapObject customObject
                && Workspace.Sdk.GetEntity(customObject.LinkedEntity) is CustomEntity entity
                && entity.CustomEntityType == AedUnitCustomEntityType.Id)
            {
                views.Add(new AedMapObjectView(Workspace, customObject));
            }
        }

        return views;
    }
}
