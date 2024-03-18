// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using Sdk.Workspace.Tasks;
    using Sdk;

    public class SampleModule : Sdk.Workspace.Modules.Module
    {
        public override void Load()
        {
            if (Workspace.ApplicationType == ApplicationType.SecurityDesk)
            {
                var task  = new CreatePageTask<SamplePage>();
                task.Initialize(Workspace);
                Workspace.Tasks.Register(task);
            }
        }

        public override void Unload()
        {
        }
    }
}