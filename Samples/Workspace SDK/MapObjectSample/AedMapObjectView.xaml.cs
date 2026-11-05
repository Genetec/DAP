// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using Sdk;
using Sdk.Controls.Themes.Styles;
using Sdk.Entities;
using Sdk.Entities.Maps;
using Sdk.Workspace;
using Sdk.Workspace.Maps;

public partial class AedMapObjectView : MapObjectView
{
    private readonly Workspace m_workspace;
    private readonly TextBlock m_name = new() { TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.SemiBold };
    private readonly TextBlock m_summary = new() { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 0) };
    private readonly TextBlock m_dates = new() { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 0) };
    private readonly Label m_bubble;
    private readonly DispatcherTimer m_calendar;
    private bool m_attached;
    private bool m_canView;
    private DateTime m_today;

    public AedMapObjectView(Workspace workspace, CustomEntityMapObject mapObject)
    {
        m_workspace = workspace;
        InitializeComponent();

        // Attach this view to the saved map placement. The host owns its position.
        Initialize(mapObject);

        // Content margin is respected even when the native label template ignores Padding.
        var details = new StackPanel { Margin = new Thickness(12), MaxWidth = 320 };
        details.Children.Add(m_name);
        details.Children.Add(m_summary);
        details.Children.Add(m_dates);

        m_bubble = new Label
        {
            Content = details,
            Style = (Style)new StylesExtension(LabelStyles.Normal).ProvideValue(null)
        };

        m_calendar = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        m_calendar.Tick += OnCalendar;
        Refresh();
    }

    protected override void OnClick(RoutedEventArgs e)
    {
        if (m_canView)
        {
            IsSelected = !IsSelected;
        }

        e.Handled = true;
    }

    protected override void OnDoubleClick(RoutedEventArgs e)
    {
        if (m_canView)
        {
            IsSelected = true;
        }

        e.Handled = true;
    }

    protected override void OnIsSelectedChanged()
    {
        base.OnIsSelectedChanged();

        if (IsSelected && m_canView)
        {
            // Use the SDK bubble and native label style for the selected marker's details.
            DisplayBubbleContent(m_bubble);
        }
        else
        {
            HideBubbleContent();
        }
    }

    private void Refresh()
    {
        if (!m_workspace.Sdk.IsConnected)
        {
            HideMarker();
            return;
        }

        var entity = m_workspace.Sdk.GetEntity(MapObject.LinkedEntity) as CustomEntity;
        m_canView = entity != null
            && entity.CustomEntityType == AedUnitCustomEntityType.Id
            && (m_workspace.Sdk.LoggedUser.IsAdministrator
                || m_workspace.Sdk.SecurityManager.IsPrivilegeGranted(AedUnitCustomPrivilege.View, entity.Guid));

        if (!m_canView)
        {
            HideMarker();
            return;
        }

        MarkerContent.Visibility = Visibility.Visible;
        m_today = DateTime.Today;
        m_name.Text = entity.Name;

        try
        {
            var information = AedUnitInformation.Deserialize(entity.Xml)
                ?? throw new SerializationException("The AED configuration is empty.");
            var maintenance = AedMaintenance.Evaluate(information, entity.IsInMaintenance, m_today);

            MarkerBody.Background = GetStatusBrush(maintenance.State);
            MarkerIndicator.Text = maintenance.State switch
            {
                AedMaintenanceState.Current => "",
                AedMaintenanceState.InMaintenance => "M",
                AedMaintenanceState.Unknown or AedMaintenanceState.InvalidData => "?",
                _ => "!"
            };

            m_summary.Text = string.IsNullOrWhiteSpace(entity.Description)
                ? maintenance.Summary
                : entity.Description + "\n" + maintenance.Summary;

            m_dates.Visibility = Visibility.Visible;
            m_dates.Text = "Last inspection: " + FormatDate(information.LastInspectionDate)
                + "\nNext maintenance: " + FormatDate(information.NextScheduledMaintenance)
                + "\nBattery expires: " + FormatDate(information.BatteryExpirationDate)
                + "\nPads expire: " + FormatDate(information.PadExpirationDate);
        }
        catch (Exception ex) when (ex is SerializationException or XmlException or ArgumentException)
        {
            MarkerBody.Background = Brushes.DarkMagenta;
            MarkerIndicator.Text = "?";
            m_summary.Text = "The AED maintenance configuration cannot be read. Check it in Config Tool.";
            m_dates.Text = "";
            m_dates.Visibility = Visibility.Collapsed;
        }

        ToolTip = (m_name.Text + "\n" + m_summary.Text + "\n" + m_dates.Text).TrimEnd();
    }

    private void HideMarker()
    {
        // The map host controls root visibility, so hide our own content instead.
        m_canView = false;
        IsSelected = false;
        MarkerContent.Visibility = Visibility.Collapsed;
        ToolTip = null;
        m_name.Text = "";
        m_summary.Text = "";
        m_dates.Text = "";
        MarkerIndicator.Text = "";
        MarkerBody.Background = null;
        HideBubbleContent();
    }

    private static Brush GetStatusBrush(AedMaintenanceState state)
    {
        return state switch
        {
            AedMaintenanceState.Current => Brushes.DarkGreen,
            AedMaintenanceState.DueSoon => Brushes.DarkOrange,
            AedMaintenanceState.Overdue => Brushes.Firebrick,
            AedMaintenanceState.InvalidData => Brushes.DarkMagenta,
            AedMaintenanceState.InMaintenance => Brushes.SlateBlue,
            _ => Brushes.DimGray
        };
    }

    private static string FormatDate(DateTime date)
    {
        return date == DateTime.MinValue ? "Not recorded" : date.ToString("yyyy-MM-dd");
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (m_attached)
        {
            return;
        }

        m_attached = true;
        m_workspace.Sdk.EntitiesAdded += OnEntitiesChanged;
        m_workspace.Sdk.EntitiesInvalidated += OnEntitiesChanged;
        m_workspace.Sdk.EntitiesRemoved += OnEntitiesChanged;
        m_workspace.Sdk.LoggedOn += OnSessionChanged;
        m_workspace.Sdk.LoggedOff += OnSessionChanged;
        m_calendar.Start();
        Refresh();
    }

    private void OnEntitiesChanged(object sender, EntitiesUpdateEventArgs e)
    {
        // User, group, and partition changes can affect access to this AED.
        if (e.Entities.Any(update => update.EntityGuid == MapObject.LinkedEntity
            || update.EntityType is EntityType.User or EntityType.UserGroup or EntityType.Partition))
        {
            QueueRefresh();
        }
    }

    private void OnSessionChanged(object sender, EventArgs e)
    {
        QueueRefresh();
    }

    private void QueueRefresh()
    {
        // SDK notifications can arrive off the WPF thread.
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (m_attached)
            {
                Refresh();
            }
        }));
    }

    private void OnCalendar(object sender, EventArgs e)
    {
        // Re-evaluate deadlines at midnight even when the entity has not changed.
        if (m_today != DateTime.Today)
        {
            Refresh();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        m_attached = false;
        m_calendar.Stop();
        m_workspace.Sdk.EntitiesAdded -= OnEntitiesChanged;
        m_workspace.Sdk.EntitiesInvalidated -= OnEntitiesChanged;
        m_workspace.Sdk.EntitiesRemoved -= OnEntitiesChanged;
        m_workspace.Sdk.LoggedOn -= OnSessionChanged;
        m_workspace.Sdk.LoggedOff -= OnSessionChanged;
        HideBubbleContent();
    }
}
