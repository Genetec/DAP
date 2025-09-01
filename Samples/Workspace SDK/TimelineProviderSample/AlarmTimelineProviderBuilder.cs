// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using Sdk.Workspace.Components.TimelineProvider;

public class AlarmTimelineProviderBuilder : TimelineProviderBuilder
{
    public override string Name => nameof(AlarmTimelineProviderBuilder);

    public override string Title => "Active Alarms";

    public override Guid UniqueId { get; } = new Guid("4765D714-2BD6-42A8-99E3-0A0767C76321");

    public override TimelineProvider CreateProvider()
    {
        return new AlarmTimelineProvider(Workspace);
    }
}