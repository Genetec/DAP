// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using Genetec.Sdk.Workspace.Maps;

    class SampleMapLayer : MapLayer
    {
        public override Guid Id { get; } = new Guid("957117D3-67D0-4652-A13A-8C5BE971B0BF");

        public override string Name { get; } = "Map Layer Sample";
    }
}
