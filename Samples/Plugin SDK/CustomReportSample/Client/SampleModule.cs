﻿// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples.Client;

using Genetec.Sdk;
using Genetec.Sdk.Workspace.Modules;
using Genetec.Sdk.Workspace.Tasks;

/// <summary>
/// Client module for the custom report sample.
/// </summary>
public class SampleModule : Module
{
    public override void Load()
    {
        // Register the custom report page task for Security Desk only
        if (Workspace.ApplicationType == ApplicationType.SecurityDesk)
        {
            var task = new CreatePageTask<CustomReportPage>();
            task.Initialize(Workspace);
            Workspace.Tasks.Register(task);
        }
    }

    public override void Unload()
    {
    }
}