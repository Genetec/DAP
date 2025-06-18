// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples
{
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
}
