// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using Sdk;
using Sdk.Workspace.Modules;

public sealed class SampleModule : Module
{
    private AedMapPanelBuilder m_panels;

    public override void Load()
    {
        // Both clients support custom panels in their map views.
        if (Workspace.ApplicationType is not (ApplicationType.SecurityDesk or ApplicationType.ConfigTool))
        {
            return;
        }

        m_panels = new AedMapPanelBuilder();
        m_panels.Initialize(Workspace);
        Workspace.Components.Register(m_panels);
    }

    public override void Unload()
    {
        if (m_panels != null)
        {
            Workspace.Components.Unregister(m_panels);
        }
    }
}
