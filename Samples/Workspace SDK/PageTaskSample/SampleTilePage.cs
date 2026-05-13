// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries;
using Genetec.Sdk.Workspace.Commands;
using Genetec.Sdk.Workspace.Pages;
using Genetec.Sdk.Workspace.Pages.Contents;
using Genetec.Sdk.Workspace.Pages.Tiles;
using Genetec.Sdk.Workspace.Services;
using Genetec.Sdk.Workspace.SharedComponents;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Monitor = Sdk.Workspace.Monitors.Monitor;

// This attribute associates the page with its descriptor
[Page(typeof(SampleTilePageDescriptor))]
public class SampleTilePage : TilePage
{
    private SampleTilePageView m_view;

    // Shared component for the tile canvas
    private SharedComponent m_tileCanvas;

    private bool m_isActivated;
    private bool m_hasPattern;

    // Pattern commands
    public DelegateCommand ModifyPatternCommand { get; private set; }
    public DelegateCommand CyclePatternNextCommand { get; private set; }
    public DelegateCommand CyclePatternPreviousCommand { get; private set; }

    // Tile selection commands
    public DelegateCommand SelectNextTileCommand { get; private set; }
    public DelegateCommand SelectPreviousTileCommand { get; private set; }

    // Tile display commands
    public DelegateCommand ToggleExpandTileCommand { get; private set; }
    public DelegateCommand ToggleFullscreenTileCommand { get; private set; }

    // Content cycling commands
    public DelegateCommand ToggleStartStopCyclingCommand { get; private set; }

    // Video commands
    public DelegateCommand SynchronizeVideoCommand { get; private set; }
    public DelegateCommand SaveSnapshotCommand { get; private set; }
    public DelegateCommand ExportVideoCommand { get; private set; }
    public DelegateCommand ExportVideoAllTilesCommand { get; private set; }
    public DelegateCommand BrowseVaultCommand { get; private set; }
    public DelegateCommand ToggleDigitalZoomCommand { get; private set; }

    // Entity browser command
    public DelegateCommand DisplayEntityCommand { get; private set; }

    // Clear commands
    public DelegateCommand ClearTileCommand { get; private set; }

    public DelegateCommand ClearAllTilesCommand { get; private set; }

    public bool IsActivated
    {
        get => m_isActivated;
        private set
        {
            if (m_isActivated != value)
            {
                m_isActivated = value;
                OnPropertyChanged();
                RefreshCommands();
            }
        }
    }

    public bool HasPattern
    {
        get => m_hasPattern;
        private set
        {
            if (m_hasPattern != value)
            {
                m_hasPattern = value;
                OnPropertyChanged();
                RefreshCommands();
            }
        }
    }

    protected override void Initialize()
    {
        InitializeCommands();

        m_view = new SampleTilePageView();
        m_view.DataContext = this;
        View = m_view;

        LoadCameras();
    }

    protected override void OnActivated(Monitor monitor)
    {
        // Get the tile canvas from the monitor's shared components
        m_tileCanvas = monitor?.SharedComponents[SharedComponents.TileCanvas].FirstOrDefault();
        if (m_tileCanvas != null)
        {
            m_view.m_tilesHost.Content = m_tileCanvas;
            m_tileCanvas?.Connect();
            LoadTiles();
        }

        IsActivated = true;
    }

    protected override void OnDeactivated(Monitor monitor)
    {
        IsActivated = false;
        HasPattern = false;

        // Disconnect the tile canvas and clear the content
        m_tileCanvas?.Disconnect();
        m_view.m_tilesHost.Content = null;
    }

