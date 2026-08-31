using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PUBGCustomStats.Data;
using PUBGCustomStats.Data.Models;

namespace PUBGCustomStats.Web.Pages.Shared.Components.KnockMatrix
{
    public class KnockMatrixViewComponent : ViewComponent
    {
        private static readonly Guid BotGuid = new("00000000-0000-0000-0000-000000000001");
        private static readonly Guid BlueZoneGuid = new("00000000-0000-0000-0000-000000000002");
        private readonly PUBGCustomStatsContext _context;

        public KnockMatrixViewComponent(PUBGCustomStatsContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(
            Guid? matchGuid = null,
            Guid? sessionGuid = null,
            IEnumerable<Guid>? matchGuids = null)
        {
            await _context.Database.EnsureCreatedAsync();

            var includeDoNotCountMatch = matchGuid.HasValue;
            var selectedMatchGuids = matchGuids?.ToHashSet();
            if (matchGuid.HasValue)
            {
                selectedMatchGuids = new HashSet<Guid> { matchGuid.Value };
            }
            else if (sessionGuid.HasValue)
            {
                selectedMatchGuids = await _context.Matches
                    .Where(m => m.SessionGuid == sessionGuid.Value && m.DoNotCount != true)
                    .Select(m => m.MatchGuid)
                    .ToHashSetAsync();
            }

            if (selectedMatchGuids != null)
            {
                selectedMatchGuids = await _context.Matches
                    .Where(m => selectedMatchGuids.Contains(m.MatchGuid)
                        && (includeDoNotCountMatch || m.DoNotCount != true))
                    .Select(m => m.MatchGuid)
                    .ToHashSetAsync();
            }

            var includeRandomPlayers = matchGuid.HasValue;
            var playersQuery = _context.Players.Where(p => includeRandomPlayers || p.IsRandom != true);
            if (selectedMatchGuids != null)
            {
                var participatingPlayerGuids = await _context.MatchPlayerStats
                    .Where(stat => selectedMatchGuids.Contains(stat.MatchGuid) && stat.PlayerGuid.HasValue)
                    .Select(stat => stat.PlayerGuid!.Value)
                    .Distinct()
                    .ToHashSetAsync();

                playersQuery = playersQuery.Where(p => participatingPlayerGuids.Contains(p.PlayerGuid));
            }

            var players = await playersQuery
                .OrderBy(p => (p.PlayerName ?? string.Empty).ToLower())
                .ToListAsync();

            if (!players.Any(p => p.PlayerGuid == BotGuid))
            {
                players.Add(new Player { PlayerGuid = BotGuid, PlayerName = "BOT" });
            }

            if (!players.Any(p => p.PlayerGuid == BlueZoneGuid))
            {
                players.Add(new Player { PlayerGuid = BlueZoneGuid, PlayerName = "Blue Zone" });
            }

            var timelinesQuery = _context.MatchTimeline
                .Include(mt => mt.Player)
                .Include(mt => mt.SecondaryPlayer)
                .Include(mt => mt.Match)
                .Where(mt => mt.EventType == "LogPlayerMakeGroggy"
                    && mt.Match != null
                    && (includeDoNotCountMatch && selectedMatchGuids != null
                        ? selectedMatchGuids.Contains(mt.MatchGuid)
                        : mt.Match.DoNotCount != true));

            if (selectedMatchGuids != null)
            {
                timelinesQuery = timelinesQuery.Where(mt => selectedMatchGuids.Contains(mt.MatchGuid));
            }

            var timelines = await timelinesQuery.ToListAsync();
            var victimLabels = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            var allowedPlayerNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var player in players)
            {
                var name = player.PlayerName ?? "Unknown";
                victimLabels.Add(name);
                allowedPlayerNames.Add(name);
            }

            var playerNames = await _context.Players
                .Select(p => p.PlayerName)
                .Where(name => name != null)
                .ToHashSetAsync(StringComparer.OrdinalIgnoreCase);

            foreach (var timeline in timelines)
            {
                var label = GetVictimLabel(timeline);
                if (!string.IsNullOrEmpty(label)
                    && (allowedPlayerNames.Contains(label) || !playerNames.Contains(label)))
                {
                    victimLabels.Add(label);
                }
            }

            var victims = victimLabels.ToList();
            victims.RemoveAll(v => string.Equals(v, "Blue Zone", StringComparison.OrdinalIgnoreCase));
            if (victims.Remove("BOT"))
            {
                victims.Add("BOT");
            }

            var counts = new Dictionary<Guid, Dictionary<string, int>>();
            foreach (var player in players)
            {
                counts[player.PlayerGuid] = victims.ToDictionary(victim => victim, _ => 0);
            }

            var suicideCells = new HashSet<string>();
            foreach (var timeline in timelines)
            {
                var killerGuid = GetKillerGuid(timeline);
                if (!killerGuid.HasValue)
                {
                    continue;
                }

                var victimLabel = timeline.IsSuicide.GetValueOrDefault()
                    && timeline.Player != null
                    && !string.IsNullOrEmpty(timeline.Player.PlayerName)
                    ? timeline.Player.PlayerName
                    : GetVictimLabel(timeline);

                if (string.IsNullOrEmpty(victimLabel)
                    || string.Equals(victimLabel, "BOT", StringComparison.OrdinalIgnoreCase)
                    || (timeline.PlayerIsNPC.GetValueOrDefault()
                        && timeline.PlayerAccountId?.StartsWith("ai", StringComparison.OrdinalIgnoreCase) == true)
                    || !counts.TryGetValue(killerGuid.Value, out var row))
                {
                    continue;
                }

                if (!row.ContainsKey(victimLabel))
                {
                    row[victimLabel] = 0;
                }

                row[victimLabel]++;
                if (timeline.IsSuicide.GetValueOrDefault())
                {
                    suicideCells.Add($"{killerGuid}:{victimLabel}");
                }
            }

            var model = new KnockMatrixViewModel
            {
                Players = players,
                Victims = victims,
                Counts = counts,
                SuicideCells = suicideCells
            };

            return View(model);
        }

