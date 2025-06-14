// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples;

using Sdk;
using Sdk.Workspace.Modules;
using Sdk.Workspace.Services;

public class SampleModule : Module
{
    public override void Load()
    {
        if (Workspace.ApplicationType is ApplicationType.ConfigTool or ApplicationType.SecurityDesk)
        {
            IContextualActionsService contextualActionsService = Workspace.Services.Get<IContextualActionsService>();

            // Register the contextual action group
            var actionGroup = new SampleContextualActionGroup();
            actionGroup.Initialize(Workspace);
            contextualActionsService.Register(actionGroup);

            // Register the contextual action
            var action = new SampleContextualAction();
            action.Initialize(Workspace);
            contextualActionsService.Register(action);
        }
    }

    public override void Unload()
    {
    }
}