using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PUBGCustomStats.Data.Models
{
    public class MatchPlayerStat
    {
        public Guid MatchPlayerStatGuid { get; set; }
        public Guid MatchGuid { get; set; } // Foreign key to Match
        public Guid? PlayerGuid { get; set; } // Foreign key to Player
        public int? Rank { get; set; } // Player's rank in the match
        public int? Kills { get; set; }
        public int? Assists { get; set; }
        public double? DamageDealt { get; set; }
        public int? HeadshotKills { get; set; }
        public int? Revives { get; set; }
        public int? TeamId { get; set; } // Team ID for the player in the match
        public string? PlayerName { get; set; } // Name of the player in the match
        public string? Platform { get; set; } // Platform of the player (e.g., PC, Xbox, PlayStation)
        //public TimeOnly? MatchLength { get; set; } // Optional: Store the match length for this player
        public string? PUBGPlayerId { get; set; } // Player's PUBG ID

        public int? DBNOs { get; set; }
        public int? Heals { get; set; }
        public int? Boosts { get; set; }
        public int? KillPlace { get; set; }
        public int? KillStreaks { get; set; }
        public string? DeathType { get; set; }
        public TimeOnly? TimeSurvived { get; set; } // Time survived in the match
        public int? DamagePlace { get; set; }
        public bool? DamagePlaceEqual { get; set; } 
        public int? Score { get; set; }
        public double? LongestKill { get; set; } // Longest kill distance in meters
        public double? RideDistance { get; set; } // Distance traveled in a vehicle
        public int? RoadKills { get; set; } // Number of kills while driving a vehicle
        public double? SwimDistance { get; set; } // Distance traveled while swimming
        public int? TeamKills { get; set; } // Number of kills made by the player's team
        public double? WalkDistance { get; set; } // Distance traveled on foot
        public int? VehicleDestroys { get; set; } // Number of vehicles destroyed by the player
        public int? WeaponsAcquired { get; set; } // Number of weapons acquired by the player
        public virtual Match? Match { get; set; } // Navigation property to the match
        public virtual Player? Player { get; set; } // Navigation property to the player


    }
}
