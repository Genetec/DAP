namespace Genetec.Dap.AccessControl
{
    using System;
    using System.Collections.Generic;
    using Genetec.Sdk;

    public class AccessControlRawEvent
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

        public IEnumerable<Guid> GetRelatedEntities()
        {
            if (SourceGuid.HasValue)
                yield return SourceGuid.Value;

            if (AccessPointGuid.HasValue)
                yield return AccessPointGuid.Value;

            if (AccessPointGroupGuid.HasValue)
                yield return AccessPointGroupGuid.Value;

            if (CredentialGuid.HasValue)
                yield return CredentialGuid.Value;

            if (CardholderGuid.HasValue)
                yield return CardholderGuid.Value;

            if (DeviceGuid.HasValue)
                yield return DeviceGuid.Value;
        }
    }
}