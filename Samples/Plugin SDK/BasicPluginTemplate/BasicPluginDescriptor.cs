// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using Sdk.Plugin;

public class BasicPluginDescriptor : PluginDescriptor
{
    public override string Description => "Basic plugin's description";

    public override string Name => "Basic Plugin";

    // TODO: Replace with your own unique plugin GUID
    public override Guid PluginGuid => new Guid("84E87A98-8AD3-4853-85E3-05C86B9BF90C");

    public override string SpecificDefaultConfig => null;

    public override bool IsSingleInstance => true;

    public override List<string> ApplicationId => new List<string>
    {
        "KxsD11z743Hf5Gq9mv3+5ekxzemlCiUXkTFY5ba1NOGcLCmGstt2n0zYE9NsNimv", // Allow the plugin to run on a demo system
        //TODO: Add your SDK certificate application ID
    };
}