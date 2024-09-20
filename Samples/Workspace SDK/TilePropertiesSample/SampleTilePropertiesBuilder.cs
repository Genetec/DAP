// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples;

using System;
using Sdk.Workspace.Components;
using Sdk.Workspace.Components.TileProperties;
using Sdk.Workspace.Pages.Contents;

public class SampleTilePropertiesBuilder : TilePropertiesBuilder
{
    public override string Name => nameof(SampleTilePropertiesBuilder);

    public override Guid UniqueId { get; } = new Guid("25DD268E-A7AA-41DF-89FF-FAB6C0A74D57");

    public override TileProperties CreateView()
    {
        return new SampleTileProperties();
    }

    public override bool IsSupported(TilePluginContext context)
    {
        return context.Content is EntityContent || context.ContentGroup is EntityContentGroup;
    }

    public override string Title => "Sample Tile Properties";
}