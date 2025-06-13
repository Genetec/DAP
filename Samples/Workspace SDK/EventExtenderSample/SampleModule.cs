// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples
{
    using Genetec.Sdk.Workspace.Services;
    using Sdk.Workspace.Modules;

    public class SampleModule : Module
    {
        public override void Load()
        {
            if (Workspace.ApplicationType == Sdk.ApplicationType.SecurityDesk)
            {
                var extender = new SampleEventExtender();
                extender.Initialize(Workspace);
                Workspace.Services.Get<IEventService>().RegisterEventExtender(extender);
            }
        }

        public override void Unload()
        {
        }
    }
}