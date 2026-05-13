// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using Genetec.Sdk;
using Sdk.Workspace.Modules;

public class SampleModule : Module
{
    private SampleCardholderFieldsExtractor m_extractor;

    public override void Load()
    {
        if (Workspace.ApplicationType is ApplicationType.SecurityDesk or ApplicationType.ConfigTool)
        {
            m_extractor = new SampleCardholderFieldsExtractor();
            m_extractor.Initialize(Workspace);
            Workspace.Components.Register(m_extractor);
        }
    }

    public override void Unload()
    {
        if (m_extractor != null)
        {
            Workspace.Components.Unregister(m_extractor);
            m_extractor = null;
        }
    }
}