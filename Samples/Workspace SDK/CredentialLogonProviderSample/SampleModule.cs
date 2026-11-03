// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using Sdk;
using Sdk.Workspace.Modules;

public sealed class SampleModule : Module
{
    private CredentialLogonProvider m_provider;

    public override void Load()
    {
        if (Workspace.ApplicationType is not (ApplicationType.SecurityDesk or ApplicationType.ConfigTool))
            return;

        m_provider = new CredentialLogonProvider();
        m_provider.Initialize(Workspace);
        Workspace.Components.Register(m_provider);
    }

    public override void Unload()
    {
        if (m_provider == null)
            return;

        Workspace.Components.Unregister(m_provider);
        m_provider.Dispose();
        m_provider = null;
    }
}
