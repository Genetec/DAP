// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using Sdk.Entities;
using Sdk.Workspace.Components.MapPanel;

public sealed class AedMapPanelBuilder : MapPanelBuilder
{
    public override string Name => "AED map panel";
    public override Guid UniqueId => new("F1CFC3A5-5198-4599-8D31-55EDB8ED57B1");

    public override MapPanel CreateView()
    {
        return new AedMapPanel();
    }

    public override bool IsSupported(MapPanelContext context)
    {
        // The Maps task can identify the displayed map by its owning area.
        var entity = Workspace.Sdk.GetEntity(context.MapId);
        return entity is Map || entity is Area area && area.MapId != Guid.Empty;
    }
}
