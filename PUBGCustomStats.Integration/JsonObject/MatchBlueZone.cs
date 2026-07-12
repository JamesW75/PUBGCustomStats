using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUBGCustomStats.Integration.JsonObject
{
    public class MatchBlueZone
    {
        public int blueZonePhase { get; set; }
        public int startDelay { get; set; }
        public int warningDuration { get; set; }
        public int releaseDuration { get; set; }
        public double blueZoneDamagePerSecond { get; set; }
        public double radiusRate { get; set; }
        public double spreadRatio { get; set; }
        public int landRatio { get; set; }
        public int circleAlgorithm { get; set; }
    }
}
