// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using Sdk.Workspace.Pages;

public class SamplePageDescriptor : PageDescriptor
{
    public override string Name => "Sample page task";

    public override Guid Type { get; } = new("10B5DC08-51DF-470E-9169-8344DF69F372");

    public override string Description => "This sample illustrates a Task opening a page.";

    public override Guid CategoryId { get; } = Guid.Parse(Sdk.Workspace.Tasks.TaskCategories.Operation);

    public override TaskIconColor IconColor => TaskIconColor.DefaultIconColor;
        
    public override bool HasPrivilege()
    {
        // Check if the user has the privilege to access the page.
        return true;
    }
}