using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUBGCustomStats.Data.Models
{
    public class MatchBlueZone
    {
        public Guid MatchBlueZoneGuid { get; set; }
        public Guid MatchGuid { get; set; } // Foreign key to Match
        public int BlueZonePhase { get; set; }
        public int StartDelay { get; set; }
        public int WarningDuration { get; set; }
        public int ReleaseDuration { get; set; }
        public double BlueZoneDamagePerSecond { get; set; }
        public double RadiusRate { get; set; }
        public double SpreadRatio { get; set; }
        public int LandRatio { get; set; }
        public int CircleAlgorithm { get; set; }
    }
}


