// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples
{
    using System.Windows.Controls;
    using System.Windows.Input;

    public partial class WebBrowserSampleView
    {
        public WebBrowserSampleView()
        {
            InitializeComponent();
        }

        private void AddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox textBox = (TextBox)sender;
                Navigate(textBox.Text);
            }
        }

        private void Navigate(string address)
        {
            (DataContext as WebBrowserSampleViewModel)?.NavigateCommand.Execute(address);
        }
    }
}
