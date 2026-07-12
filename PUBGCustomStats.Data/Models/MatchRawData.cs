using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUBGCustomStats.Data.Models
{
    public class MatchRawData
    {
        public Guid MatchGuid { get; set; }
        public string? RawData { get; set; } // Store the raw JSON response for debugging or logging
        public string? TelemetryRawData { get; set; } // Store the raw telemetry data for debugging or logging
        public virtual Match? Match { get; set; } // Navigation property to the session
    }
}
