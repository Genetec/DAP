// Copyright (C) 2024 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System.Windows;
    using System.Windows.Controls;

    public partial class PageView : UserControl
    {
        public PageView()
        {
            InitializeComponent();
        }

        private void Browser_OnLoaded(object sender, RoutedEventArgs e)
        {
            Browser.Navigate("https://www.genetec.com");
        }
    }
}
