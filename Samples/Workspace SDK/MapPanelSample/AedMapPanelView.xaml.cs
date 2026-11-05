// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml;
using Sdk;
using Sdk.Entities;
using Sdk.Entities.Maps;
using Sdk.Workspace;
using Sdk.Workspace.Maps;

public partial class AedMapPanelView : UserControl
{
    private AedMapPanel m_panel;
    private IMapControl m_map;
    private readonly DispatcherTimer m_calendar;
    private readonly DispatcherTimer m_selectionRetry;
    private MapObject m_pendingSelection;
    private DateTime m_selectionDeadline;
    private Guid[] m_entityIds = [];
    private Guid[] m_selectedIds = [];
    private bool m_attached;
    private bool m_updating;
    private DateTime m_today;

    public AedMapPanelView()
    {
        InitializeComponent();
        m_calendar = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        m_calendar.Tick += OnCalendar;
        m_selectionRetry = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        m_selectionRetry.Tick += OnSelectionRetry;
    }

    public void Initialize(AedMapPanel panel)
    {
        m_panel = panel;
        SetMapControl(panel.MapControl);
    }

    public void SetMapControl(IMapControl map)
    {
        DetachMap();
        m_map = map;
        m_selectedIds = [];

        AttachMap();
        Refresh();
    }

    private Map GetCurrentMap(bool watchEntities = false)
    {
        if (watchEntities)
        {
            m_entityIds = [m_map.Map];
        }

        var entity = m_panel.Workspace.Sdk.GetEntity(m_map.Map);

        if (entity is Area area)
        {
            if (watchEntities)
            {
                // Keep watching the map even when it has not reached the entity cache yet.
                m_entityIds = [m_map.Map, area.MapId];
            }

            entity = m_panel.Workspace.Sdk.GetEntity(area.MapId);
        }

        return entity as Map;
    }

    private bool CanView(CustomEntity entity)
    {
        var sdk = m_panel.Workspace.Sdk;
        return entity != null && entity.CustomEntityType == AedUnitCustomEntityType.Id
            && (sdk.LoggedUser.IsAdministrator || sdk.SecurityManager.IsPrivilegeGranted(AedUnitCustomPrivilege.View, entity.Guid));
    }

    private void Refresh()
    {
        if (m_panel?.Workspace == null)
        {
            return;
        }

        var items = new List<AedItem>();
        m_entityIds = [];
        MapNameText.Content = "AEDs on this map";
        MessageText.Text = "Select an AED to locate its marker.";

        if (!m_panel.Workspace.Sdk.IsConnected)
        {
            m_selectedIds = [];
            MessageText.Text = "Connect to Security Center to view AEDs.";
        }
        else if (m_map != null && GetCurrentMap(watchEntities: true) is Map map)
        {
            MapNameText.Content = map.Name;
            var placements = map.MapObjects.OfType<CustomEntityMapObject>().ToList();
            m_entityIds = [.. placements.Select(placement => placement.LinkedEntity), map.Guid, m_map.Map];

            foreach (var placement in placements)
            {
                var entity = m_panel.Workspace.Sdk.GetEntity(placement.LinkedEntity) as CustomEntity;

                if (CanView(entity))
                {
                    items.Add(CreateItem(entity, placement));
                }
            }

            if (items.Count == 0)
            {
                MessageText.Text = "No visible AEDs have saved placements on this map.";
            }
        }
        else
        {
            MessageText.Text = "The current map is not available yet.";
        }

        if (m_pendingSelection != null && !items.Any(item => item.Placement.Id == m_pendingSelection.Id))
        {
            CancelPendingSelection();
        }

        m_today = DateTime.Today;
        m_updating = true;

        try
        {
            AedList.ItemsSource = items.OrderBy(item => item.Name).ToList();
            AedList.SelectedItem = items.FirstOrDefault(item => m_pendingSelection != null
                ? item.Placement.Id == m_pendingSelection.Id
                : m_selectedIds.Contains(item.Placement.Id));
        }
        finally
        {
            m_updating = false;
        }

        SummaryText.Content = $"AEDs on this map: {items.Count}";
        m_panel.Count = items.Count;

        if (AedList.SelectedItem is AedItem selected)
        {
            MessageText.Text = selected.Summary;
        }
    }

