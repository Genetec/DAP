// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using Sdk;
    using Sdk.Workspace.Modules;
    using Sdk.Workspace.Services;

    public class SampleModule : Module
    {
        public override void Load()
        {
            if (Workspace.ApplicationType == ApplicationType.ConfigTool)
            {
                var page = new CustomConfigPage();
                page.Initialize(Workspace);
                IConfigurationService service = Workspace.Services.Get<IConfigurationService>();
                service.Register(page);
            }
        }

        public override void Unload()
        {
        }
    }
}