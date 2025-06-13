// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples
{
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
}