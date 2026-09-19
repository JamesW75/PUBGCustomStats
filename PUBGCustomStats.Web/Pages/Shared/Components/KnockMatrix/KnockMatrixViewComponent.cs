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
        private static readonly Guid RedZoneGuid = new("00000000-0000-0000-0000-000000000003");
        private static readonly Guid CarePackageDropGuid = new("00000000-0000-0000-0000-000000000004");
        private static readonly Guid TrainGuid = new("00000000-0000-0000-0000-000000000005");
        private static readonly Guid BearGuid = new("00000000-0000-0000-0000-000000000006");
        private static readonly Guid LavaGuid = new("00000000-0000-0000-0000-000000000007");
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

            var includeRandomPlayers = matchGuid.HasValue || sessionGuid.HasValue;
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

            if (!players.Any(p => p.PlayerGuid == RedZoneGuid))
            {
                players.Add(new Player { PlayerGuid = RedZoneGuid, PlayerName = "Red Zone" });
            }

            if (!players.Any(p => p.PlayerGuid == CarePackageDropGuid))
            {
                players.Add(new Player { PlayerGuid = CarePackageDropGuid, PlayerName = "Care Package Drop" });
            }

            if (!players.Any(p => p.PlayerGuid == TrainGuid))
            {
                players.Add(new Player { PlayerGuid = TrainGuid, PlayerName = "Train" });
            }

            if (!players.Any(p => p.PlayerGuid == BearGuid))
            {
                players.Add(new Player { PlayerGuid = BearGuid, PlayerName = "Bear" });
            }

            if (!players.Any(p => p.PlayerGuid == LavaGuid))
            {
                players.Add(new Player { PlayerGuid = LavaGuid, PlayerName = "Lava" });
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

            var victims = victimLabels
                .Where(v => !string.Equals(v, "Blue Zone", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(v, "Red Zone", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(v, "Care Package Drop", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(v, "Train", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(v, "Lava", StringComparison.OrdinalIgnoreCase))
                .OrderBy(v => string.Equals(v, "Bear", StringComparison.OrdinalIgnoreCase) ? 1
                    : string.Equals(v, "BOT", StringComparison.OrdinalIgnoreCase) ? 2
                    : 0)
                .ThenBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var bearIndex = victims.FindIndex(v => string.Equals(v, "Bear", StringComparison.OrdinalIgnoreCase));
            if (bearIndex >= 0)
            {
                var bear = victims[bearIndex];
                victims.RemoveAt(bearIndex);
                victims.Insert(victims.Count == 0 ? 0 : victims.Count - 1, bear);
            }

            var botIndex = victims.FindIndex(v => string.Equals(v, "BOT", StringComparison.OrdinalIgnoreCase));
            if (botIndex >= 0 && bearIndex >= 0)
            {
                var bot = victims[botIndex];
                victims.RemoveAt(botIndex);
                victims.Add(bot);
            }

            var counts = new Dictionary<Guid, Dictionary<string, int>>();
            foreach (var player in players)
            {
                counts[player.PlayerGuid] = victims.ToDictionary(victim => victim, _ => 0);
            }

            if (counts.Values.All(row => !row.TryGetValue("Bear", out var bearValue) || bearValue == 0))
            {
                victims.RemoveAll(v => string.Equals(v, "Bear", StringComparison.OrdinalIgnoreCase));
                foreach (var playerRow in counts.Values)
                {
                    playerRow.Remove("Bear");
                }
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
                && timeline.SecondaryPlayerAccountId.StartsWith("ai", StringComparison.OrdinalIgnoreCase))
            {
                return BotGuid;
            }

            if (timeline.SecondaryPlayerIsNPC.GetValueOrDefault()
                && !string.IsNullOrEmpty(timeline.SecondaryPlayerAccountId)
                && timeline.SecondaryPlayerAccountId.StartsWith("Monster.Bear", StringComparison.OrdinalIgnoreCase))
            {
                return BearGuid;
            }

            if (timeline.IsSuicide.GetValueOrDefault() && timeline.PlayerGuid.HasValue)
            {
                return timeline.PlayerGuid.Value;
            }

            var specialGuid = GetSpecialAttackerGuid(timeline);
            if (specialGuid.HasValue)
            {
                return specialGuid.Value;
            }

            if (timeline.DamageCategory == "Damage_BlueZone")
            {
                return BlueZoneGuid;
            }

            if (timeline.DamageCategory == "Damage_Explosion_RedZone")
            {
                return RedZoneGuid;
            }

            return null;
        }

        private static Guid? GetSpecialAttackerGuid(MatchTimeline timeline)
        {
            var weaponDescription = MatchHelpers.GetWeaponDescription(timeline.Weapon);
            switch (weaponDescription)
            {
                case "Care Package Drop": return CarePackageDropGuid;
                case "Train": return TrainGuid;
                case "Bear": return BearGuid;
                case "Lava": return LavaGuid;
            }

            var damageDescription = MatchHelpers.GetWeaponDescription(timeline.DamageCategory, timeline.DamageReason);
            switch (damageDescription)
            {
                case "Care Package Drop": return CarePackageDropGuid;
                case "Train": return TrainGuid;
                case "Bear": return BearGuid;
                case "Lava": return LavaGuid;
            }

            if (timeline.DamageCategory == "Damage_Lava")
            {
                return LavaGuid;
            }

            if (timeline.DamageCategory == "Damage_CarePackageDropHit")
            {
                return CarePackageDropGuid;
            }

            if (timeline.DamageCategory == "Damage_Monster"
                && !string.IsNullOrEmpty(timeline.SecondaryPlayerAccountId)
                && timeline.SecondaryPlayerAccountId.StartsWith("Monster.Bear", StringComparison.OrdinalIgnoreCase))
            {
                return BearGuid;
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
                    "Damage_Explosion_RedZone" => "Red Zone",
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
