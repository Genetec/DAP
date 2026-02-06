// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using Genetec.Sdk;
using Sdk.Workspace.Modules;

public class SampleModule : Module
{
    private SampleImageExtractor m_imageExtractor;

    public override void Load()
    {
        if (Workspace.ApplicationType is ApplicationType.SecurityDesk or ApplicationType.ConfigTool)
        {
            m_imageExtractor = new SampleImageExtractor();
            m_imageExtractor.Initialize(Workspace);
            Workspace.Components.Register(m_imageExtractor);
        }
    }

    public override void Unload()
    {
        if (m_imageExtractor != null)
        {
            Workspace.Components.Unregister(m_imageExtractor);
            m_imageExtractor = null;
        }
    }
}