    private static AedItem CreateItem(CustomEntity entity, MapObject placement)
    {
        var item = new AedItem { Name = entity.Name, Placement = placement };

        try
        {
            var information = AedUnitInformation.Deserialize(entity.Xml)
                ?? throw new SerializationException("The AED configuration is empty.");
            var maintenance = AedMaintenance.Evaluate(information, entity.IsInMaintenance, DateTime.Today);
            item.Status = maintenance.State switch
            {
                AedMaintenanceState.Current => "Dates current",
                AedMaintenanceState.DueSoon => "Due within 30 days",
                AedMaintenanceState.Unknown => "Dates missing",
                AedMaintenanceState.Overdue => "Overdue",
                AedMaintenanceState.InvalidData => "Invalid data",
                _ => "In maintenance"
            };
            item.Summary = maintenance.Summary;
        }
        catch (Exception ex) when (ex is SerializationException or XmlException or ArgumentException)
        {
            item.Status = "Invalid data";
            item.Summary = "The AED maintenance configuration cannot be read. Check it in Config Tool.";
        }

        return item;
    }

    private void OnAedSelected(object sender, SelectionChangedEventArgs e)
    {
        if (m_updating)
        {
            return;
        }

        CancelPendingSelection();

        if (AedList.SelectedItem is not AedItem item || m_map == null || !m_panel.Workspace.Sdk.IsConnected)
        {
            return;
        }

        var entity = m_panel.Workspace.Sdk.GetEntity(item.Placement.LinkedEntity) as CustomEntity;
        var placement = GetCurrentMap()?.MapObjects.FirstOrDefault(candidate => candidate.Id == item.Placement.Id);

        if (!CanView(entity) || placement == null)
        {
            Refresh();
            return;
        }

        m_pendingSelection = placement;
        m_selectionDeadline = DateTime.UtcNow.AddSeconds(10);
        m_selectionRetry.Start();

        // Offscreen marker views are created asynchronously after the map moves.
        m_map.ForceCenter(new GeoCoordinate(placement.Latitude, placement.Longitude));
        TrySelectPending();
        MessageText.Text = item.Summary;
    }

    private void OnSelectionRetry(object sender, EventArgs e)
    {
        if (m_pendingSelection == null)
        {
            return;
        }

        var sdk = m_panel.Workspace.Sdk;
        var placement = sdk.IsConnected
            ? GetCurrentMap()?.MapObjects.FirstOrDefault(candidate => candidate.Id == m_pendingSelection.Id)
            : null;

        if (placement == null || !CanView(sdk.GetEntity(placement.LinkedEntity) as CustomEntity))
        {
            CancelPendingSelection();
            Refresh();
            return;
        }

        m_pendingSelection = placement;
        TrySelectPending();
    }

    private void TrySelectPending()
    {
        if (m_pendingSelection == null)
        {
            return;
        }

        if (DateTime.UtcNow >= m_selectionDeadline)
        {
            CancelPendingSelection();
            Refresh();
            MessageText.Text = "The AED marker is not available. Check the map layers and try again.";
            return;
        }

        m_map.Select(m_pendingSelection, keepCurrentSelection: false);
    }

    private void CancelPendingSelection()
    {
        m_selectionRetry.Stop();
        m_pendingSelection = null;
    }

    private void OnMapInput(object sender, RoutedEventArgs e)
    {
        if (m_pendingSelection != null)
        {
            CancelPendingSelection();
            Refresh();
        }
    }

