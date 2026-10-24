// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System.Windows;

public partial class ControlsSampleView
{
    private string m_displayName = "Sample";

    public ControlsSampleView()
    {
        InitializeComponent();
        customWindowResult.Text = $"Display name: {m_displayName}";
    }

    private void OnOpenCustomWindowClicked(object sender, RoutedEventArgs e)
    {
        using var window = new CustomWindowSampleWindow(m_displayName)
        {
            Owner = Window.GetWindow(this)
        };

        if (window.ShowDialog() == true)
        {
            m_displayName = window.DisplayName;
            customWindowResult.Text = $"Saved display name: {m_displayName}";
        }
        else
        {
            customWindowResult.Text = $"Canceled. Display name remains: {m_displayName}";
        }
    }
}
