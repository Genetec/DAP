// Copyright (C) 2024 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using Prism.Mvvm;
    using Sdk;

    public class AlarmInstance : BindableBase
    {
        public int Id { get; set; }

        public Guid Alarm { get; set; }

        public string Name { get; set; }

        public byte Priority { get; set; }

        public string SourceName { get; set; }

        public EntityType SourceType { get; set; }

        public EventType? TriggerEvent { get; set; }

        public AlarmState State { get; set; }

        public string Context { get; set; }

        public string AcknowledgedBy { get; set; }

        public DateTime? AcknowledgedOn { get; set; }

        public string InvestigatedBy { get; set; }

        public DateTime? InvestigatedOn { get; set; }

        public DateTime TriggerTime { get; set; }

        public OfflinePeriodType OccurencePeriod { get; set; }

        public bool HasSourceCondition { get; set; }

        protected bool Equals(AlarmInstance other) => Id == other.Id;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals((AlarmInstance)obj);
        }

        public override int GetHashCode() => Id;
    }
}