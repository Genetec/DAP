// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

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