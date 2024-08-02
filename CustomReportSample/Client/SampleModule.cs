// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples.Client
{
    using Genetec.Sdk.Workspace.Modules;
    using Genetec.Sdk.Workspace.Tasks;

    /// <summary>
    /// Client module for the custom report sample.
    /// </summary>
    public class SampleModule : Module
    {
        public override void Load()
        {
            // Register the custom report page task for Security Desk only
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
