using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PUBGCustomStats.Data;

namespace PUBGCustomStats.Web.Pages
{
    public class ClanModel : PageModel
    {
        public List<Clan> Clans { get; set; } = new List<Clan>();
        public class Clan
        {
            public string? Name { get; set; }
            public string? Tag { get; set; }
            public Stats Stats { get; set; } = new Stats(); // Overall stats for the clan
            public Dictionary<string, Stats> PlayerTable { get; set; } = new Dictionary<string, Stats>();
        }
        public class Stats
        {
            public string? PlayerName { get; set; }
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

        private readonly PUBGCustomStatsContext _context;

        public ClanModel(PUBGCustomStatsContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            PopulateModel();
        }

        private void PopulateModel()
        {
            _context.Database.EnsureCreated();
            // Load seasons and sessions from the database
            var clans = _context.Clans.ToList(); // Replace 'YourEntities' with your DbSet

            foreach (var clan in clans)
            {
                var clanModel = new Clan
                {
                    Name = clan.ClanName,
                    Tag = clan.ClanTag,
                };

                // Load players for the clan
                var players = _context.Players.Where(p => p.ClanGuid == clan.ClanGuid).ToList();
                //var players = clan.Players;

                foreach (var player in players)
                {
                    if (player.PUBGPlayerId != null)
                    {
                        var stats = new Stats
                        {
                            PlayerName = player.PlayerName,
                            MatchesPlayed = 0, // Initialize to 0, will be updated later
                            Wins = 0, // Initialize to 0, will be updated later
                            Kills = 0, // Initialize to 0, will be updated later
                            HeadshotKills = 0, // Initialize to 0, will be updated later
                            Assists = 0, // Initialize to 0, will be updated later
                            DamageDealt = 0.0, // Initialize to 0.0, will be updated later
                            Revives = 0, // Initialize to 0, will be updated later
                            TimeSurvived = new TimeOnly(), // Initialize to zero time
                            DBNOs = 0,
                            Heals = 0,
                            Boosts = 0,
                            KillPlace = 0,
                            KillStreaks = 0,
                            DeathByPlayer = 0,
                            DeathByZone = 0,
                            DeathBySuicide = 0,
                            Score = 0
                        };
                        clanModel.PlayerTable[player.PUBGPlayerId] = stats;

                        var sa = _context.Players.Where(p => p.ClanGuid == clan.ClanGuid).ToList();

                        var matchPlayerStats = _context.MatchPlayerStats
                                        .Where(mps => mps.PlayerGuid == player.PlayerGuid)
                                        .ToList();
                        foreach (var matchStats in matchPlayerStats)
                        {
                            var match = _context.Matches.FirstOrDefault(m => m.MatchGuid == matchStats.MatchGuid);
                            if (match != null)
                            {
                                if (!match.DoNotCount.GetValueOrDefault(false))
                                {
                                    stats.MatchesPlayed++;
                                    if (matchStats.Rank == 1)
                                    {
                                        stats.Wins++;
                                    }
                                    stats.Kills += matchStats.Kills ?? 0;
                                    stats.HeadshotKills += matchStats.HeadshotKills ?? 0;
                                    stats.Assists += matchStats.Assists ?? 0;
                                    stats.DamageDealt += matchStats.DamageDealt ?? 0.0;
                                    stats.Revives += matchStats.Revives ?? 0;
                                    stats.DBNOs += matchStats.DBNOs ?? 0;
                                    stats.Heals += matchStats.Heals ?? 0;
                                    stats.Boosts += matchStats.Boosts ?? 0;
                                    stats.KillPlace += matchStats.KillPlace ?? 0;
                                    stats.KillStreaks += matchStats.KillStreaks ?? 0;
                                    switch (matchStats.DeathType)
                                    {
                                        case "byplayer":
                                            stats.DeathByPlayer++;
                                            break;
                                        case "byzone":
                                            stats.DeathByZone++;
                                            break;
                                        case "bysuicide":
                                        case "suicide":
                                            stats.DeathBySuicide++;
                                            break;
                                    }
                                    stats.Score += matchStats.Score ?? 0;
                                }
                            }
                        }

                        clanModel.Stats.MatchesPlayed += stats.MatchesPlayed;
                        clanModel.Stats.Wins += stats.Wins;
                        clanModel.Stats.Kills += stats.Kills;
                        clanModel.Stats.HeadshotKills += stats.HeadshotKills;
                        clanModel.Stats.Assists += stats.Assists;
                        clanModel.Stats.DamageDealt += stats.DamageDealt;
                        clanModel.Stats.Revives += stats.Revives;
                        clanModel.Stats.DBNOs += stats.DBNOs;
                        clanModel.Stats.Heals += stats.Heals;
                        clanModel.Stats.Boosts += stats.Boosts;
                        clanModel.Stats.KillPlace += stats.KillPlace;
                        clanModel.Stats.KillStreaks += stats.KillStreaks;
                        clanModel.Stats.DeathByPlayer += stats.DeathByPlayer;
                        clanModel.Stats.DeathByZone += stats.DeathByZone;
                        clanModel.Stats.DeathBySuicide += stats.DeathBySuicide;
                        clanModel.Stats.Score += stats.Score;
                    }
                }

                Clans.Add(clanModel);
            }

        }
    }
}
