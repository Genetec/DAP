// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using Sdk;
using Sdk.Workspace.Modules;

public sealed class SampleModule : Module
{
    private AedMapObjectViewBuilder m_mapObjects;

    public override void Load()
    {
        // Both clients can display AED markers on maps.
        if (Workspace.ApplicationType is not (ApplicationType.SecurityDesk or ApplicationType.ConfigTool))
        {
            return;
        }

        m_mapObjects = new AedMapObjectViewBuilder();
        m_mapObjects.Initialize(Workspace);
        Workspace.Components.Register(m_mapObjects);
    }

    public override void Unload()
    {
        if (m_mapObjects != null)
        {
            Workspace.Components.Unregister(m_mapObjects);
        }
    }
}
