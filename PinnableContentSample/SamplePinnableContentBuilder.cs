// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using Genetec.Sdk.Workspace.Components.PinnableContentBuilder;
    using System;
    using System.Windows;

    class SamplePinnableContentBuilder : PinnableContentBuilder
    {
        public override string Name => nameof(SamplePinnableContentBuilder);

        public override Guid UniqueId { get; } = new Guid("6F5EC0B5-E89C-4DE6-8EBC-D69520F15456");
        
        public override FrameworkElement CreateContent()
        {
            return new PinnableContentView();
        }
    }
}