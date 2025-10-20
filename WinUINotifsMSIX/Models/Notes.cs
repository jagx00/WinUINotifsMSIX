using System;

namespace WinUINotifsMSIX.Models
{
    public partial class Notes
    {
        // See BettingContext for fluent api
        public int Id { get; internal set; }
        public string? Comments { get; internal set; }
        public DateTime TS { get; set; }
    }
}