// Copyright (C) 2024 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using Sdk.Workspace.Pages;

    public class SamplePageDescriptor : PageDescriptor
    {
        public override string Name => "Web Browser";

        public override Guid Type { get; } = new Guid("10B5DC08-51DF-470E-9169-8344DF69F372");

        public override string Description => "This page provides an example of how to use the web browser control.";

        public override Guid CategoryId { get; } = Guid.Parse(Sdk.Workspace.Tasks.TaskCategories.Operation);

        public override TaskIconColor IconColor => TaskIconColor.DefaultIconColor;
        
        public override bool HasPrivilege()
        {
            return true;
        }
    }
}