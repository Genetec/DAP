// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using Sdk;

public record AlarmInstance
{
    public int InstanceId { get; init; }
    public Guid Alarm { get; init; }
    public Guid TriggerEntity { get; init; }
    public EventType TriggerEvent { get; init; }
    public DateTime TriggerTime { get; init; }
    public DateTime? AckedTime { get; init; }
    public Guid? AckBy { get; init; }
    public DateTime CreationTime { get; init; }
    public OfflinePeriodType OfflinePeriod { get; init; }
    public int AckReason { get; init; }
    public int ExternalInstanceId { get; init; }
    public Guid? InvestigatedBy { get; init; }
    public DateTime? InvestigatedTime { get; init; }
    public AlarmState State { get; init; }
    public bool HasSourceCondition { get; init; }
    public byte Priority { get; init; }
    public string DynamicContext { get; init; }
    public IReadOnlyList<Guid> AttachedEntities { get; init; } = new List<Guid>();
    public IReadOnlyList<string> DynamicUrls { get; init; } = new List<string>();
}