// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples
{
    using Sdk;
    using Sdk.Workspace.Modules;

    public class SampleModule : Module
    {
        public override void Load()
        {
            if (Workspace.ApplicationType == ApplicationType.SecurityDesk)
            {
                var builder = new ClockWidgetBuilder();
                builder.Initialize(Workspace);
                Workspace.Components.Register(builder);
            }
        }

        public override void Unload()
        {
        }
    }
}
