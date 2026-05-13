// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using Sdk;
using Sdk.Workspace.Components.TimelineProvider;
using System;
using System.Windows;
using System.Windows.Media;

public class AccessTimelineEvent : TimelineEvent
{
    private static readonly Size s_size = new Size(16, 16);

    private static readonly ImageSource s_grantedImage;
    private static readonly ImageSource s_refusedImage;

    private readonly bool m_isGranted;

    static AccessTimelineEvent()
    {
        s_grantedImage = EventType.AccessGranted.GetIcon();
        s_refusedImage = EventType.AccessRefused.GetIcon();
    }

    public AccessTimelineEvent(Guid cardholderGuid, DateTime timestamp, bool isGranted) : base(timestamp)
    {
        CardholderGuid = cardholderGuid;
        m_isGranted = isGranted;
    }

    public Guid CardholderGuid { get; }

    public override TimelineVisual GetVisual(Rect constraint, double msPerPixel)
    {
        var drawingVisual = new DrawingVisual();

        DrawingContext drawingContext = drawingVisual.RenderOpen();

        ImageSource image = m_isGranted ? s_grantedImage : s_refusedImage;
        drawingContext.DrawImage(image, new Rect(new Point(constraint.X, (constraint.Height - 16) / 2), s_size));

        drawingContext.Close();

        return new TimelineVisual(drawingVisual)
        {
            AlignmentY = AlignmentY.Center,
            AlignmentX = AlignmentX.Center
        };
    }
}
