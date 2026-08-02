using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PUBGCustomStats.Data;
using PUBGCustomStats.Data.Models;
namespace PUBGCustomStats.Web.Pages
{
    public class KillMatrixModel : PageModel
    {
        private readonly PUBGCustomStatsContext _context;
        public List<Player>? Players { get; set; }
        public List<string>? Victims { get; set; }
        // Rows: Player -> Columns: Victim -> Count
        public Dictionary<Guid, Dictionary<string, int>> Counts { get; set; } = new();

        public KillMatrixModel(PUBGCustomStatsContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            _context.Database.EnsureCreatedAsync();

            // Load players (exclude random players) and sort case-insensitively
            Players = _context.Players
                .Where(p => p.IsRandom != true)
                .OrderBy(p => (p.PlayerName ?? string.Empty).ToLower())
                .ToList();

            // Add synthetic BOT player so NPC kills can be shown as a row
            var botGuid = new Guid("00000000-0000-0000-0000-000000000001");
            if (!Players.Any(p => p.PlayerGuid == botGuid))
            {
                Players.Add(new Player { PlayerGuid = botGuid, PlayerName = "BOT" });
            }

            // Add synthetic Blue Zone player to represent environment kills as a killer
            var blueZoneGuid = new Guid("00000000-0000-0000-0000-000000000002");
            if (!Players.Any(p => p.PlayerGuid == blueZoneGuid))
            {
                Players.Add(new Player { PlayerGuid = blueZoneGuid, PlayerName = "Blue Zone" });
            }

            // Load relevant match timeline events into memory for processing
            // Exclude knocks (LogPlayerMakeGroggy) and any matches marked DoNotCount
            var timelines = _context.MatchTimeline
                .Include(mt => mt.Player)
                .Include(mt => mt.SecondaryPlayer)
                .Include(mt => mt.Match)
                .Where(mt => mt.EventType == "LogPlayerKillV2" && mt.Match != null && mt.Match.DoNotCount != true)
                .ToList();

            // Build set of victim labels: include all players (so every player appears as a column)
            var victimLabels = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            // Build set of allowed player names (for columns) from non-random players
            var allowedPlayerNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (Players != null)
            {
                foreach (var p in Players)
                {
                    var name = p.PlayerName ?? "Unknown";
                    victimLabels.Add(name);
                    allowedPlayerNames.Add(name);
                }
            }

            // Also include non-player / special victim labels discovered in timelines
            foreach (var t in timelines)
            {
                var label = GetVictimLabel(t);
                if (string.IsNullOrEmpty(label)) continue;

                // If the label is a player name, only include it if the player is not marked IsRandom
                if (allowedPlayerNames.Contains(label) || !_context.Players.Any(p => p.PlayerName == label))
                {
                    victimLabels.Add(label);
                }
            }

            Victims = victimLabels.ToList();

            // Exclude Blue Zone from victim columns (keep Blue Zone as a killer row only)
            Victims.RemoveAll(v => string.Equals(v, "Blue Zone", StringComparison.OrdinalIgnoreCase));

            // Move BOT label to the end of the victims list so it's the last column
            if (Victims.Remove("BOT"))
            {
                Victims.Add("BOT");
            }

            // Initialize counts dictionary
            if (Players != null)
            {
                foreach (var player in Players)
                {
                    Counts[player.PlayerGuid] = Victims.ToDictionary(v => v, v => 0);
                }
            }

            // Populate counts: for each timeline determine killer GUID (real player or synthetic BOT) and increment
            foreach (var t in timelines)
            {
                Guid killerGuid;

                if (t.SecondaryPlayerGuid != null)
                {
                    killerGuid = t.SecondaryPlayerGuid.Value;
                }
                else if (!string.IsNullOrEmpty(t.SecondaryPlayerAccountId) && t.SecondaryPlayerIsNPC.GetValueOrDefault())
                {
                    // Map NPC account ids starting with ai to BOT synthetic player
                    if (t.SecondaryPlayerAccountId.StartsWith("ai"))
                    {
                        killerGuid = botGuid;
                    }
                    else
                    {
                        // Unknown NPC type - skip
                        continue;
                    }
                }
                else if (!string.IsNullOrEmpty(t.DamageCategory) && t.DamageCategory == "Damage_BlueZone")
                {
                    // Attribute blue zone kills to synthetic Blue Zone player
                    killerGuid = blueZoneGuid;
                }
                else
                {
                    continue;
                }

                var victimLabel = GetVictimLabel(t);
                if (string.IsNullOrEmpty(victimLabel)) continue;

                if (Counts.TryGetValue(killerGuid, out var row))
                {
                    if (!row.ContainsKey(victimLabel)) row[victimLabel] = 0;
                    row[victimLabel]++;
                }
            }
        }

        private string GetVictimLabel(MatchTimeline timeline)
        {
            // If there is a linked Player, prefer its name
            if (timeline.Player != null && !string.IsNullOrEmpty(timeline.Player.PlayerName))
                return timeline.Player.PlayerName;

            var playerId = timeline.PlayerAccountId ?? string.Empty;

            if (!string.IsNullOrEmpty(playerId))
            {
                if (timeline.PlayerIsNPC.GetValueOrDefault())
                {
                    if (playerId.StartsWith("ai")) return "BOT";
                    if (playerId.StartsWith("Guard")) return "Guard";
                    if (playerId.StartsWith("Commander")) return "Commander";
                    if (playerId.StartsWith("Monster.Bear")) return "Bear";
                }
            }

            if (string.IsNullOrEmpty(playerId))
            {
                if (timeline.IsSuicide.GetValueOrDefault()) return "Suicide";
                switch (timeline.DamageCategory)
                {
                    case "Damage_BlueZone": return "Blue Zone";
                    case "Damage_Drown": return "Drowning";
                    case "Damage_Explosion_JerryCan": return "JerryCan";
                    case "Damage_Explosion_RedZone": return "RedZone";
                    case "Damage_Explosion_BlackZone": return "BlackZone";
                    case "Damage_HelicopterHit": return "Helicopter";
                    default:
                        // Fallback to raw damage category or Unknown
                        return timeline.DamageCategory ?? "Unknown";
                }
            }

            return playerId; // fallback to account id if nothing else
        }
    }
}