    private void InitializeCommands()
    {
        // Pattern commands
        ModifyPatternCommand = new DelegateCommand(
            () => Workspace.Commands.Execute(WorkspaceCommands.ModifyPattern, null),
            () => m_isActivated && m_hasPattern);

        CyclePatternNextCommand = new DelegateCommand(
            () => Workspace.Commands.Execute(WorkspaceCommands.CyclePatternNext, 0),
            () => m_isActivated && m_hasPattern);

        CyclePatternPreviousCommand = new DelegateCommand(
            () => Workspace.Commands.Execute(WorkspaceCommands.CyclePatternPrevious, null),
            () => m_isActivated && m_hasPattern);

        // Tile selection commands
        SelectNextTileCommand = new DelegateCommand(
            () => Workspace.Commands.Execute(WorkspaceCommands.SelectNextTile, null),
            () => m_isActivated && m_hasPattern);

        SelectPreviousTileCommand = new DelegateCommand(
            () => Workspace.Commands.Execute(WorkspaceCommands.SelectPreviousTile, null),
            () => m_isActivated && m_hasPattern);

        // Tile display commands
        ToggleExpandTileCommand = new DelegateCommand(
            () => Workspace.Commands.Execute(WorkspaceCommands.ToggleExpandTile, null),
            () => m_isActivated && m_hasPattern);

        ToggleFullscreenTileCommand = new DelegateCommand(
            () => Workspace.Commands.Execute(WorkspaceCommands.ToggleFullscreenTile, null),
            () => m_isActivated && m_hasPattern);

        // Content cycling commands
        ToggleStartStopCyclingCommand = new DelegateCommand(
            () => Workspace.Commands.Execute(WorkspaceCommands.ToggleStartStopCycling, null),
            () => m_isActivated && m_hasPattern);

        // Video commands
        SynchronizeVideoCommand = new DelegateCommand(
            () => Workspace.Commands.Execute(WorkspaceCommands.SynchronizeVideo, null),
            () => m_isActivated && m_hasPattern);

        SaveSnapshotCommand = new DelegateCommand(
            () => Workspace.Commands.Execute(WorkspaceCommands.SaveSnapshot, null),
            () => m_isActivated && m_hasPattern);

        ExportVideoCommand = new DelegateCommand(
            () => Workspace.Commands.Execute(WorkspaceCommands.ExportVideo, null),
            () => m_isActivated && m_hasPattern);

        ExportVideoAllTilesCommand = new DelegateCommand(
            () => Workspace.Commands.Execute(WorkspaceCommands.ExportVideoAllTiles, null),
            () => m_isActivated && m_hasPattern);

        BrowseVaultCommand = new DelegateCommand(
            () => Workspace.Commands.Execute(WorkspaceCommands.BrowseVault, null),
            () => m_isActivated);

        ToggleDigitalZoomCommand = new DelegateCommand(
            () => Workspace.Commands.Execute(WorkspaceCommands.ToggleDigitalZoom, null),
            () => m_isActivated && m_hasPattern);

        // Entity browser command
        DisplayEntityCommand = new DelegateCommand(
            DisplayEntityInSelectedTile,
            () => m_isActivated && m_hasPattern);

        // Clear commands
        ClearTileCommand = new DelegateCommand(
            () => Workspace.Commands.Execute(WorkspaceCommands.ClearTile, null),
            () => m_isActivated && m_hasPattern);

        ClearAllTilesCommand = new DelegateCommand(
            () => Workspace.Commands.Execute(WorkspaceCommands.ClearAllTiles, null),
            () => m_isActivated && m_hasPattern);
    }

    private void RefreshCommands()
    {
        ModifyPatternCommand.RaiseCanExecuteChanged();
        CyclePatternNextCommand.RaiseCanExecuteChanged();
        CyclePatternPreviousCommand.RaiseCanExecuteChanged();
        SelectNextTileCommand.RaiseCanExecuteChanged();
        SelectPreviousTileCommand.RaiseCanExecuteChanged();
        ToggleExpandTileCommand.RaiseCanExecuteChanged();
        ToggleFullscreenTileCommand.RaiseCanExecuteChanged();
        ToggleStartStopCyclingCommand.RaiseCanExecuteChanged();
        SynchronizeVideoCommand.RaiseCanExecuteChanged();
        SaveSnapshotCommand.RaiseCanExecuteChanged();
        ExportVideoCommand.RaiseCanExecuteChanged();
        ExportVideoAllTilesCommand.RaiseCanExecuteChanged();
        BrowseVaultCommand.RaiseCanExecuteChanged();
        ToggleDigitalZoomCommand.RaiseCanExecuteChanged();
        DisplayEntityCommand.RaiseCanExecuteChanged();
        ClearTileCommand.RaiseCanExecuteChanged();
        ClearAllTilesCommand.RaiseCanExecuteChanged();
    }

    private void DisplayEntityInSelectedTile()
    {
        // Show the entity browser dialog allowing the user to select a single entity
        List<Guid> selectedEntities = Workspace.Services.Get<IDialogService>().ShowEntityBrowserDialog(new EntityTypeCollection([EntityType.Camera]), null, false, SelectionMode.Single);

        if (selectedEntities.Count > 0)
        {
            // Build content for the selected entity and display it in the active tile
            ContentGroup content = Workspace.Services.Get<IContentBuilderService>().Build(selectedEntities[0]);
            if (content != null)
            {
                Workspace.Commands.Execute(WorkspaceCommands.DisplayEntity, selectedEntities[0]);
            }
        }
    }

    private void LoadCameras()
    {
        // Create a query to fetch camera entities
        var query = (EntityConfigurationQuery)Workspace.Sdk.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
        query.EntityTypeFilter.Add(EntityType.Camera);
        query.MaximumResultCount = 4; // Limit to 4 cameras

        // Execute the query asynchronously
        Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
    }

    // Loads camera tiles into the page
    private void LoadTiles()
    {
        // Get the first 4 cameras from the entity cache
        List<Camera> cameras = Workspace.Sdk.GetEntities(EntityType.Camera).OfType<Camera>().Take(4).ToList();

        // Create a 2x2 grid pattern for the tiles
        Pattern = TilePattern.Create(2, 2);

        IContentBuilderService builderService = Workspace.Services.Get<IContentBuilderService>();

        // Populate each tile with a camera's video content
        for (int index = 0; index < cameras.Count; index++)
        {
            // Create a content group for the camera using the content builder service
            ContentGroup context = builderService.Build(cameras[index].Guid);
            States[index].Content = context;
        }

        HasPattern = true;
    }
}
