using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PUBGCustomStats.Data;
using System.Data;

namespace PUBGCustomStats.Web.Pages
{
    public class PlayerModel : PageModel
    {
        private readonly PUBGCustomStatsContext _context;
        public Guid? PlayerGuid { get; set; }
        public string? PlayerName { get; set; }
        public List<Match>? MatchList { get; set; }
        //public PlayerStats? Stats { get; private set; }
        public List<PlayerStats>? PlayerKills { get; set; }
        public List<PlayerStats>? PlayerKilledBy { get; set; }

        public class Match
        {
            public Guid MatchGuid { get; set; }
            public TimeOnly? MatchLength { get; set; }
            public string? Map { get; set; }
            public string? GameMode { get; set; }
            public string? Winner { get; set; }
            public string? MatchName { get; set; }
            public bool DoNotCount { get; set; } = false; // Default to false if not specified
            public DateTime? MatchStartTime { get; set; }
            // public List<MatchPlayerStat> PlayerStats { get; set; } = new List<MatchPlayerStat>();
            public int? Rank { get; set; }
            public Guid? SessionGuid { get; set; }
            public Guid? SeasonGuid { get; set; }
        }


        public void OnGet(Guid playerGuid)
        {
            PlayerGuid = playerGuid;
            //Stats = PlayerStatsService.GetStats(playerName);
            PopulateModel();
        }

        public PlayerModel(PUBGCustomStatsContext context)
        {
            _context = context;
        }

