using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PUBGCustomStats.Data;
using PUBGCustomStats.Data.Models;

namespace PUBGCustomStats.Web.Pages.Shared.Components.MatchScores
{
    public class MatchScoresViewComponent : ViewComponent
    {
        private readonly PUBGCustomStatsContext _context;

        public MatchScoresViewComponent(PUBGCustomStatsContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid? matchGuid = null, Guid? sessionGuid = null)
        {
            await _context.Database.EnsureCreatedAsync();

            var matchesQuery = _context.Matches.AsQueryable();
            if (matchGuid.HasValue)
            {
                matchesQuery = matchesQuery.Where(match => match.MatchGuid == matchGuid.Value);
            }
            else if (sessionGuid.HasValue)
            {
                matchesQuery = matchesQuery.Where(match => match.SessionGuid == sessionGuid.Value);
            }

            var matches = await matchesQuery
                .OrderBy(match => match.StartTime)
                .ToListAsync();

            var matchGuids = matches.Select(match => match.MatchGuid).ToHashSet();
            var playerStats = await _context.MatchPlayerStats
                .Where(stat => matchGuids.Contains(stat.MatchGuid))
                .ToListAsync();

            var model = new MatchScoresViewModel
            {
                Matches = matches
                    .Select(match => new MatchScoresMatch
                    {
                        Match = match,
                        PlayerStats = playerStats
                            .Where(stat => stat.MatchGuid == match.MatchGuid)
                            .OrderBy(stat => stat.Rank)
                            .ThenByDescending(stat => stat.Score)
                            .ToList()
                    })
                    .ToList()
            };

            return View(model);
        }
    }

    public sealed class MatchScoresViewModel
    {
        public required List<MatchScoresMatch> Matches { get; init; }
    }

    public sealed class MatchScoresMatch
    {
        public required Match Match { get; init; }
        public required List<MatchPlayerStat> PlayerStats { get; init; }
    }
}
