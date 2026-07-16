using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUBGCustomStats.Data.Models
{
    public class MatchTimeline
    {
        public Guid MatchTimelineGuid { get; set; }
        public Guid MatchGuid { get; set; } // Foreign key to Match
        public string? EventType { get; set; } // Type of event (e.g., "PlayerKilled", "ItemPickedUp")
        public DateTime EventTimestamp { get; set; } // Timestamp of the event
        public Guid? PlayerGuid { get; set; } // PlayerGuid of the victim (if applicable)
        public string? PlayerAccountId { get; set; } // Account ID of the victim (if applicable)
        public bool? PlayerIsNPC { get; set; }
        public string? SecondaryPlayerAccountId { get; set; } // Account ID of the killer (if applicable)
        public Guid? SecondaryPlayerGuid { get; set; } // PlayerGuid of the killer (if applicable)
        public bool? SecondaryPlayerIsNPC { get; set; }
        //public string DamageTypeCategory { get; set; } // Type of damage (e.g., "Weapon", "Vehicle")
        public string? DamageReason { get; set; } // Reason for damage (e.g., "Headshot", "Explosion")
        public string? DamageCategory { get; set; } // Category of damage (e.g., "Physical", "Fire")    
        public string? Weapon { get; set; } // Weapon used (if applicable)
        public double? Distance { get; set; } // Distance of the kill (if applicable)
        public int? Phase { get; set; } // Phase of the match (if applicable)
        public decimal? IsGame { get; set; }
        public bool? IsSuicide { get; set; }
        public int? DBNOId { get; set; }
        public string? Vehicle { get; set; } // Vehicle used (if applicable)

        public int? Ranking { get; set; } // Player ranking at the time of the event (if applicable)
        public int? IndividualRanking { get; set; } // Individual ranking at the time of the event (if applicable)
        public string? Status { get; set; } // Status of the player (e.g., "Alive", "Dead")

        public string? Zone { get; set; } // Zone information (if applicable)
        public virtual Match? Match { get; set; } // Navigation property to the match
        public virtual Player? SecondaryPlayer { get; set; } // Navigation property to the killer player
        public virtual Player? Player { get; set; } // Navigation property to the victim player
        public List<MatchTimelinePlayer>? MatchTimelinePlayers { get; set; } // Navigation property to related MatchTimelinePlayer entries
    }
}
