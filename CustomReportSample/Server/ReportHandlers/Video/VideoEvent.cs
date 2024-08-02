// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers.Video
{
    using System;
    using Genetec.Sdk;

    public class VideoEvent
    {
        public Guid CameraGuid { get; set; }
        public Guid ArchiveSourceGuid { get; set; }
        public DateTime EventTime { get; set; }
        public EventType EventType { get; set; }
        public uint Value { get; set; }
        public string Notes { get; set; }
        public string XmlData { get; set; }
        public uint Capabilities { get; set; }
        public string TimeZone { get; set; }
        public byte[] Thumbnail { get; set; }
    }
}