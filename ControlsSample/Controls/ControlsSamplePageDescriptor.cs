// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples
{
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
}