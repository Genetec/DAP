// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

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