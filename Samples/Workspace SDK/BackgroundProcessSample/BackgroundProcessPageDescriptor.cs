// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

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