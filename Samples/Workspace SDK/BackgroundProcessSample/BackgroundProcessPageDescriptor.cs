// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using Genetec.Sdk.Workspace.Pages;
using Genetec.Sdk.Workspace.Tasks;

public class BackgroundProcessPageDescriptor : PageDescriptor
{
    public override Guid CategoryId { get; } = new Guid(TaskCategories.Operation);

    public override string Description => "Demonstrate the IBackgroundProcessNotificationService";

    public override string Name => "Background Process Service";

    public override Guid Type { get; } = new Guid("B94C35AA-D324-4381-A05E-B3A5F24FF487"); // TODO: Replace with a new GUID

    public override bool AllowOfflineExecution => false;
}