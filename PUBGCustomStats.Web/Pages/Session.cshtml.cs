using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PUBGCustomStats.Data;
using PUBGCustomStats.Data.Models;

namespace PUBGCustomStats.Web.Pages
{
    public class SessionModel : PageModel
    {
        public Dictionary<string, Stats> Table { get; set; } = new Dictionary<string, Stats>();
        public class Stats
        {
            public string PlayerName { get; set; }
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

        public string? Name { get; set; }
        public string StartDate { get; set; }
        public List<Match> Matches { get; set; } = new List<Match>();

        public Guid? SessionGuid { get; set; }

        public class Match
        {
            public string MatchGuid { get; set; }
            public TimeOnly? MatchLength { get; set; }
            public string Map { get; set; }
            public string GameMode { get; set; }
            public string Winner { get; set; }
            public string Perspective { get; set; }
            public string MatchName { get; set; }
            public int? MatchNumber { get; set; }
            public bool DoNotCount { get; set; } = false; // Default to false if not specified
            public DateTime? MatchStartTime { get; set; }
            public List<MatchPlayerStat> PlayerStats { get; set; } = new List<MatchPlayerStat>();

        }

        public class MatchPlayerStat
        {
            public string PlayerName { get; set; }
            public Guid PlayerGuid { get; set; }
            public string Platform { get; set; }
            public int? Rank { get; set; }
            public int? Kills { get; set; }
            public int? Assists { get; set; }
            public double? DamageDealt { get; set; }
            public int? HeadshotKills { get; set; }
            public int? Revives { get; set; }
            public int? TeamId { get; set; }
            public string PUBGPlayerId { get; set; }
            public TimeOnly? TimeSurvived { get; set; }
            public int ? DBNOs { get; set; }
            public int? Heals { get; set; }
            public int? Boosts { get; set; }
            public int? KillPlace { get; set; }
            public string? DeathType { get; set; }
            public int? DamagePlace { get; set; }
            public bool? DamagePlaceEqual { get; set; }


            public int Score { get; set; }

        }

        private readonly PUBGCustomStatsContext _context;

        public void OnGet(string? sessionGuid)
        {
            if (!string.IsNullOrEmpty(sessionGuid))
            {
                SessionGuid = Guid.Parse(sessionGuid);
            }

            PopulateModel();
        }

        public SessionModel(PUBGCustomStatsContext context)
        {
            _context = context;
        }
        
        private void PopulateModel()
        {
            // https://localhost:7093/session?sessionGuid=feecbfda-331b-44c1-ac85-125f418b2048

            // Ensure the database is created
            _context.Database.EnsureCreatedAsync();

            // Load session from the database

            
            var session = _context.Sessions
                .Where(s => s.SessionGuid == SessionGuid) // Assuming you have a foreign key relationship
                .FirstOrDefault();

            if (session != null)
            {

                Name = session.SessionName;
                StartDate = session.StartDateTime?.ToString("yyyy-MM-dd");

                // Load matches for each session
                var matches = _context.Matches
                    .Where(m => m.SessionGuid == session.SessionGuid); // Assuming you have a foreign key relationship
                    
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
                            Perspective = match.Perspective,
                            Winner = match.Winner,
                            MatchName = match.MatchNameOrDefault(),
                            MatchNumber = match.MatchNumber,
                            MatchStartTime = match.StartTime,
                            DoNotCount = match.DoNotCount ?? false,
                        };
                        Matches.Add(matchModel);

                        // Calculate stats for each match
                        
                            var matchPlayerStats = _context.MatchPlayerStats
                                .Where(mps => mps.MatchGuid == match.MatchGuid)
                                .ToList();

                            matchModel.PlayerStats = new List<MatchPlayerStat>();

                            foreach (var stat in matchPlayerStats)
                            {
                                // Exclude bots
                                if (!stat.PUBGPlayerId.StartsWith("ai."))
                                {
                                    matchModel.PlayerStats.Add(new MatchPlayerStat
                                    {
                                        PlayerName = stat.PlayerName,
                                        PlayerGuid = stat.PlayerGuid.Value,
                                        Platform = stat.Platform,
                                        Rank = stat.Rank,
                                        Kills = stat.Kills,
                                        Assists = stat.Assists,
                                        DamageDealt = stat.DamageDealt,
                                        HeadshotKills = stat.HeadshotKills,
                                        Revives = stat.Revives,
                                        TeamId = stat.TeamId,
                                        PUBGPlayerId = stat.PUBGPlayerId,
                                        TimeSurvived = stat.TimeSurvived,
                                        DBNOs = stat.DBNOs,
                                        Heals = stat.Heals,
                                        Boosts = stat.Boosts,
                                        KillPlace = stat.KillPlace,
                                        DeathType = stat.DeathType,
                                        DamagePlace = stat.DamagePlace,
                                        DamagePlaceEqual = stat.DamagePlaceEqual,
                                        Score = stat.Score.GetValueOrDefault(0)
                                    });

                                if (!match.DoNotCount.GetValueOrDefault(false))
                                {
                                    if (!Table.ContainsKey(stat.PlayerName))
                                    {
                                        Table[stat.PlayerName] = new Stats
                                        {
                                            PlayerName = stat.PlayerName,
                                            PlayerGuid = stat.PlayerGuid.Value,
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
