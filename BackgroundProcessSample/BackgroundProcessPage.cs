// Copyright (C) 2022 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using Genetec.Sdk.Workspace.Pages;
    using Genetec.Sdk.Workspace.Services;

    [Page(typeof(BackgroundProcessPageDescriptor))]
    public class BackgroundProcessPage : Page
    {
        protected override void Initialize()
        {
            var service = Workspace.Services.Get<IBackgroundProcessNotificationService>();

            View = new BackgroundProcessPageView
            {
                DataContext = new BackgroundProcessPageViewModel(service)
            };
        }
    }
}