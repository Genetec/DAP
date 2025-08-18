namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using Sdk;

class AlarmInstance
{
    public int InstanceId { get; set; }
    public Guid Alarm { get; set; }
    public Guid TriggerEntity { get; set; }
    public EventType TriggerEvent { get; set; }
    public DateTime TriggerTime { get; set; }
    public DateTime? AckedTime { get; set; }
    public Guid? AckBy { get; set; }
    public DateTime CreationTime { get; set; }
    public OfflinePeriodType OfflinePeriod { get; set; }
    public int AckReason { get; set; }
    public int ExternalInstanceId { get; set; }
    public Guid? InvestigatedBy { get; set; }
    public DateTime? InvestigatedTime { get; set; }
    public AlarmState State { get; set; }
    public bool HasSourceCondition { get; set; }
    public byte Priority { get; set; }
    public string DynamicContext { get; set; }
    public List<Guid> AttachedEntities { get; set; } = new();
    public List<string> DynamicUrls { get; set; } = new();
}