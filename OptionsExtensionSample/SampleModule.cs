// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

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
                var extensions = new SampleOptionsExtensions();
                extensions.Initialize(Workspace);
                Workspace.Options.Register(extensions);
            }
        }

        public override void Unload()
        {
        }
    }
}