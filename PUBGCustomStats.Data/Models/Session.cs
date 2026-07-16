using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUBGCustomStats.Data.Models
{
    public class Session
    {
        public Guid SessionGuid { get; set; } // Unique identifier for the session
        public string? SessionName { get; set; } // Name of the session
        public DateTime? StartDateTime { get; set; } // Start date of the session
        public List<Match> Matches { get; set; } = new List<Match>(); // List of matches in the session
        public Guid SeasonGuid { get; set; } // Foreign key to the season this session belongs to
        public virtual Season? Season { get; set; } // Navigation property to the season
    }
}
