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

            // Load players (exclude random players)
            Players = _context.Players
                .Where(p => p.IsRandom != true)
                .OrderBy(p => p.PlayerName)
                .ToList();

            // Load relevant match timeline events into memory for processing
            var timelines = _context.MatchTimeline
                .Where(mt => mt.EventType == "LogPlayerMakeGroggy" || mt.EventType == "LogPlayerKillV2")
                .Include(mt => mt.Player)
                .Include(mt => mt.SecondaryPlayer)
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

            // Initialize counts dictionary
            if (Players != null)
            {
                foreach (var player in Players)
                {
                    Counts[player.PlayerGuid] = Victims.ToDictionary(v => v, v => 0);
                }
            }

            // Populate counts: for each timeline where there is a killer (SecondaryPlayerGuid)
            foreach (var t in timelines)
            {
                if (t.SecondaryPlayerGuid == null) continue;

                var killerGuid = t.SecondaryPlayerGuid.Value;
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
