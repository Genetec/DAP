// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using Sdk.Workspace.Pages;

public class MapControlSamplePageDescriptor : PageDescriptor
{
    public override string Name => "Map Control Sample";

    public override Guid Type { get; } = new("A7B2C8D4-E1F5-4A92-B3E6-7F8D9C2E1B4A");

    public override Guid CategoryId => CustomTaskCategories.SdkSamples;

    public override string Description => "This page provides a sample of the MapControl.";

    public override TaskIconColor IconColor => TaskIconColor.DefaultIconColor;

    public override bool AllowOfflineExecution => false;

    public override bool HasPrivilege() => true;
}