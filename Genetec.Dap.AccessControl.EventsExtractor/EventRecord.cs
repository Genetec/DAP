namespace Genetec.Dap.AccessControl
{
    using System;

    public class EventRecord
    {
        public string EMPLID { get; set; }
        
        public string FullName { get; set; }
        
        public long BadgeId { get; set; }
        
        public string EventID { get; set; }
        
        public string ReaderName { get; set; }
        
        public string DoorName { get; set; }
        
        public string ProfileName { get; set; }
        
        public DateTime? EventDateLocalTime { get; set; }
        
        public DateTime? EventDateGMT { get; set; }
        
        public string EventType { get; set; }
        
        public DateTime? CreatedDate { get; set; }
        
        public string SiteName { get; set; }

        public string AccessManager { get; set; }

        public EventRecord Clone() => (EventRecord)MemberwiseClone();
    }
}
