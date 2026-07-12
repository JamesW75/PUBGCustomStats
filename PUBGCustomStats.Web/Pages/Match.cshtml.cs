using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PUBGCustomStats.Data;

namespace PUBGCustomStats.Web.Pages
{
    public class MatchModel : PageModel
    {
        public Guid MatchGuid { get; set; }
        public Data.Models.Match? Match { get; set; }
        public List<Data.Models.MatchPlayerStat>? MatchPlayerStats { get; set; }
        public List<Data.Models.MatchTimeline>? MatchTimeline { get; set; }

        private readonly PUBGCustomStatsContext _context;

        public void OnGet(string? matchGuid)
        {
            if (!string.IsNullOrEmpty(matchGuid))
            {
                MatchGuid = Guid.Parse(matchGuid);
            }

            PopulateModel();
        }
        public MatchModel(PUBGCustomStatsContext context)
        {
            _context = context;
        }
        private void PopulateModel()
        {
            // Ensure the database is created
            _context.Database.EnsureCreatedAsync();
            Match = _context.Matches

                            .Where(m => m.MatchGuid == MatchGuid)
                            .FirstOrDefault();

            if (Match != null)
            {
                MatchPlayerStats = _context.MatchPlayerStats
                    .Where(mps => mps.MatchGuid == Match.MatchGuid)
                    .OrderByDescending(mps => mps.Kills)
                    .ThenByDescending(mps => mps.DamageDealt)
                    .ToList();

                MatchTimeline = _context.MatchTimeline.Include(mt => mt.Player).Include(mt => mt.SecondaryPlayer)
                    .Include(mt => mt.MatchTimelinePlayers)
                    .Where(mt => mt.MatchGuid == Match.MatchGuid)
                    .OrderBy(mt => mt.EventTimestamp)
                    .ToList();
            }
        }
    }
}

