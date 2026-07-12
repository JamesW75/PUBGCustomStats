using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace PUBGCustomStats.Data.Models
{
    public class Match
    {
        public Guid MatchGuid { get; set; }
        public Guid? SessionGuid { get; set; } // Optional: Link to a session if applicable
        public string? MatchName { get; set; }
        public string? MatchDescription { get; set; }
        public int? MatchNumber { get; set; }
        public TimeOnly? MatchLength { get; set; }
        //public string? RawData { get; set; } // Store the raw JSON response for debugging or logging
        public int? NoFireUntilPhase { get; set; } // Optional: Store the no-fire phase information
        public string? TelemetryUrl { get; set; } 
        //public string? TelemetryRawData { get; set; } // Store the raw telemetry data for debugging or logging
        public DateTime? StartTime { get; set; } // Optional: Store the creation time of the match
        public string? Winner { get; set; } // Optional: Store the winner of the match
        public string? Map { get; set; } // Optional: Store the map name
        public string? MatchType { get; set; } // Optional: Store the type of match (e.g., Solo, Duo, Squad)
        public string? GameMode { get; set; } // Optional: Store the game mode (e.g., Normal, Ranked)
        public string? Perspective { get; set; } // Optional: Store the perspective (e.g., FPP, TPP)
        public bool? DoNotCount { get; set; } // Optional: Flag to indicate if the match should not be counted in stats
        public string? Weather { get; set; }
        public string? BlueZoneSettings { get; set; }
        public virtual Session? Session { get; set; } // Navigation property to the session
        public List<MatchPlayerStat>? MatchPlayerStats { get; set; } 
        public List<MatchTimeline>? MatchTimelines { get; set; }
        //public virtual MatchRawData? MatchRawData { get; set; } // Navigation property to the session

        public string MatchNameOrDefault()
        {
            if (!string.IsNullOrEmpty(MatchName))
            {
                return MatchName;
            }
            else
            {
                return "Untitled Match #" + MatchNumber;
            }
        }

    }
}
