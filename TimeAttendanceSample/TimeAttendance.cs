// // Copyright (C) 2024 by Genetec, Inc. All rights reserved.
// // May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;

    public class TimeAttendance
    {
        public DateTime Date { get; set; }
        public Guid CardholderGuid { get; set; }
        public Guid AreaGuid { get; set; }
        public DateTime FirstTimeIn { get; set; }
        public DateTime? LastExitTime { get; set; }
        public int TotalMinutes { get; set; }
        public int TotalMinutesInclusive { get; set; }
    }
}