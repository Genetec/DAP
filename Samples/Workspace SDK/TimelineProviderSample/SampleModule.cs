// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using Genetec.Sdk.Workspace.Components;
using Sdk;
using Sdk.Workspace.Modules;

public class SampleModule : Module
{
    private AlarmTimelineProviderBuilder m_alarmBuilder;
    private AccessControlTimelineProviderBuilder m_accessControlBuilder;

    public override void Load()
    {
        if (Workspace.ApplicationType == ApplicationType.SecurityDesk)
        {
            m_alarmBuilder = new AlarmTimelineProviderBuilder();
            Register(m_alarmBuilder);

            m_accessControlBuilder = new AccessControlTimelineProviderBuilder();
            Register(m_accessControlBuilder);

            void Register(Component component)
            {
                component.Initialize(Workspace);
                Workspace.Components.Register(component);
            }
        }
    }

    public override void Unload()
    {
        if (m_alarmBuilder != null)
        {
            Workspace.Components.Unregister(m_alarmBuilder);
            m_alarmBuilder = null;
        }

        if (m_accessControlBuilder != null)
        {
            Workspace.Components.Unregister(m_accessControlBuilder);
            m_accessControlBuilder = null;
        }
    }
}
