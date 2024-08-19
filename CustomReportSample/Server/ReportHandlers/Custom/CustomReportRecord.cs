// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers.Custom;

using System;

public class CustomReportRecord
{
    public Guid SourceId { get; set; }

    public int EventId { get; set; }

    public string Message { get; set; }

    public DateTime EventTimestamp { get; set; }

    public decimal Decimal { get; set; }

    public bool Boolean { get; set; }

    public byte[] Picture { get; set; }

    public string Hidden { get; set; }

    public int Numeric { get; set; }

    public TimeSpan Duration { get; set; }
}