using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAGLog.Models
{
    // manually entered - based on SQL table
    // also need pk defn?? etc? indexes
    // also see OnModelCreating in BettingContext

    // Use Reverse Engineering if you want your database schema to be the source of truth. This approach allows you to scaffold a DbContext and the entity type classes by reverse engineering your database schema into an EF Core model.
    // https://docs.microsoft.com/en-us/ef/core/managing-schemas/scaffolding

    public partial class SeriLog
    {
        public int Id { get; set; }
        public string? Message { get; set; }
        public string? MessageTemplate { get; set; }
        public string? Level { get; set; }
        public DateTime? TimeStamp { get; set; }
        public string? Exception { get; set; }
        public string? Properties { get; set; }
        public string? LogEvent { get; set; }
        public string? Application { get; set; }
        public string? MethodName { get; set; }
        public string FormattedTimeStamp => TimeStamp.HasValue ? TimeStamp.Value.ToString("yyyy-MM-dd HH:mm:ss.fff") : string.Empty;
    }
}
