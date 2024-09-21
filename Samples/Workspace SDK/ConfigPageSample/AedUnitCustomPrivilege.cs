// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using Sdk;

// TODO: Update the GUIDs in this class with your own SDK privilege IDs
static class AedUnitCustomPrivilege
{
    public static SdkPrivilege View { get; } = new(new Guid("0896B350-7225-4A53-8DDC-E8365CAE456E"));

    public static SdkPrivilege Modify { get; } = new(new Guid("1938B316-B651-43B7-96B1-B94C3FDBB756"));

    public static SdkPrivilege Add { get; } = new(new Guid("0707B77A-1091-4AEB-877E-30EAA80E9626"));

    public static SdkPrivilege Delete { get; } = new(new Guid("6008ECEB-024F-4D4C-BC54-0B8BC8C58D5B"));
}