        private static Guid? GetKillerGuid(MatchTimeline timeline)
        {
            if (timeline.SecondaryPlayerGuid.HasValue)
            {
                return timeline.SecondaryPlayerGuid.Value;
            }

            if (timeline.SecondaryPlayerIsNPC.GetValueOrDefault()
                && !string.IsNullOrEmpty(timeline.SecondaryPlayerAccountId)
                && timeline.SecondaryPlayerAccountId.StartsWith("ai"))
            {
                return BotGuid;
            }

            if (timeline.IsSuicide.GetValueOrDefault() && timeline.PlayerGuid.HasValue)
            {
                return timeline.PlayerGuid.Value;
            }

            if (timeline.DamageCategory == "Damage_BlueZone")
            {
                return BlueZoneGuid;
            }

            return null;
        }

        private static string GetVictimLabel(MatchTimeline timeline)
        {
            if (timeline.Player != null && !string.IsNullOrEmpty(timeline.Player.PlayerName))
            {
                return timeline.Player.PlayerName;
            }

            var playerId = timeline.PlayerAccountId ?? string.Empty;
            if (!string.IsNullOrEmpty(playerId) && timeline.PlayerIsNPC.GetValueOrDefault())
            {
                if (playerId.StartsWith("ai")) return "BOT";
                if (playerId.StartsWith("Guard")) return "Guard";
                if (playerId.StartsWith("Commander")) return "Commander";
                if (playerId.StartsWith("Monster.Bear")) return "Bear";
            }

            if (string.IsNullOrEmpty(playerId))
            {
                if (timeline.IsSuicide.GetValueOrDefault()) return "Suicide";
                return timeline.DamageCategory switch
                {
                    "Damage_BlueZone" => "Blue Zone",
                    "Damage_Drown" => "Drowning",
                    "Damage_Explosion_JerryCan" => "JerryCan",
                    "Damage_Explosion_RedZone" => "RedZone",
                    "Damage_Explosion_BlackZone" => "BlackZone",
                    "Damage_HelicopterHit" => "Helicopter",
                    _ => timeline.DamageCategory ?? "Unknown"
                };
            }

            return playerId;
        }
    }

    public sealed class KnockMatrixViewModel
    {
        public required List<Player> Players { get; init; }
        public required List<string> Victims { get; init; }
        public required Dictionary<Guid, Dictionary<string, int>> Counts { get; init; }
        public required HashSet<string> SuicideCells { get; init; }
    }
}
