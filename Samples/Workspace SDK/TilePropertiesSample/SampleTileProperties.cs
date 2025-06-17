// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0

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
