// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples.Client;

using System;
using Genetec.Sdk.Workspace.Components.CustomAction;

class SampleCustomActionBuilder : CustomActionBuilder
{
    public override CustomActionView CreateView()
    {
        var view =  new SampleCustomActionView();
        view.Initialize(Workspace);
        return view;
    }

    public override bool IsSupported(CustomActionContext context)
    {
        // Determine if the action is supported based on the context.
        return true;
    }

    // TODO: Replace with your own unique custom action type GUID
    public override Guid CustomActionType => CustomActionTypes.LaunchEncoderCommand;
}