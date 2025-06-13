// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples
{
    using Sdk;

    public class SampleModule : Sdk.Workspace.Modules.Module
    {
        public override void Load()
        {
            if (Workspace.ApplicationType == ApplicationType.SecurityDesk)
            {
                var builder = new SampleTileViewBuilder();
                builder.Initialize(Workspace);
                Workspace.Components.Register(builder);
            }
        }

        public override void Unload()
        {
        }
    }
}
