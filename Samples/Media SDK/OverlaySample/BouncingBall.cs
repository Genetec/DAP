// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System.Windows;
using System.Windows.Media;
using Sdk.Media.Overlay;

public class BouncingBall
{
    public double CanvasWidth = 1290;

    public double CanvasHeight = 100;

    private double m_x;

    private double m_y;

    private readonly double m_radius;

    private double m_horizontalVelocity;

    private double m_verticalVelocity;

    private readonly SolidColorBrush m_brush;

    private readonly Pen m_pen;

    public BouncingBall(double x, double y, double horizontalVelocity, double verticalVelocity, double radius)
    {
        m_x = x;
        m_y = y;
        m_horizontalVelocity = horizontalVelocity;
        m_verticalVelocity = verticalVelocity;
        m_radius = radius;

        m_brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Red"));
        m_brush.Freeze();

        m_pen = new Pen(m_brush, 1.0);
        m_pen.Freeze();
    }

    public void Draw(Layer layer)
    {
        Update();
        layer.DrawEllipse(m_brush, m_pen, new Point(m_x, m_y), m_radius, m_radius);
    }

    private void Update()
    {
        // Update position using basic physics equations
        m_x += m_horizontalVelocity * 0.5f;
        m_y += m_verticalVelocity * 0.5f;

        // Check if ball has collided with the left or right boundary
        if (m_x - m_radius < 0)
        {
            m_x = m_radius;
            m_horizontalVelocity = -m_horizontalVelocity;
        }
        else if (m_x + m_radius > CanvasWidth)
        {
            m_x = CanvasWidth - m_radius;
            m_horizontalVelocity = -m_horizontalVelocity;
        }

        // Check if ball has collided with the top or bottom boundary
        if (m_y - m_radius < 0)
        {
            m_y = m_radius;
            m_verticalVelocity = -m_verticalVelocity;
        }
        else if (m_y + m_radius > CanvasHeight)
        {
            m_y = CanvasHeight - m_radius;
            m_verticalVelocity = -m_verticalVelocity;
        }
    }
}