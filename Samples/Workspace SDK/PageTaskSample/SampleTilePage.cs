// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Workspace.Pages.Contents;
using Genetec.Sdk.Workspace.Pages.Tiles;
using Genetec.Sdk.Workspace.SharedComponents;
using Genetec.Sdk.Workspace.Pages;
using Genetec.Sdk.Queries;
using Genetec.Sdk.Entities;
using Monitor = Sdk.Workspace.Monitors.Monitor;
using Genetec.Sdk.Workspace.Services;

// This attribute associates the page with its descriptor
[Page(typeof(SampleTilePageDescriptor))]
public class SampleTilePage : TilePage
{
    // The view associated with this page
    private readonly SampleTilePageView m_view = new();

    public SampleTilePage() => View = m_view;

    // Shared component for the tile canvas
    private SharedComponent m_tileCanvas;

    protected override void OnActivated(Monitor monitor)
    {
        // Get the tile canvas from the monitor's shared components
        m_tileCanvas = monitor?.SharedComponents[SharedComponents.TileCanvas].FirstOrDefault();
        if (m_tileCanvas != null)
        {
            m_view.m_tilesHost.Content = m_tileCanvas; // Set the tile canvas as the content of the tiles host in the view
            m_tileCanvas?.Connect();  // Connect the tile canvas
            LoadTiles(); // Load the tiles with camera content
        }
    }

    protected override void Initialize()
    {
        LoadCameras();
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

    protected override void OnDeactivated(Monitor monitor)
    {
        // Disconnect the tile canvas and clear the content
        m_tileCanvas?.Disconnect();
        m_view.m_tilesHost.Content = null;
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
    }
}