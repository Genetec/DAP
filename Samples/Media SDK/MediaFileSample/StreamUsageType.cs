// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;

namespace Genetec.Dap.CodeSamples;

public static class StreamUsageType
{
    public static readonly Guid Live = new("e30e6525-2202-4502-8101-5a75ee15f04b");
    public static readonly Guid Archiving = new("bac8d8bf-ee2a-41ac-b62c-3c2dd00020a5");
    public static readonly Guid Export = new("677ac8df-09c1-4cde-94fd-ad7146533802");
    public static readonly Guid HighRes = new("cf7bfe49-9df9-450b-ac83-12025f70b0d5");
    public static readonly Guid LowRes = new("3403c910-32b3-4f9a-b553-c7dc689a78bf");
    public static readonly Guid Remote = new("98bf3d2a-259c-4902-9f8c-5c15ccc57252");
    public static readonly Guid EdgePlayback = new("8ee4b8e0-8ce2-4552-89d7-60c2b9324c5e");
}