        private void PopulateModel()
        {
            // https://localhost:7093/session?sessionGuid=feecbfda-331b-44c1-ac85-125f418b2048

            // Ensure the database is created
            _context.Database.EnsureCreatedAsync();

            // Load session from the database


            var player = _context.Players
                .Where(s => s.PlayerGuid == PlayerGuid) // Assuming you have a foreign key relationship
                .Include(m => m.MatchPlayerStats)
                //.Include(mk => mk.MatchTimelinesAsKiller)//.ThenInclude(t => t.SecondaryPlayer)
                //  .Include(m => m.MatchTimelines)
                .FirstOrDefault();

            if (player != null)
            {
                var matchTimelines = _context.MatchTimeline
                    .Where(mt => mt.PlayerGuid == player.PlayerGuid && (mt.EventType == "LogPlayerMakeGroggy" || mt.EventType == "LogPlayerKillV2"))
                    .Include(mt => mt.SecondaryPlayer)
                    .ToList();


                var matchTimelinesKiller = _context.MatchTimeline
                    .Where(mt => mt.SecondaryPlayerGuid == player.PlayerGuid && (mt.EventType == "LogPlayerMakeGroggy" || mt.EventType == "LogPlayerKillV2"))
                    .Include(mt => mt.Player)
                    .ToList();

                PlayerName = player.PlayerName;

                MatchList = new List<Match>();
                if (player.MatchPlayerStats != null)
                {
                    foreach (var matchPlayerStat in player.MatchPlayerStats)
                    {
                        var match = _context.Matches
                            .Where(m => m.MatchGuid == matchPlayerStat.MatchGuid) // Assuming you have a foreign key relationship
                            .FirstOrDefault();
                        if (match != null)
                        {
                            var matchModel = new Match
                            {
                                MatchGuid = match.MatchGuid,
                                MatchLength = match.MatchLength,
                                Map = match.Map,
                                GameMode = match.GameMode,
                                Winner = match.Winner,
                                MatchName = match.MatchNameOrDefault(),
                                DoNotCount = match.DoNotCount.GetValueOrDefault(),
                                MatchStartTime = match.StartTime,
                                Rank = matchPlayerStat.Rank,
                                SessionGuid = match.SessionGuid,
                                SeasonGuid = match.Session?.SeasonGuid
                            };
                            MatchList.Add(matchModel);
                        }
                    }
                }
                var matchTimelinesAsPlayer = new Dictionary<string, PlayerStats>();
                var matchTimelinesAsKiller = new Dictionary<string, PlayerStats>();

                foreach (var timeline in matchTimelines)
                {
                    var playerId = timeline.SecondaryPlayerAccountId ?? "Unknown";
                    var playerName = timeline.SecondaryPlayer?.PlayerName ?? "Unknown";

                    if (timeline.SecondaryPlayerIsNPC.GetValueOrDefault() && timeline.SecondaryPlayerAccountId != null)
                    {
                        if (timeline.SecondaryPlayerAccountId.StartsWith("ai"))
                        {
                            playerId = "BOT";
                            playerName = "BOT";
                        }
                        if (timeline.SecondaryPlayerAccountId.StartsWith("Guard"))
                        {
                            playerId = "GUARD";
                            playerName = "GUARD";
                        }
                        if (timeline.SecondaryPlayerAccountId.StartsWith("Commander"))
                        {
                            playerId = "Commander";
                            playerName = "Commander";
                        }
                        if (timeline.SecondaryPlayerAccountId.StartsWith("Monster.Bear"))
                        {
                            playerId = "Bear";
                            playerName = "Bear";
                        }
                    }

                    if (string.IsNullOrEmpty(timeline.SecondaryPlayerAccountId))
                    {
                        if (timeline.IsSuicide.GetValueOrDefault())
                        {
                            playerId = "Suicide";
                            playerName = "Suicide";
                        }
                        else
                        {
                            switch (timeline.DamageCategory)
                            {
                                case "Damage_BlueZone":
                                    playerId = "Blue Zone";
                                    playerName = "Blue Zone";
                                    break;

                                case "Damage_Drown":
                                    playerId = "Drowning";
                                    playerName = "Drowning";
                                    break;

                                case "Damage_Explosion_RedZone":
                                    playerId = "Red Zone";
                                    playerName = "Red Zone";
                                    break;

                                case "Damage_Explosion_JerryCan":
                                    playerId = "Jerry Can";
                                    playerName = "Jerry Can";
                                    break;

                                case "Damage_Explosion_BlackZone":
                                    playerId = "Black Zone";
                                    playerName = "Black Zone";
                                    break;

                                case "Damage_HelicopterHit":
                                    playerId = "Helicopter Hit";
                                    playerName = "Helicopter Hit";
                                    break;

                                case "Damage_Groggy":
                                    playerId = "Bleed";
                                    playerName = "Bleeding Out";
                                    break;

                                case "Damage_Punch":
                                case "Damage_Gun":
                                    // Need to invesiage
                                    break;

                                case "Damage_VehicleHit":
                                    playerId = "Vehicle Hit";
                                    playerName = "Vehicle Hit";
                                    break;

                                default:
                                    break;
                            }
                        }
                    }

                    if (!matchTimelinesAsPlayer.ContainsKey(playerId))
                    {
                        matchTimelinesAsPlayer.Add(playerId, new PlayerStats
                        {
                            PlayerName = playerName,
                            KillCount = 0,
                            KnockCount = 0
                        });
                    }

                    if (timeline.Match != null)
                    {
                        if (timeline.EventType == "LogPlayerMakeGroggy")
                        {
                            if (timeline.Match.DoNotCount.GetValueOrDefault())
                            {
                                matchTimelinesAsPlayer[playerId].KnockCountOther++;
                            }
                            else
                            {
                                matchTimelinesAsPlayer[playerId].KnockCount++;
                            }
                        }
                        else if (timeline.EventType == "LogPlayerKillV2")
                        {
                            if (timeline.Match.DoNotCount.GetValueOrDefault())
                            {
                                matchTimelinesAsPlayer[playerId].KillCountOther++;
                            }
                            else
                            {
                                matchTimelinesAsPlayer[playerId].KillCount++;
                            }
                        }
                    }
                }

                foreach (var timeline in matchTimelinesKiller)
                {

                    if (!string.IsNullOrEmpty(timeline.PlayerAccountId))
                    {
                        var playerId = timeline.PlayerAccountId;
                        var playerName = timeline.Player?.PlayerName ?? "Unknown";

                        if (timeline.PlayerIsNPC.GetValueOrDefault())
                        {
                            if (timeline.PlayerAccountId.StartsWith("ai"))
                            {
                                playerId = "BOT";
                                playerName = "BOT";
                            }
                            if (timeline.PlayerAccountId.StartsWith("Guard"))
                            {
                                playerId = "GUARD";
                                playerName = "GUARD";
                            }
                            if (timeline.PlayerAccountId.StartsWith("Commander"))
                            {
                                playerId = "Commander";
                                playerName = "Commander";
                            }
                            if (timeline.PlayerAccountId.StartsWith("Monster.Bear"))
                            {
                                playerId = "Bear";
                                playerName = "Bear";
                            }
                        }

                        if (!matchTimelinesAsKiller.ContainsKey(playerId))
                        {
                            matchTimelinesAsKiller.Add(playerId, new PlayerStats
                            {
                                PlayerName = playerName,
                                KillCount = 0,
                                KnockCount = 0
                            });
                        }

                        if (timeline.EventType == "LogPlayerMakeGroggy")
                        {
                            if (timeline.Match != null)
                            {
                                if (timeline.Match.DoNotCount.GetValueOrDefault())
                                {
                                    matchTimelinesAsKiller[playerId].KnockCountOther++;

                                }
                                else
                                {
                                    matchTimelinesAsKiller[playerId].KnockCount++;
                                }
                            }                        }
                        else if (timeline.EventType == "LogPlayerKillV2")
                        {
                            if (timeline.Match != null)
                            {
                                if (timeline.Match.DoNotCount.GetValueOrDefault())
                                {
                                    matchTimelinesAsKiller[playerId].KillCountOther++;
                                }
                                else
                                {
                                    matchTimelinesAsKiller[playerId].KillCount++;
                                }
                            }
                        }
                    }
                }

                PlayerKills = matchTimelinesAsKiller.Values.OrderByDescending(ps => ps.KillCount).ThenByDescending(ps => ps.KnockCount).ThenByDescending(ps => ps.KillCountOther).ThenByDescending(ps => ps.KnockCountOther).ToList();
                PlayerKilledBy = matchTimelinesAsPlayer.Values.OrderByDescending(ps => ps.KillCount).ThenByDescending(ps => ps.KnockCount).ThenByDescending(ps => ps.KillCountOther).ThenByDescending(ps => ps.KnockCountOther).ToList();
            }
        }




        public class PlayerStats
        {
            public string PlayerName { get; set; } = string.Empty;
            public int KillCount { get; set; }
            public int KnockCount { get; set; }

            public int KillCountOther { get; set; }
            public int KnockCountOther { get; set; }
            /*   public int MatchesPlayed { get; set; }
               public int Wins { get; set; }
               public int Kills { get; set; }*/
        }



    }
}
