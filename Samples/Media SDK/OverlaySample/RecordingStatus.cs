// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System.Windows;
using System.Windows.Media;
using Genetec.Sdk.Media.Overlay;

public class RecordingStatus
{
    private readonly Genetec.Sdk.Entities.Camera m_camera;
    private readonly SolidColorBrush m_backgroundBrush;
    private readonly SolidColorBrush m_textBrush;
    private readonly Pen m_pen;

    public RecordingStatus(Genetec.Sdk.Entities.Camera camera)
    {
        m_camera = camera;

        m_backgroundBrush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        m_backgroundBrush.Freeze();

        m_textBrush = new SolidColorBrush(Colors.Red);
        m_textBrush.Freeze();

        m_pen = new Pen(new SolidColorBrush(Colors.White), 1.0);
        m_pen.Freeze();
    }

    public void Draw(Layer layer)
    {
        if (m_camera.RecordingState is Sdk.RecordingState.Off or Sdk.RecordingState.Problem or Sdk.RecordingState.OffLocked)
        {
            // Draw warning text
            const string warningText = "🔴 NOT RECORDING";

            FormattedText text = new(
                warningText,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                32,
                m_textBrush,
                1.0);

            layer.DrawText(text, new Point(25, 25));

            var backgroundRect = new Rect(25 - 5, 25 - 5, text.Width + 10, text.Height + 10);
            layer.DrawRectangle(m_backgroundBrush, m_pen, backgroundRect);
        }
        else
        {
            layer.Clear();
        }
    }
}