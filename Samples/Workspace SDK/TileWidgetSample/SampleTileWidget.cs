// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples;

using System.Windows;
using Sdk.Workspace.Components.TileWidget;

public class SampleTileWidget : TileWidget
{
    private readonly SampleTileWidgetView m_view = new SampleTileWidgetView();

    public override UIElement View => m_view;

}