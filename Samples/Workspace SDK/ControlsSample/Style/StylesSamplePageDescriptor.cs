// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using Sdk.Workspace.Pages;

public class StylesSamplePageDescriptor : PageDescriptor
{
    public override string Name => "Styles Sample";

    public override Guid Type { get; } = new("99470468-048A-4511-878B-CDDDE2B18AA7");

    public override string Description => "This page provides samples of the available control styles.";

    public override Guid CategoryId => CustomTaskCategories.SdkSamples;

    public override TaskIconColor IconColor => TaskIconColor.DefaultIconColor;

    public override bool HasPrivilege() => true;
}