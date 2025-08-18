using System;
using Genetec.Sdk;

namespace Genetec.Dap.CodeSamples;

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