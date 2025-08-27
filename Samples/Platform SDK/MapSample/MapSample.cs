// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Entities.Maps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genetec.Dap.CodeSamples;

public class MapSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        var filePath = Path.Combine(Environment.CurrentDirectory, "OfficeFloor.png");

        var officeArea = (Area)engine.CreateEntity("Office", EntityType.Area);
        _ = officeArea.CreateMap(new DocumentMapCreationInfo(filePath: filePath, documentType: MapDocumentTypes.Image, zoomLevel: 1));

        var singaporeArea = (Area)engine.CreateEntity("Singapore", EntityType.Area);
        Map singaporeMap = singaporeArea.CreateMap(new GeographicalMapCreationInfo(MapProviders.Bing));
        singaporeMap.DefaultView = new GeoView(new GeoCoordinate(1.3521, 103.8198, 15.0), 1);

        // Load all maps into the entity cache
        await LoadEntities(engine, token, EntityType.Map);

        // Retrieve all maps from the entity cache
        IEnumerable<Map> maps = engine.GetEntities(EntityType.Map).OfType<Map>();

        foreach (Map map in maps)
        {
            DisplayMapProperties(map, engine);
        }
    }

    private void DisplayMapProperties(Map map, Engine engine)
    {
        Console.WriteLine($"Map: {map.Name}");
        Console.WriteLine($"  Guid: {map.Guid}");
        Console.WriteLine($"  MaxZoom: {map.MaxZoom}");
        Console.WriteLine($"  IsGeoReferenced: {map.IsGeoReferenced}");
        Console.WriteLine($"  Provider: {map.Provider}");
        Console.WriteLine($"  ProviderId: {map.ProviderId}");

        DisplayGeoReference(map.GeoReference);
        DisplayGeoView(map.DefaultView);
        DisplayMapObjects(map, engine);

        Console.WriteLine();
    }

    private void DisplayGeoView(GeoView view)
    {
        if (view == null)
        {
            Console.WriteLine("  DefaultView: null");
            return;
        }

        Console.WriteLine("  DefaultView:");
        Console.WriteLine($"    Center: {FormatCoordinate(view.Center)}");
        Console.WriteLine($"    ScaleFactor: {(view.ScaleFactor.HasValue ? view.ScaleFactor.Value.ToString() : "null")}");
        DisplayGeoBounds(view.ViewArea);
    }

    private void DisplayGeoBounds(GeoBounds bounds)
    {
        if (bounds == null)
        {
            Console.WriteLine("    ViewArea: null");
            return;
        }

        Console.WriteLine("    ViewArea:");
        Console.WriteLine($"      Name: {bounds.Name}");
        Console.WriteLine($"      North: {bounds.North}");
        Console.WriteLine($"      South: {bounds.South}");
        Console.WriteLine($"      East: {bounds.East}");
        Console.WriteLine($"      West: {bounds.West}");
        Console.WriteLine($"      Height: {bounds.Height}");
        Console.WriteLine($"      Width: {bounds.Width}");
        Console.WriteLine($"      Center: {FormatCoordinate(bounds.Center)}");
        Console.WriteLine($"      Northeast: {FormatCoordinate(bounds.Northeast)}");
        Console.WriteLine($"      Northwest: {FormatCoordinate(bounds.Northwest)}");
        Console.WriteLine($"      Southeast: {FormatCoordinate(bounds.Southeast)}");
        Console.WriteLine($"      Southwest: {FormatCoordinate(bounds.Southwest)}");
    }

    private void DisplayGeoReference(GeoReference geoReference)
    {
        if (geoReference == null)
        {
            Console.WriteLine("  GeoReference: null");
            return;
        }

        Console.WriteLine("  GeoReference:");

        int count = Math.Min(geoReference.SourcePoints.Count, geoReference.TargetPoints.Count);
        for (int i = 0; i < count; i++)
        {
            var src = geoReference.SourcePoints[i];
            var tgt = geoReference.TargetPoints[i];
            Console.WriteLine($"    Point {i}: Pixel=({src.X}, {src.Y}) => Geo=Lat={tgt.Latitude}, Lon={tgt.Longitude}, Alt={tgt.Altitude}");
        }
    }

    private void DisplayMapObjects(Map map, Engine engine)
    {
        if (map.MapObjects.Count == 0)
        {
            Console.WriteLine("  MapObjects: None");
            return;
        }

        Console.WriteLine("  MapObjects:");
        foreach (var mapObject in map.MapObjects)
        {
            Console.WriteLine($"    Type: {mapObject.GetType().Name}");
            Console.WriteLine($"      LayerId: {mapObject.LayerId}");
            Console.WriteLine($"      Latitude: {mapObject.Latitude}");
            Console.WriteLine($"      Longitude: {mapObject.Longitude}");
            Console.WriteLine($"      Rotation: {mapObject.Rotation}");
            Console.WriteLine($"      ZIndex: {mapObject.ZIndex}");

            // Display relative dimensions if applicable
            if (mapObject is MapRelativeObject mapRelativeObject)
            {
                Console.WriteLine($"      Relative Dimensions:");
                Console.WriteLine($"        Height: {mapRelativeObject.RelativeHeight}");
                Console.WriteLine($"        Width: {mapRelativeObject.RelativeWidth}");
            }

            // Display action type if executable
            if (mapObject.CanExecuteAction)
            {
                Console.WriteLine($"      Action Type: {mapObject.ActionType}");
            }

            // Display linked entity
            Console.WriteLine($"      Linked Entity: {(mapObject.LinkedEntity != Guid.Empty ? engine.GetEntity(mapObject.LinkedEntity)?.Name ?? "Unknown" : "None")}");

            // Display links
            if (mapObject.Links.Any())
            {
                Console.WriteLine("      Links:");
                foreach (Uri link in mapObject.Links)
                {
                    Console.WriteLine($"        {link}");
                }
            }
            else
            {
                Console.WriteLine("      Links: None");
            }

            // Display specific properties based on object type
            DisplaySpecificMapObjectProperties(mapObject, engine);
        }
    }

    private void DisplaySpecificMapObjectProperties(MapObject mapObject, Engine engine)
    {
        switch (mapObject)
        {
            case AreaMapObject m:
                Console.WriteLine($"      Area Properties:");
                Console.WriteLine($"        Identity: {engine.GetEntity(m.Identity)?.Name ?? "Unknown"}");
                Console.WriteLine($"        Altitude: {m.Altitude}");
                Console.WriteLine($"        Block Field Of View: {m.BlockFieldOfView}");
                Console.WriteLine($"        Block Field Of View Using Altitude: {m.BlockFieldOfViewUsingAltitude}");
                Console.WriteLine($"        Opacity: {m.Opacity}");
                Console.WriteLine($"        Border Color: {m.BorderColor}");
                Console.WriteLine($"        Background: {m.Background}");
                Console.WriteLine($"        Border Thickness: {m.BorderThickness}");
                break;

            case CameraMapObject m:
                Console.WriteLine($"      Camera Properties:");
                Console.WriteLine($"        Altitude: {m.Altitude}");
                Console.WriteLine($"        Angle Of View: {m.AngleOfView}");
                Console.WriteLine($"        Distance: {m.Distance}");
                Console.WriteLine($"        FovColor: {m.FovColor}");
                Console.WriteLine($"        Max Distance: {m.MaxDistance}");
                Console.WriteLine($"        Offset: {m.Offset}");
                Console.WriteLine($"        Hardware Offset: {m.HardwareOffsest}");
                Console.WriteLine($"        Zoom: {m.Zoom}");
                Console.WriteLine($"        Is Reversed: {m.IsReversed}");
                Console.WriteLine($"        Show Field Of View: {m.ShowFieldOfView}");
                break;

            case EllipseMapObject m:
                Console.WriteLine($"      Ellipse Properties:");
                Console.WriteLine($"        Altitude: {m.Altitude}");
                Console.WriteLine($"        Dimensions:");
                Console.WriteLine($"          Height: {m.RelativeHeight}");
                Console.WriteLine($"          Width: {m.RelativeWidth}");
                Console.WriteLine($"        Background: {m.Background}");
                Console.WriteLine($"        Border Color: {m.BorderColor}");
                Console.WriteLine($"        Border Thickness: {m.BorderThickness}");
                Console.WriteLine($"        Opacity: {m.Opacity}");
                Console.WriteLine($"        Block Field Of View: {m.BlockFieldOfView}");
                Console.WriteLine($"        Block Field Of View Using Altitude: {m.BlockFieldOfViewUsingAltitude}");

                if (m.Points.Any())
                {
                    Console.WriteLine($"        Points:");
                    foreach (var point in m.Points)
                    {
                        Console.WriteLine($"          X={point.X}, Y={point.Y}");
                    }
                }
                break;

            case PolygonMapObject m:
                Console.WriteLine($"      Polygon Properties:");
                Console.WriteLine($"        Altitude: {m.Altitude}");
                Console.WriteLine($"        Background: {m.Background}");
                Console.WriteLine($"        Border Color: {m.BorderColor}");
                Console.WriteLine($"        Border Thickness: {m.BorderThickness}");
                Console.WriteLine($"        Block Field Of View: {m.BlockFieldOfView}");
                Console.WriteLine($"        Block Field Of View Using Altitude: {m.BlockFieldOfViewUsingAltitude}");
                Console.WriteLine($"        Is Polyline: {m.IsPolyline}");
                Console.WriteLine($"        Opacity: {m.Opacity}");

                if (m.Points.Any())
                {
                    Console.WriteLine($"        Points:");
                    foreach (var point in m.Points)
                    {
                        Console.WriteLine($"          X={point.X}, Y={point.Y}");
                    }
                }
                break;

            case RectangleMapObject m:
                Console.WriteLine($"      Rectangle Properties:");
                Console.WriteLine($"        Altitude: {m.Altitude}");
                Console.WriteLine($"        Background: {m.Background}");
                Console.WriteLine($"        Block Field Of View: {m.BlockFieldOfView}");
                Console.WriteLine($"        Block Field Of View Using Altitude: {m.BlockFieldOfViewUsingAltitude}");
                Console.WriteLine($"        Opacity: {m.Opacity}");

                if (m.Points.Any())
                {
                    Console.WriteLine($"        Points:");
                    foreach (var point in m.Points)
                    {
                        Console.WriteLine($"          X={point.X}, Y={point.Y}");
                    }
                }

                Console.WriteLine($"        Border Color: {m.BorderColor}");
                Console.WriteLine($"        Border Thickness: {m.BorderThickness}");
                break;

            case TextBlockMapObject m:
                Console.WriteLine($"      TextBlock Properties:");
                Console.WriteLine($"        Background: {m.Background}");
                Console.WriteLine($"        Border Color: {m.BorderColor}");
                Console.WriteLine($"        Border Thickness: {m.BorderThickness}");
                Console.WriteLine($"        Opacity: {m.Opacity}");
                Console.WriteLine($"        Font:");
                Console.WriteLine($"          Family: {m.FontFamily}");
                Console.WriteLine($"          Bold: {m.FontIsBold}");
                Console.WriteLine($"          Italic: {m.FontIsItalic}");
                Console.WriteLine($"          Underline: {m.FontIsUnderline}");
                Console.WriteLine($"          Size: {m.FontSize}");
                Console.WriteLine($"        Foreground: {m.Foreground}");
                Console.WriteLine($"        Text: {m.Text}");
                Console.WriteLine($"        Alignment: {m.TextAlignment}");
                break;
        }
    }

    private string FormatCoordinate(GeoCoordinate coordinate)
    {
        return coordinate == null || coordinate.IsUnknown
            ? "Unknown"
            : $"Lat={coordinate.Latitude}, Lon={coordinate.Longitude}, Alt={coordinate.Altitude}";
    }

}