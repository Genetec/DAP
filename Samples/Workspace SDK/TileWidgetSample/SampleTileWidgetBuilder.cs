// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using Sdk.Workspace.Components;
using Sdk.Workspace.Components.TileWidget;
using Sdk.Workspace.Pages.Contents;

public class SampleTileWidgetBuilder : TileWidgetBuilder
{
    public override string Name => nameof(SampleTileWidgetBuilder);

    public override Guid UniqueId { get; } = new("AAF2CC2A-A731-4882-98C1-87DF91AC6734");

    public override TileWidget CreateView()
    {
        return new SampleTileWidget();
    }

    public override bool IsSupported(TilePluginContext context)
    {
        return context.Content is EntityContent || context.ContentGroup is EntityContentGroup;
    }

    public override string Title => "Sample Tile Widget";
}