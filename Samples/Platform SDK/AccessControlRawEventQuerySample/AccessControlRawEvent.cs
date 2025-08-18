using System;
using Genetec.Sdk;

namespace Genetec.Dap.CodeSamples;

class AccessControlRawEvent
{
    public Guid? SourceGuid { get; set; }
    public Guid? AccessPointGuid { get; set; }
    public Guid? CredentialGuid { get; set; }
    public OfflinePeriodType OccurrencePeriod { get; set; }
    public Guid AccessManagerGuid { get; set; }
    public string TimeZone { get; set; }
    public long Position { get; set; }
    public DateTime InsertionTimestamp { get; set; }
    public DateTime Timestamp { get; set; }
    public EventType EventType { get; set; }
    public Guid? CardholderGuid { get; set; }
    public Guid? DeviceGuid { get; set; }
    public Guid? AccessPointGroupGuid { get; set; }
}