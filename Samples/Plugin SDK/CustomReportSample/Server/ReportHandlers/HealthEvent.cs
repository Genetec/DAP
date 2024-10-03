// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers;

using System;

record HealthEvent
{
    public int HealthEventId { get; set; }
    public int EventSourceTypeId { get; set; }
    public Guid SourceEntityGuid { get; set; }
    public string EventDescription { get; set; }
    public string MachineName { get; set; }
    public DateTime Timestamp { get; set; }
    public int SeverityId { get; set; }
    public int ErrorNumber { get; set; }
    public long Occurrence { get; set; }
    public Guid ObserverEntity { get; set; }
}