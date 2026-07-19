using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PUBGCustomStats.Data;
using PUBGCustomStats.Data.Models;

namespace PUBGCustomStats.Web.Pages
{
    public class IndexModel : PageModel
    {
        public Dictionary<string, Stats> Table { get; set; } = new Dictionary<string, Stats>();
        public class Stats
        {
            public string? PlayerName { get; set; }
            public Guid PlayerGuid { get; set; }
            public int MatchesPlayed { get; set; }
            public int Wins { get; set; }
            public int Kills { get; set; }
            public int HeadshotKills { get; set; }
            public int Assists { get; set; }
            public double DamageDealt { get; set; }
            public int Revives { get; set; }
            public TimeOnly TimeSurvived { get; set; } // Total time survived across all matches
            public int DBNOs { get; set; }
            public int Heals { get; set; }
            public int Boosts { get; set; }
            public int KillPlace { get; set; }
            public int KillStreaks { get; set; }
            public int DeathByPlayer { get; set; }
            public int DeathByZone { get; set; }
            public int DeathBySuicide { get; set; }
            public int Score { get; set; }
        }

        public List<Season> Seasons { get; set; } = new List<Season>();

        public class Season
        {
            public string? Name { get; set; }
            public string? StartDate { get; set; }
            public string? EndDate { get; set; }

            public List<Session> Sessions { get; set; } = new List<Session>();
        }

        public class Session
        {
            public Guid SessionGuid { get; set; }
            public string? Name { get; set; }
            public string? StartDate { get; set; }
            public List<Match> Matches { get; set; } = new List<Match>();
        }

        public class Match
        {
            public string? MatchGuid { get; set; }
            public TimeOnly? MatchLength { get; set; }
            public string? Map { get; set; }
            public string? GameMode { get; set; }
            public string? Winner { get; set; }
            public string? MatchName { get; set; }
            public bool DoNotCount { get; set; } = false; // Default to false if not specified
            public DateTime? MatchStartTime { get; set; }
            public List<MatchPlayerStat> PlayerStats { get; set; } = new List<MatchPlayerStat>();
        }

        public class MatchPlayerStat
        {
            public string? PlayerName { get; set; }
            public string? Platform { get; set; }
            public int? Rank { get; set; }
            public int? Kills { get; set; }
            public int? Assists { get; set; }
            public double? DamageDealt { get; set; }
            public int? HeadshotKills { get; set; }
            public int? Revives { get; set; }
            public int? TeamId { get; set; }
            public string? PUBGPlayerId { get; set; }
            public TimeOnly? TimeSurvived { get; set; }
            public int Score { get; set; }
        }

        private readonly PUBGCustomStatsContext _context;

        public IndexModel(PUBGCustomStatsContext context)
        {
            _context = context;

            // Ensure the database is created
            _context.Database.EnsureCreated();
            // Load seasons and sessions from the database
            var seasons = _context.Seasons.ToList(); // Replace 'YourEntities' with your DbSet
            foreach (var season in seasons.Where(s => !s.DoNotCount && s.IsCurrentSeason))
            {
                var seasonModel = new Season
                {
                    Name = season.SeasonName,
                    StartDate = season.StartDate?.ToString("yyyy-MM-dd"),
                    EndDate = season.EndDate?.ToString("yyyy-MM-dd")
                };
                Seasons.Add(seasonModel);

                // Load sessions for each season
                var sessions = _context.Sessions
                    .Where(s => s.SeasonGuid == season.SeasonGuid) // Assuming you have a foreign key relationship
                    .ToList();

                foreach (var session in sessions)
                {
                    var sessionModel = new Session
                    {
                        Name = session.SessionName,
                        StartDate = session.StartDateTime?.ToString("yyyy-MM-dd"),
                        SessionGuid = session.SessionGuid
                    };
                    seasonModel.Sessions.Add(sessionModel);

                    // Load matches for each session
                    var matches = _context.Matches
                        .Where(m => m.SessionGuid == session.SessionGuid) // Assuming you have a foreign key relationship
                        .ToList();
                    // Map matches to the session model
                    foreach (var match in matches)
                    {
                        if (match.StartTime.HasValue)
                        {
                            var matchModel = new Match
                            {
                                MatchGuid = match.MatchGuid.ToString(),
                                MatchLength = match.MatchLength,
                                Map = match.Map,
                                GameMode = match.GameMode,
                                Winner = match.Winner,
                                MatchName = match.MatchNameOrDefault(),
                                MatchStartTime = match.StartTime,
                                DoNotCount = match.DoNotCount ?? false,
                            };
                            sessionModel.Matches.Add(matchModel);

                            // Calculate stats for each match
                            if (!match.DoNotCount.GetValueOrDefault(false))
                            {
                                var matchPlayerStats = _context.MatchPlayerStats
                                    .Where(mps => mps.MatchGuid == match.MatchGuid)
                                    .ToList();
                                foreach (var stat in matchPlayerStats)
                                {
                                    if (stat.PUBGPlayerId != null)
                                    {
                                        // Exclude bots
                                        if (!stat.PUBGPlayerId.StartsWith("ai.") && stat.PlayerName != null) 
                                        {
                                            if (!Table.ContainsKey(stat.PlayerName))
                                            {
                                                Table[stat.PlayerName] = new Stats
                                                {
                                                    PlayerName = stat.PlayerName,
                                                    PlayerGuid = stat.PlayerGuid.GetValueOrDefault(),
                                                    MatchesPlayed = 0,
                                                    Wins = 0,
                                                    Kills = 0,
                                                    Assists = 0,
                                                    DamageDealt = 0
                                                };
                                            }
                                            var playerStats = Table[stat.PlayerName];
                                            playerStats.MatchesPlayed++;
                                            if (stat.Rank == 1)
                                            {
                                                playerStats.Wins++;
                                            }
                                            playerStats.Kills += stat.Kills ?? 0;
                                            playerStats.Assists += stat.Assists ?? 0;
                                            playerStats.DamageDealt += stat.DamageDealt ?? 0;
                                            playerStats.HeadshotKills += stat.HeadshotKills ?? 0;
                                            playerStats.Revives += stat.Revives ?? 0;
                                            playerStats.DBNOs += stat.DBNOs ?? 0;
                                            playerStats.Heals += stat.Heals ?? 0;
                                            playerStats.Boosts += stat.Boosts ?? 0;
                                            switch (stat.DeathType)
                                            {
                                                case "byplayer":
                                                    playerStats.DeathByPlayer++;
                                                    break;
                                                case "byzone":
                                                    playerStats.DeathByZone++;
                                                    break;
                                                case "bysuicide":
                                                case "suicide":
                                                    playerStats.DeathBySuicide++;
                                                    break;
                                                default:
                                                    break;
                                            }
                                            //playerStats.TimeSurvived += stat.TimeSurvived ?? new TimeOnly(0);

                                            playerStats.Score += stat.Score.GetValueOrDefault(0);
                                        }
                                    }
                                }
                            }
                        }
                    }


                }

            }

        }
    }
}
