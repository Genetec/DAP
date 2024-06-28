// Copyright 2024 Genetec
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using Sdk;

    class AccessControlEvent
    {
        public DateTime Timestamp { get; set; }
        public Guid? Unit { get; set; }
        public Guid? AccessPoint { get; set; }
        public Guid? AccessPointGroup { get; set; }
        public Guid? Credential { get; set; }
        public Guid? Credential2 { get; set; }
        public Guid? Device { get; set; }
        public string CustomEventMessage { get; set; }
        public EventType EventType { get; set; }
        public Guid? Source { get; set; }
        public TimeZoneInfo TimeZone { get; set; }
        public OfflinePeriodType OccurrencePeriod { get; set; }
        public Guid? Cardholder { get; set; }
    }
}