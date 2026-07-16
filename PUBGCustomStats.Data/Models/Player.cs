using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUBGCustomStats.Data.Models
{
    public class Player
    {
        public Guid PlayerGuid { get; set; }
        public string? PlayerName { get; set; }
        public string? PlayerConsole { get; set; }
        public string? PlayerClan { get; set; }
        public string? PUBGPlayerId { get; set; }
        public bool? IsRandom { get; set; }
        public string? RawData { get; set; }
        public Guid? ClanGuid { get; set; }
        public virtual Clan? Clan { get; set; } // Navigation property to the player
        public List<MatchPlayerStat>? MatchPlayerStats { get; set; }
        public List<MatchTimeline>? MatchTimelines { get; set; }
        public List<MatchTimeline>? MatchTimelinesAsKiller { get; set; }
        public List<MatchTimelinePlayer>? MatchTimelinePlayers { get; set; }


    }
}
