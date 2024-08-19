// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.


namespace Genetec.Dap.CodeSamples.Server.ReportHandlers.AccessControl;

using System;
using Genetec.Sdk;

public class AccessControlReportRecord
{
    public DateTime Timestamp { get; set; }
    public EventType EventType { get; set; }
    public Guid UnitGuid { get; set; }
    public Guid DeviceGuid { get; set; }
    public Guid APGuid { get; set; }
    public Guid SourceGuid { get; set; }
    public Guid CredentialGuid { get; set; }
    public Guid CardholderGuid { get; set; }
    public Guid Credential2Guid { get; set; }
    public string TimeZone { get; set; }
    public int OccurrencePeriod { get; set; }
    public Guid AccessPointGroupGuid { get; set; }
    public string CustomEventMessage { get; set; }
}