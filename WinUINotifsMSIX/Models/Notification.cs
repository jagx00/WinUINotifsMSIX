using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinUINotifsMSIX.Models
{
    public partial class Notification
    {
        // See BettingContext onModelCreating for fluent api
        public int Id { get; internal set; }
        public DateTime CreationTime { get; internal set; }
        public string? Source { get; internal set; }
        public string? Title { get; internal set; }
        public string? Body { get; internal set; }
        public DateTime TS { get; set; }
        // 20200617 = tinyint = -127 to 128 - but Byte is unsigned --- tbi
        public Byte Visibility { get; set; }
    }
}
