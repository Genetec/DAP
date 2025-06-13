// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples
{
    using System.Windows;
    using Sdk.Workspace.Components.TileProperties;

    public class SampleTileProperties : TileProperties
    {
        private readonly SampleTilePropertiesView m_view = new SampleTilePropertiesView();

        public override UIElement View => m_view;
    }
}