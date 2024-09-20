// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples;

using System;
using Sdk.Workspace.Components;
using Sdk.Workspace.Components.TileWidget;
using Sdk.Workspace.Pages.Contents;

public class SampleTileWidgetBuilder : TileWidgetBuilder
{
    public override string Name => nameof(SampleTileWidgetBuilder);

    public override Guid UniqueId { get; } = new Guid("AAF2CC2A-A731-4882-98C1-87DF91AC6734");

    public override TileWidget CreateView()
    {
        return new SampleTileWidget();
    }

    public override bool IsSupported(TilePluginContext context)
    {
        return context.Content is EntityContent || context.ContentGroup is EntityContentGroup;
    }

    public override string Title  => "Sample Tile Widget";
}