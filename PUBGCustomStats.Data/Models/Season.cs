using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUBGCustomStats.Data.Models
{
    public class Season
    {
        public Guid SeasonGuid { get; set; } // Unique identifier for the season
        public string? SeasonName { get; set; } // Name of the season
        public DateTime? StartDate { get; set; } // Start date of the season
        public DateTime? EndDate { get; set; } // End date of the season
        public int SeasonNumber { get; set; } // Season number (e.g., 1, 2, 3, etc.)
        public bool IsCurrentSeason { get; set; }
        public bool DoNotCount {  get; set; }
        public List<Session> Sessions { get; set; } = new List<Session>(); // List of sessions in the season
    }
}
