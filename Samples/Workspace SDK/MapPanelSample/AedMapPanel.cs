// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Sdk.Workspace;
using Sdk.Workspace.Components.MapPanel;

public sealed class AedMapPanel : MapPanel
{
    public AedMapPanel()
    {
        Title = "AED maintenance";
        DockPosition = Dock.Right;
    }

    public override Guid UniqueId => new("481E0425-C8D5-47B5-BFFD-140508A322B6");
    public override Size MinDockedSize => new(300, 200);
    public override Size MaxDockedSize => new(500, double.PositiveInfinity);
    public override IList<Dock> SupportedDockPositions => [Dock.Left, Dock.Right];

    // A side panel leaves the map center visible when locating an AED.
    public override bool SupportsFloating => false;

    public override void Initialize(Workspace workspace)
    {
        Workspace = workspace;
        ((AedMapPanelView)View).Initialize(this);
    }

    protected override UIElement CreateView()
    {
        // The SDK calls this from its base constructor, before Initialize supplies Workspace.
        return new AedMapPanelView();
    }

    protected override ImageSource CreateIcon()
    {
        var drawing = new DrawingGroup();
        drawing.Children.Add(new GeometryDrawing(Brushes.ForestGreen, null, new RectangleGeometry(new Rect(0, 0, 24, 24), 4, 4)));
        drawing.Children.Add(new GeometryDrawing(Brushes.White, null, Geometry.Parse("M 9,4 L 15,4 L 15,9 L 20,9 L 20,15 L 15,15 L 15,20 L 9,20 L 9,15 L 4,15 L 4,9 L 9,9 Z")));
        drawing.Freeze();
        return new DrawingImage(drawing);
    }

    protected override void OnMapControlChanged()
    {
        ((AedMapPanelView)View).SetMapControl(MapControl);
    }
}
