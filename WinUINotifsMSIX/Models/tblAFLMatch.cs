using System;

namespace WinUINotifsMSIX.Models
{
    // 20220524 - only read by combo box
    public partial class tblAFLMatch
    {
        // See BettingContext for fluent api for this table
        public int ID { get; internal set; }
        public int AFLMatchTypeID { get; internal set; }
        public int AFLRoundID { get; internal set; }
        public byte MatchNumber { get; internal set; }
        public int VenueID { get; internal set; }
        public string? MatchName { get; internal set; }
        public DateTime StartDateGMT { get; set; }
        public string? MatchStatus { get; internal set; }
        public DateTime TS { get; set; }
        // leave out match pred columns for now
    }
}