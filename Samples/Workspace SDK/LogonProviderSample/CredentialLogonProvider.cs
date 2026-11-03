// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using Sdk.Workspace.Components.LogonProvider;

public sealed class CredentialLogonProvider : LogonProvider
{
    private CredentialLogonWindow m_window;

    public override string Name => "Windows Credential Manager logon";
    public override Guid UniqueId => new("83DB108C-16F7-46A0-91AE-BDAF4E432397");

    // When choosing between custom logon providers, Security Center prefers the
    // highest Priority value. Use 100 to prefer this sample over providers using 0.
    public override int Priority => 100;

    public override void LogOn()
    {
        if (m_window != null)
        {
            m_window.Activate();
            return;
        }

        m_window = new CredentialLogonWindow(LogOnAsync, IsIdleMode)
        {
            Owner = Workspace.ActiveMonitor.Window
        };
        try
        {
            m_window.ShowDialog();
        }
        finally
        {
            m_window = null;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            m_window?.RequestClose();

        base.Dispose(disposing);
    }
}
