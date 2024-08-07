// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Genetec.Sdk.Controls.Charts;

namespace Genetec.Dap.CodeSamples
{
    public class ChartSampleViewModel : BindableBase
    {
        private readonly Dictionary<string, ChartSeries> m_chartData;
        private readonly string[] m_labels = { "Label 1", "Label 2", "Label 3" };
        private Chart m_currentChart;
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

        public Chart CurrentChart
        {
            get => m_currentChart;
            private set => SetProperty(ref m_currentChart, value);
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
                if (SetProperty(ref m_disableAnimations, value) && CurrentChart != null)
                {
                    CurrentChart.DisableAnimations = value;
                }
            }
        }

        public bool PerformanceMode
        {
            get => m_performanceMode;
            set
            {
                if (SetProperty(ref m_performanceMode, value) && CurrentChart != null)
                {
                    CurrentChart.PerformanceMode = value;
                }
            }
        }

        public bool ShowChartGrid
        {
            get => m_showChartGrid;
            set
            {
                if (SetProperty(ref m_showChartGrid, value) && CurrentChart != null)
                {
                    CurrentChart.ShowChartGrid = value;
                }
            }
        }

        public bool ShowChartValue
        {
            get => m_showChartValue;
            set
            {
                if (SetProperty(ref m_showChartValue, value) && CurrentChart != null)
                {
                    CurrentChart.ShowChartValue = value;
                }
            }
        }

        public bool ShowLegend
        {
            get => m_showLegend;
            set
            {
                if (SetProperty(ref m_showLegend, value) && CurrentChart != null)
                {
                    CurrentChart.ShowLegend = value;
                }
            }
        }

        public Color StrokeColor
        {
            get => m_strokeColor;
            set
            {
                if (SetProperty(ref m_strokeColor, value) && CurrentChart != null)
                {
                    CurrentChart.Stroke = new SolidColorBrush(value);
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
            CurrentChart = ChartFactory.CreateChart(SelectedChartType, m_labels, m_chartData);
            ApplyChartProperties();
        }

        private void ApplyChartProperties()
        {
            if (CurrentChart != null)
            {
                CurrentChart.DisableAnimations = DisableAnimations;
                CurrentChart.PerformanceMode = PerformanceMode;
                CurrentChart.ShowChartGrid = ShowChartGrid;
                CurrentChart.ShowChartValue = ShowChartValue;
                CurrentChart.ShowLegend = ShowLegend;
                CurrentChart.Stroke = new SolidColorBrush(StrokeColor);
            }
        }
    }
}