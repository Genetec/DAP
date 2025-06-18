// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

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

            View = new BackgroundProcessPageView(new BackgroundProcessPageViewModel(service));
        }
    }
}
