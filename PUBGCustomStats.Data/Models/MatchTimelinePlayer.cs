using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUBGCustomStats.Data.Models
{
    public class MatchTimelinePlayer
    {
        public Guid MatchTimelineGuid { get; set; }
        public Guid MatchTimelinePlayerGuid { get; set; }
        public Guid? PlayerGuid { get; set; } // PlayerGuid of the victim (if applicable)
        public string? PlayerAccountId { get; set; } // Account ID of the victim (if applicable)
        public bool? PlayerIsNPC { get; set; }
        

        public virtual MatchTimeline? MatchTimeline { get; set; } // Navigation property to the match
        public virtual Player? Player { get; set; } // Navigation property to the victim player
    }
}
