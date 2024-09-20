// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using Prism.Commands;
using Prism.Mvvm;
using Sdk.Controls;

public class WebBrowserSampleViewModel : BindableBase
{
    public WebBrowserSampleViewModel()
    {        
        NavigateCommand = new DelegateCommand<string>(address => WebBrowser.Navigate(address));
        ReloadCommand = new DelegateCommand(() => WebBrowser.Reload());
        GoBackCommand = new DelegateCommand(WebBrowser.GoBack, () => WebBrowser.CanGoBack);
        GoForwardCommand = new DelegateCommand(WebBrowser.GoForward, () => WebBrowser.CanGoForward);

        WebBrowser.Navigated += (_, _) =>
        {
            GoBackCommand.RaiseCanExecuteChanged();
            GoForwardCommand.RaiseCanExecuteChanged();
        };
    }

    public WebBrowser WebBrowser { get; } = new();

    public DelegateCommand ReloadCommand { get; }
        
    public DelegateCommand GoBackCommand { get; }
        
    public DelegateCommand GoForwardCommand { get; }

    public DelegateCommand<string> NavigateCommand { get; }
}