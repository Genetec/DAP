// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System.Windows;
using Sdk.Workspace.Components.TileView;

public class SampleTileView : TileView
{
    private readonly SampleTileViewControl m_view;
    
    public SampleTileView()
    {
        m_view = new SampleTileViewControl();
    }

    public override UIElement View => m_view;
}