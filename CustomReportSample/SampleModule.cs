// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using Sdk.Workspace.Modules;
    using Sdk.Workspace.Tasks;

    public class SampleModule : Module
    {
        public override void Load()
        {
            if (Workspace.ApplicationType == Sdk.ApplicationType.SecurityDesk)
            {
                var task = new CreatePageTask<CustomReportPage>();
                task.Initialize(Workspace);
                Workspace.Tasks.Register(task);
            }
        }

        public override void Unload()
        {
        }
    }
}
