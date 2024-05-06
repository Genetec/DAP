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
            var notification = new SampleNotification();
            notification.Initialize(Workspace);
            Workspace.ActiveMonitor.Notifications.Add(notification);
        }

        public override void Unload()
        {
        }
    }
}