using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PUBGCustomStats.Data;
using PUBGCustomStats.Data.Models;
using System.Data;

namespace PUBGCustomStats.Web.Pages
{
    public class NonPlayerModel : PageModel
    {
        private readonly PUBGCustomStatsContext _context;
        public string PlayerName { get; set; }
        public bool DisplayKilledBy { get; set; }
        public List<PlayerStats> PlayerKills { get; set; }
        public List<PlayerStats> PlayerKilledBy { get; set; }


        public void OnGet(string name)
        {
            PlayerName = name;
            //Stats = PlayerStatsService.GetStats(playerName);
            PopulateModel();
        }

        public NonPlayerModel(PUBGCustomStatsContext context)
        {
            _context = context;
        }

        private void PopulateModel()
        {
            // https://localhost:7093/session?sessionGuid=feecbfda-331b-44c1-ac85-125f418b2048

            // Ensure the database is created
            _context.Database.EnsureCreatedAsync();

            // Load session from the database
            List<MatchTimeline> matchTimelines = null; // As Victim
            List<MatchTimeline> matchTimelinesKiller;

            switch (PlayerName)
            {
                case "BOT":
                    matchTimelinesKiller = _context.MatchTimeline
                    .Where(mt => mt.SecondaryPlayerAccountId.StartsWith("ai") && (mt.EventType == "LogPlayerMakeGroggy" || mt.EventType == "LogPlayerKillV2"))
                    .Include(mt => mt.Player)
                    .ToList();

                    matchTimelines = _context.MatchTimeline
                    .Where(mt => mt.PlayerAccountId.StartsWith("ai") && (mt.EventType == "LogPlayerMakeGroggy" || mt.EventType == "LogPlayerKillV2"))
                    .Include(mt => mt.SecondaryPlayer)
                    .ToList();

                    DisplayKilledBy = true;
                    break;

                case "Guard":
                    matchTimelinesKiller = _context.MatchTimeline
                    .Where(mt => mt.SecondaryPlayerAccountId.StartsWith("Guard") && (mt.EventType == "LogPlayerMakeGroggy" || mt.EventType == "LogPlayerKillV2"))
                    .Include(mt => mt.Player)
                    .ToList();

                    matchTimelines = _context.MatchTimeline
                    .Where(mt => mt.PlayerAccountId.StartsWith("Guard") && (mt.EventType == "LogPlayerMakeGroggy" || mt.EventType == "LogPlayerKillV2"))
                    .Include(mt => mt.SecondaryPlayer)
                    .ToList();

                    DisplayKilledBy = true;
                    break;

                case "Commander":
                    matchTimelinesKiller = _context.MatchTimeline
                    .Where(mt => mt.SecondaryPlayerAccountId.StartsWith("Commander") && (mt.EventType == "LogPlayerMakeGroggy" || mt.EventType == "LogPlayerKillV2"))
                    .Include(mt => mt.Player)
                    .ToList();

                    matchTimelines = _context.MatchTimeline
                    .Where(mt => mt.PlayerAccountId.StartsWith("Commander") && (mt.EventType == "LogPlayerMakeGroggy" || mt.EventType == "LogPlayerKillV2"))
                    .Include(mt => mt.SecondaryPlayer)
                    .ToList();

                    DisplayKilledBy = true;
                    break;

                case "Bear":
                    matchTimelinesKiller = _context.MatchTimeline
                    .Where(mt => mt.SecondaryPlayerAccountId.StartsWith("Monster.Bear") && (mt.EventType == "LogPlayerMakeGroggy" || mt.EventType == "LogPlayerKillV2"))
                    .Include(mt => mt.Player)
                    .ToList();

                    matchTimelines = _context.MatchTimeline
                    .Where(mt => mt.PlayerAccountId.StartsWith("Monster.Bear") && (mt.EventType == "LogPlayerMakeGroggy" || mt.EventType == "LogPlayerKillV2"))
                    .Include(mt => mt.SecondaryPlayer)
                    .ToList();
                    DisplayKilledBy = true;
                    break;

                case "BlueZone":
                    matchTimelinesKiller = _context.MatchTimeline
                    .Where(mt => mt.DamageCategory == "Damage_BlueZone" && (mt.EventType == "LogPlayerMakeGroggy" || mt.EventType == "LogPlayerKillV2"))
                    .Include(mt => mt.Player)
                    .ToList();
                    break;

                case "RedZone":
                    matchTimelinesKiller = _context.MatchTimeline
                    .Where(mt => mt.DamageCategory == "Damage_Explosion_RedZone" && (mt.EventType == "LogPlayerMakeGroggy" || mt.EventType == "LogPlayerKillV2"))
                    .Include(mt => mt.Player)
                    .ToList();
                    break;

                case "BlackZone":
                    matchTimelinesKiller = _context.MatchTimeline
                    .Where(mt => mt.DamageCategory == "Damage_Explosion_BlackZone" && (mt.EventType == "LogPlayerMakeGroggy" || mt.EventType == "LogPlayerKillV2"))
                    .Include(mt => mt.Player)
                    .ToList();
                    break;


                case "Helicopter":
                    matchTimelinesKiller = _context.MatchTimeline
                    .Where(mt => mt.DamageCategory == "Damage_HelicopterHit" && (mt.EventType == "LogPlayerMakeGroggy" || mt.EventType == "LogPlayerKillV2"))
                    .Include(mt => mt.Player)
                    .ToList();
                    break;

                case "Drowning":
                    matchTimelinesKiller = _context.MatchTimeline
                    .Where(mt => mt.DamageCategory == "Damage_Drown" && (mt.EventType == "LogPlayerMakeGroggy" || mt.EventType == "LogPlayerKillV2"))
                    .Include(mt => mt.Player)
                    .ToList();
                    break;

                case "JerryCan":
                    matchTimelinesKiller = _context.MatchTimeline
                    .Where(mt => mt.DamageCategory == "Damage_Explosion_JerryCan" && (mt.EventType == "LogPlayerMakeGroggy" || mt.EventType == "LogPlayerKillV2"))
                    .Include(mt => mt.Player)
                    .ToList();
                    break;


                case "Suicide":
                    matchTimelinesKiller = _context.MatchTimeline
                    .Where(mt => mt.IsSuicide == true && (mt.EventType == "LogPlayerMakeGroggy" || mt.EventType == "LogPlayerKillV2"))
                    .Include(mt => mt.Player)
                    .ToList();
                    break;

                case "Lava":
                    // TODO: check this
                    return;

                default:
                    // Unknown, go to error page
                    Response.Redirect("/StatusCode?statusCode=404");
                    return;

            }

            var matchTimelinesAsPlayer = new Dictionary<string, PlayerStats>();
            var matchTimelinesAsKiller = new Dictionary<string, PlayerStats>();

            if (DisplayKilledBy && matchTimelines != null)
            {
                foreach (var timeline in matchTimelines)
                {
                    var playerId = timeline.SecondaryPlayerAccountId ?? "Unknown";
                    var playerName = timeline.SecondaryPlayer?.PlayerName ?? "Unknown";

                    if (timeline.SecondaryPlayerIsNPC.GetValueOrDefault())
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
                                    // Need o invesiage
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

                    var match = _context.Matches.FirstOrDefault(m => m.MatchGuid == timeline.MatchGuid);

                    if (timeline.EventType == "LogPlayerMakeGroggy")
                    {
                        if (match.DoNotCount.GetValueOrDefault())
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
                        if (match.DoNotCount.GetValueOrDefault())
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

                    var match = _context.Matches.FirstOrDefault(m => m.MatchGuid == timeline.MatchGuid);
                    if (timeline.EventType == "LogPlayerMakeGroggy")
                    {
                        if (match.DoNotCount.GetValueOrDefault())
                        {
                            matchTimelinesAsKiller[playerId].KnockCountOther++;

                        }
                        else
                        {
                            matchTimelinesAsKiller[playerId].KnockCount++;
                        }
                    }
                    else if (timeline.EventType == "LogPlayerKillV2")
                    {
                        if (match.DoNotCount.GetValueOrDefault())
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

            PlayerKills = matchTimelinesAsKiller.Values.OrderByDescending(ps => ps.KillCount).ThenByDescending(ps => ps.KnockCount).ThenByDescending(ps => ps.KillCountOther).ThenByDescending(ps => ps.KnockCountOther).ToList();
            if (DisplayKilledBy)
            {
                PlayerKilledBy = matchTimelinesAsPlayer.Values.OrderByDescending(ps => ps.KillCount).ThenByDescending(ps => ps.KnockCount).ThenByDescending(ps => ps.KillCountOther).ThenByDescending(ps => ps.KnockCountOther).ToList();
            }
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

