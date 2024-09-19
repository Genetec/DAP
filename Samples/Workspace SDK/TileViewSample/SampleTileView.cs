// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
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
}