    private void OnMapSelectionChanged(object sender, GuidsEventArgs e)
    {
        // Keep the selection even if its AED row has not reached the list yet.
        m_selectedIds = [.. e.SelectedGuids];

        if (m_pendingSelection != null)
        {
            // Loading can change the old selection; operator input cancels retries separately.
            if (!m_selectedIds.Contains(m_pendingSelection.Id))
            {
                return;
            }

            CancelPendingSelection();
        }

        m_updating = true;

        try
        {
            AedList.SelectedItem = AedList.Items.OfType<AedItem>().FirstOrDefault(item => m_selectedIds.Contains(item.Placement.Id));
            MessageText.Text = (AedList.SelectedItem as AedItem)?.Summary ?? "Select an AED to locate its marker.";
        }
        finally
        {
            m_updating = false;
        }
    }

    private void OnMapChanged(object sender, bool e)
    {
        CancelPendingSelection();
        m_selectedIds = [];
        Refresh();
    }

    private void OnRefresh(object sender, RoutedEventArgs e)
    {
        Refresh();
    }

    private void OnEntitiesChanged(object sender, EntitiesUpdateEventArgs e)
    {
        if (e.Entities.Any(update => m_entityIds.Contains(update.EntityGuid)
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
        if (!m_attached || Dispatcher.HasShutdownStarted)
        {
            return;
        }

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
        if (m_today != DateTime.Today)
        {
            Refresh();
        }
    }

    private void AttachMap()
    {
        if (m_map != null)
        {
            m_map.MapChanged += OnMapChanged;
            m_map.SelectionChanged += OnMapSelectionChanged;

            if (m_map.InputElement is FrameworkElement host)
            {
                // The map stays loaded when its side panel is closed.
                host.Loaded += OnMapLoaded;
                host.Unloaded += OnMapUnloaded;
                host.PreviewMouseDown += OnMapInput;
                host.PreviewKeyDown += OnMapInput;

                if (host.IsLoaded)
                {
                    OnMapLoaded(host, new RoutedEventArgs());
                }
            }
        }
    }

    private void DetachMap()
    {
        OnMapUnloaded(this, new RoutedEventArgs());

        if (m_map != null)
        {
            m_map.MapChanged -= OnMapChanged;
            m_map.SelectionChanged -= OnMapSelectionChanged;

            if (m_map.InputElement is FrameworkElement host)
            {
                host.Loaded -= OnMapLoaded;
                host.Unloaded -= OnMapUnloaded;
                host.PreviewMouseDown -= OnMapInput;
                host.PreviewKeyDown -= OnMapInput;
            }
        }
    }

    private void OnMapLoaded(object sender, RoutedEventArgs e)
    {
        if (m_attached || m_panel?.Workspace == null)
        {
            return;
        }

        m_attached = true;
        var sdk = m_panel.Workspace.Sdk;
        sdk.EntitiesAdded += OnEntitiesChanged;
        sdk.EntitiesInvalidated += OnEntitiesChanged;
        sdk.EntitiesRemoved += OnEntitiesChanged;
        sdk.LoggedOn += OnSessionChanged;
        sdk.LoggedOff += OnSessionChanged;
        m_calendar.Start();
        Refresh();
    }

    private void OnMapUnloaded(object sender, RoutedEventArgs e)
    {
        CancelPendingSelection();

        if (!m_attached)
        {
            return;
        }

        m_attached = false;
        var sdk = m_panel.Workspace.Sdk;
        sdk.EntitiesAdded -= OnEntitiesChanged;
        sdk.EntitiesInvalidated -= OnEntitiesChanged;
        sdk.EntitiesRemoved -= OnEntitiesChanged;
        sdk.LoggedOn -= OnSessionChanged;
        sdk.LoggedOff -= OnSessionChanged;
        m_calendar.Stop();
    }

    private sealed class AedItem
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string Summary { get; set; }
        public MapObject Placement { get; set; }
    }
}
