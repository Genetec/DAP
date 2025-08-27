// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using Sdk.Workspace.Pages;

public class ControlsSamplePageDescriptor : PageDescriptor
{
    public override string Name => "Controls Sample";

    public override Guid Type { get; } = new("B59AEC4B-D025-468F-9D58-65B56F96380E");

    public override Guid CategoryId => CustomTaskCategories.SdkSamples;

    public override string Description => "This page provides samples of the available controls.";

    public override TaskIconColor IconColor => TaskIconColor.DefaultIconColor;

    public override bool HasPrivilege() => true;
}