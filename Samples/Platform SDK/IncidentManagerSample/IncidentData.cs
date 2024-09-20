// Copyright 2024 Genetec
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.ObjectModel;
using Sdk;

public class IncidentData
{
    public Guid InstanceGuid { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Title { get; set; }
    public string Notes { get; set; }
    public int AlarmInstance { get; set; }
    public string Category { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreationTime { get; set; }
    public string Data { get; set; }
    public string AttachedData { get; set; }
    public DateTime Timestamp { get; set; }
    public EventType Event { get; set; }
    public Guid LastModifiedBy { get; set; }
    public Collection<Guid> References { get; set; }
}