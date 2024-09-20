// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;

namespace Genetec.Dap.CodeSamples;

public static class StreamMediaType
{
    public static readonly Guid Legacy = new("E5FDCEF3-0A63-4615-A2A2-BB34C892FB97");
    public static readonly Guid Video = new("4A0425E9-5F15-4675-83EB-2777CE199CB3");
    public static readonly Guid AudioIn = new("5FFBA43D-649A-473b-955A-E1EE5581A2E3");
    public static readonly Guid AudioOut = new("58ED2FFD-E3DC-437e-B25F-BEACF9BBCEC7");
    public static readonly Guid Metadata = new("7D06EB54-F1DE-4b29-BCAE-0E5A102CDBEE");
    public static readonly Guid Ptz = new("00000000-0000-0000-0000-111111111111");
    public static readonly Guid OverlayUpdate = new("90E7162D-7D5E-4D3E-9780-110A9570C821");
    public static readonly Guid OverlayStream = new("D6882AC2-C02D-4EFF-9217-9397EBAFB3E4");
    public static readonly Guid CollectionEvents = new("CEDF3497-0A63-4615-A2A2-BB34C892FB97");
    public static readonly Guid ArchiverEvents = new("BADDECAF-FACE-DEAF-ACED-7294DEED9863");
}