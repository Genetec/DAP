// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Genetec.Sdk.Controls.Charts;

namespace Genetec.Dap.CodeSamples;

public class ChartSampleViewModel : BindableBase
{
    private readonly Dictionary<string, ChartSeries> m_chartData;
    private readonly string[] m_labels = { "Label 1", "Label 2", "Label 3" };
    private Chart m_chart;
    private bool m_disableAnimations;
    private bool m_performanceMode;
    private ChartType m_selectedChartType = ChartType.Bar;
    private bool m_showChartGrid = true;
    private bool m_showChartValue = true;
    private bool m_showLegend = true;
    private Color m_strokeColor = Colors.Black;

    public ChartSampleViewModel()
    {
        m_chartData = InitializeChartData();
        CreateChart();
    }

    public Chart Chart
    {
        get => m_chart;
        private set => SetProperty(ref m_chart, value);
    }

    public IEnumerable<ChartType> ChartTypes => Enum.GetValues(typeof(ChartType)).Cast<ChartType>();

    public ChartType SelectedChartType
    {
        get => m_selectedChartType;
        set
        {
            if (SetProperty(ref m_selectedChartType, value))
            {
                CreateChart();
            }
        }
    }

    public bool DisableAnimations
    {
        get => m_disableAnimations;
        set
        {
            if (SetProperty(ref m_disableAnimations, value) && Chart != null)
            {
                Chart.DisableAnimations = value;
            }
        }
    }

    public bool PerformanceMode
    {
        get => m_performanceMode;
        set
        {
            if (SetProperty(ref m_performanceMode, value) && Chart != null)
            {
                Chart.PerformanceMode = value;
            }
        }
    }

    public bool ShowChartGrid
    {
        get => m_showChartGrid;
        set
        {
            if (SetProperty(ref m_showChartGrid, value) && Chart != null)
            {
                Chart.ShowChartGrid = value;
            }
        }
    }

    public bool ShowChartValue
    {
        get => m_showChartValue;
        set
        {
            if (SetProperty(ref m_showChartValue, value) && Chart != null)
            {
                Chart.ShowChartValue = value;
            }
        }
    }

    public bool ShowLegend
    {
        get => m_showLegend;
        set
        {
            if (SetProperty(ref m_showLegend, value) && Chart != null)
            {
                Chart.ShowLegend = value;
            }
        }
    }

    public Color StrokeColor
    {
        get => m_strokeColor;
        set
        {
            if (SetProperty(ref m_strokeColor, value) && Chart != null)
            {
                Chart.Stroke = new SolidColorBrush(value);
            }
        }
    }

    private Dictionary<string, ChartSeries> InitializeChartData()
    {
        var random = new Random();
        return new Dictionary<string, ChartSeries>
        {
            { "Series 1", new ChartSeries(new double[] { random.Next(1, 100), random.Next(1, 100), random.Next(1, 100) }, Brushes.Blue) },
            { "Series 2", new ChartSeries(new double[] { random.Next(1, 100), random.Next(1, 100), random.Next(1, 100) }, Brushes.Red) }
        };
    }

    private void CreateChart()
    {
        Chart = ChartFactory.CreateChart(SelectedChartType, m_labels, m_chartData);
        Chart.DisableAnimations = DisableAnimations;
        Chart.PerformanceMode = PerformanceMode;
        Chart.ShowChartGrid = ShowChartGrid;
        Chart.ShowChartValue = ShowChartValue;
        Chart.ShowLegend = ShowLegend;
        Chart.Stroke = new SolidColorBrush(StrokeColor);
    }
}