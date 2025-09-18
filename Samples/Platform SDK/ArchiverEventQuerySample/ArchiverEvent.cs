// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using Genetec.Sdk;

namespace Genetec.Dap.CodeSamples;

public record ArchiverEvent
{
    public DateTime EventTime { get; init; }
    public Guid ArchiveSourceGuid { get; init; }
    public EventType EventType { get; init; }
}