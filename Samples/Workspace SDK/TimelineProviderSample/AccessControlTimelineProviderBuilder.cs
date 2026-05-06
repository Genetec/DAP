// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using Sdk.Workspace.Components.TimelineProvider;
using System;

public class AccessControlTimelineProviderBuilder : TimelineProviderBuilder
{
    public override string Name => nameof(AccessControlTimelineProviderBuilder);

    public override string Title => "Access Control Events";

    public override Guid UniqueId { get; } = new Guid("8A3C5F12-9D4E-4B7A-B2C1-6E8F0A1D3C5B"); // Replace with your own unique GUID

    public override TimelineProvider CreateProvider()
    {
        return new AccessControlTimelineProvider(Workspace);
    }
}
