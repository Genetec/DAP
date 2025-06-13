// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples
{
    using System.Windows;
    using Sdk.Workspace.Components.TileWidget;

    public class SampleTileWidget : TileWidget
    {
        private readonly SampleTileWidgetView m_view = new SampleTileWidgetView();

        public override UIElement View => m_view;

    }
}