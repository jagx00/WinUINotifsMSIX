using System;

namespace WinUINotifsMSIX.Models
{
    // 20220526
    public partial class MatchEventType
    {
        // See BettingContext for fluent api for this table
        public long ID { get; internal set; }
        public string? EventType { get; internal set; }
        public string? EventTypeDescription { get; internal set; }
        //public DateTime TS { get; set; }
        // leave out match pred columns for now
    }
}