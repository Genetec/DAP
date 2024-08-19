// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples.Server;

using System;
using System.Collections.Generic;
using Genetec.Dap.CodeSamples.Properties;
using Genetec.Sdk.Plugin;

public class SamplePluginDescriptor : PluginDescriptor
{
    // TODO: Replace with your own unique plugin ID 
    public static Guid PluginId { get; } = new("4E8BB3F7-0D41-4F4C-A430-0B6EE7478CBE");

    public override string Description => Resources.PluginDescription;

    public override string Name => Resources.PluginName;

    public override Guid PluginGuid => PluginId;

    public override string SpecificDefaultConfig => null;

    public override bool IsSingleInstance => false;

    public override List<string> ApplicationId => new()
    {
        "KxsD11z743Hf5Gq9mv3+5ekxzemlCiUXkTFY5ba1NOGcLCmGstt2n0zYE9NsNimv" // Allow the plugin to run on a development system
        //TODO: Add your production SDK certificate application ID
    };
}