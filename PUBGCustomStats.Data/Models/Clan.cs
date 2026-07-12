using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUBGCustomStats.Data.Models
{
    public class Clan
    {
        public Guid ClanGuid { get; set; } // Unique identifier for the clan
        public string? ClanName { get; set; } // Name of the clan
        public string? ClanTag { get; set; } // Optional tag for the clan

        public string? RawData { get; set; } // Store the raw JSON response for debugging or logging
        public string? PUBGClanId { get; set; }

        public List<Player> Players { get; set; } = new List<Player>(); // List of players in the clan
    }
}
