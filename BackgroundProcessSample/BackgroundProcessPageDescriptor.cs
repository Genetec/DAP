// Copyright (C) 2022 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using Genetec.Sdk.Workspace.Pages;
    using Genetec.Sdk.Workspace.Tasks;

    public class BackgroundProcessPageDescriptor : PageDescriptor
    {
        public override Guid CategoryId { get; } = new Guid(TaskCategories.Operation);

        public override string Description => "This sample uses BackgroundProcessNotificationService to show custom processes advancements.";

        public override string Name => "Background Process Service";

        public override Guid Type { get; } = new Guid("B94C35AA-D324-4381-A05E-B3A5F24FF487");

        public override bool AllowOfflineExecution => false;
    }
}