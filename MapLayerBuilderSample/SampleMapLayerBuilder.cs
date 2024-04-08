// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using Sdk.Workspace.Components;
    using Sdk.Workspace.Maps;

    class SampleMapLayerBuilder : MapLayerBuilder
    {
        public override string Name => nameof(SampleMapLayerBuilder);

        public override Guid UniqueId { get; } = new Guid("CD43D46E-8C2E-4E7D-AA62-D73F1F786552");

        public override IList<MapLayer> CreateLayers(MapContext context)
        {
            return new List<MapLayer> { new SampleMapLayer() };
        }

        public override bool IsSupported(MapContext context)
        {
            return true;
        }
    }
}