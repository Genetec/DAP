// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Windows;
using Sdk.Controls;
using Sdk.Workspace;

public partial class ControlsSampleView : IDisposable
{
    private readonly Workspace m_workspace;
    private PinnablePopup m_popup;
    private string m_displayName = "Sample";

    public ControlsSampleView(Workspace workspace)
    {
        m_workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
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

    private void OnShowPopupClicked(object sender, RoutedEventArgs e)
    {
        if (m_popup == null)
        {
            m_popup = new PinnablePopup(Guid.NewGuid(), Window.GetWindow(this))
            {
                Header = "Sample notes",
                PlacementTarget = showPopupButton,
                PopupContent = new PinnablePopupSampleView()
            };
            m_popup.Initialize(m_workspace);
            m_popup.IsPinnedChanged += OnPopupPinnedChanged;
            hidePopupButton.IsEnabled = true;
        }

        m_popup.Show();
        UpdatePopupStatus();
    }

    private void OnHidePopupClicked(object sender, RoutedEventArgs e)
    {
        m_popup?.Hide();
        popupStatus.Text = "Hidden.";
    }

    private void OnPopupPinnedChanged(object sender, EventArgs e)
    {
        UpdatePopupStatus();
    }

    private void UpdatePopupStatus()
    {
        popupStatus.Text = m_popup.IsPinned ? "Pinned to monitor window." : "Not pinned.";
    }

    public void Dispose()
    {
        if (m_popup == null)
            return;

        m_popup.IsPinnedChanged -= OnPopupPinnedChanged;
        m_popup.Hide();
        m_popup.Dispose();
        m_popup = null;
    }
}
