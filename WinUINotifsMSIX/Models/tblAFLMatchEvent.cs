using System;

namespace WinUINotifsMSIX.Models
{
    // 20220526
    public partial class tblAFLMatchEvent
    {
        // See BettingContext for fluent api for this table
        public int ID { get; internal set; }
        public long MatchEventTypeID { get; internal set; }
        public int AFLMatchID { get; internal set; }
        public DateTime EventUTC { get; set; }
        public string? Description { get; internal set; }
        public DateTime TS { get; set; }
        // leave out match pred columns for now
    }
}