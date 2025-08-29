// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Sdk.Entities;
using Sdk;
using Sdk.Controls.Maps;
using Sdk.Queries;
using Sdk.Workspace;

public class MapControlSampleViewModel : BindableBase
{
    private readonly Workspace m_workspace;
    private ObservableCollection<Map> m_availableMaps;
    private Map m_selectedMap;
    private MapControl m_mapControl;

    public MapControlSampleViewModel(Workspace workspace)
    {
        m_workspace = workspace;
        m_availableMaps = new ObservableCollection<Map>();

        m_mapControl = new MapControl();
        m_mapControl.Initialize(m_workspace);

        _ = LoadAvailableMaps();
    }

    public ObservableCollection<Map> AvailableMaps
    {
        get => m_availableMaps;
        private set => SetProperty(ref m_availableMaps, value);
    }

    public Map SelectedMap
    {
        get => m_selectedMap;
        set
        {
            if (SetProperty(ref m_selectedMap, value))
            {
                LoadSelectedMap();
            }
        }
    }

    public MapControl MapControl
    {
        get => m_mapControl;
        private set => SetProperty(ref m_mapControl, value);
    }

    public bool IsZoomControlVisible
    {
        get => MapControl.IsZoomControlVisible;
        set
        {
            if (MapControl.IsZoomControlVisible != value)
            {
                MapControl.IsZoomControlVisible = value;
                RaisePropertyChanged();
            }
        }
    }

    public bool IsNavigationControlVisible
    {
        get => MapControl.IsNavigationControlVisible;
        set
        {
            if (MapControl.IsNavigationControlVisible != value)
            {
                MapControl.IsNavigationControlVisible = value;
                RaisePropertyChanged();
            }
        }
    }

    public bool IsPresetsControlVisible
    {
        get => MapControl.IsPresetsControlVisible;
        set
        {
            if (MapControl.IsPresetsControlVisible != value)
            {
                MapControl.IsPresetsControlVisible = value;
                RaisePropertyChanged();
            }
        }
    }

    public bool IsSendToTilesControlVisible
    {
        get => MapControl.IsSendToTilesControlVisible;
        set
        {
            if (MapControl.IsSendToTilesControlVisible != value)
            {
                MapControl.IsSendToTilesControlVisible = value;
                RaisePropertyChanged();
            }
        }
    }

    public bool IsSmartClickControlVisible
    {
        get => MapControl.IsSmartClickControlVisible;
        set
        {
            if (MapControl.IsSmartClickControlVisible != value)
            {
                MapControl.IsSmartClickControlVisible = value;
                RaisePropertyChanged();
            }
        }
    }

    private async Task LoadAvailableMaps()
    {
        var query = (EntityConfigurationQuery)m_workspace.Sdk.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
        query.EntityTypeFilter.Add(EntityType.Map);
        await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null).ConfigureAwait(true);

        var maps = m_workspace.Sdk.GetEntities(EntityType.Map).OfType<Map>().OrderBy(map => map.Name);

        AvailableMaps.Clear();
        foreach (var map in maps)
        {
            AvailableMaps.Add(map);
        }

        if (AvailableMaps.Any())
        {
            SelectedMap = AvailableMaps.First();
        }
    }

    private void LoadSelectedMap()
    {
        if (SelectedMap != null)
        {
            MapControl.Map = SelectedMap.Guid;
        }
    }
}