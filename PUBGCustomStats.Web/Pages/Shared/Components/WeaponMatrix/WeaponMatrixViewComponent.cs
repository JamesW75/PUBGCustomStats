using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PUBGCustomStats.Data;
using PUBGCustomStats.Data.Models;

namespace PUBGCustomStats.Web.Pages.Shared.Components.WeaponMatrix
{
    public class WeaponMatrixViewComponent : ViewComponent
    {
        private readonly PUBGCustomStatsContext _context;

        public WeaponMatrixViewComponent(PUBGCustomStatsContext context)
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

            var timelinesQuery = _context.MatchTimeline
                .Include(mt => mt.Match)
                .Where(mt => mt.Match != null
                    && mt.Weapon != null
                    && mt.Weapon != ""
                    && mt.Weapon.ToLower() != "none"
                    && (mt.EventType == "LogPlayerKillV2"
                        || mt.EventType == "LogPlayerMakeGroggy"
                        || mt.EventType == "LogPlayerTakeDamage"));

            if (selectedMatchGuids != null)
            {
                timelinesQuery = timelinesQuery.Where(mt => selectedMatchGuids.Contains(mt.MatchGuid));
            }
            else if (!includeDoNotCountMatch)
            {
                timelinesQuery = timelinesQuery.Where(mt => mt.Match != null && mt.Match.DoNotCount != true);
            }

            var timelines = await timelinesQuery.ToListAsync();

            var weaponNames = timelines
                .Select(mt => mt.Weapon)
                .Where(weapon => !string.IsNullOrWhiteSpace(weapon))
                .Select(weapon => MatchHelpers.GetWeaponDescription(weapon))
                .Where(weapon => !string.IsNullOrWhiteSpace(weapon))
                .Where(weapon => !IsNonPlayerWeapon(weapon))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(weapon => weapon, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var counts = new Dictionary<Guid, Dictionary<string, WeaponCellStats>>();
            foreach (var player in players)
            {
                counts[player.PlayerGuid] = weaponNames.ToDictionary(weapon => weapon, _ => new WeaponCellStats());
            }

            foreach (var timeline in timelines)
            {
                if (string.IsNullOrWhiteSpace(timeline.Weapon) || string.Equals(timeline.Weapon, "None", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var weaponName = MatchHelpers.GetWeaponDescription(timeline.Weapon);
                if (string.IsNullOrWhiteSpace(weaponName) || IsNonPlayerWeapon(weaponName))
                {
                    continue;
                }

                switch (timeline.EventType)
                {
                    case "LogPlayerKillV2":
                        var killerGuid = GetKillerGuid(timeline);
                        if (killerGuid.HasValue && counts.TryGetValue(killerGuid.Value, out var killRow) && killRow.ContainsKey(weaponName))
                        {
                            killRow[weaponName].Kills++;
                        }
                        break;

                    case "LogPlayerMakeGroggy":
                        var knockKillerGuid = GetKillerGuid(timeline);
                        if (knockKillerGuid.HasValue && counts.TryGetValue(knockKillerGuid.Value, out var knockRow) && knockRow.ContainsKey(weaponName))
                        {
                            knockRow[weaponName].Knocks++;
                        }
                        break;

                    case "LogPlayerTakeDamage":
                        var damageDealerGuid = GetAttackerGuid(timeline);
                        if (damageDealerGuid.HasValue && counts.TryGetValue(damageDealerGuid.Value, out var damageRow) && damageRow.ContainsKey(weaponName))
                        {
                            damageRow[weaponName].Damage += (int)Math.Round(timeline.Damage.GetValueOrDefault(0), MidpointRounding.AwayFromZero);
                        }
                        break;
                }
            }

            var model = new WeaponMatrixViewModel
            {
                Players = players,
                Weapons = weaponNames,
                Counts = counts
            };

            return View(model);
        }

        private static bool IsNonPlayerWeapon(string? weaponName)
        {
            if (string.IsNullOrWhiteSpace(weaponName))
            {
                return false;
            }

            return weaponName.Equals("Train", StringComparison.OrdinalIgnoreCase)
                || weaponName.Equals("Bear", StringComparison.OrdinalIgnoreCase)
                || weaponName.Equals("Lava", StringComparison.OrdinalIgnoreCase)
                || weaponName.Equals("Blue Zone", StringComparison.OrdinalIgnoreCase)
                || weaponName.Equals("Bluezone", StringComparison.OrdinalIgnoreCase)
                || weaponName.Equals("BOT", StringComparison.OrdinalIgnoreCase)
                || weaponName.Equals("Care Package Drop", StringComparison.OrdinalIgnoreCase)
                || weaponName.Equals("Red Zone", StringComparison.OrdinalIgnoreCase)
                || weaponName.Equals("Redzone", StringComparison.OrdinalIgnoreCase);
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
                return new Guid("00000000-0000-0000-0000-000000000001");
            }

            if (timeline.IsSuicide.GetValueOrDefault() && timeline.PlayerGuid.HasValue)
            {
                return timeline.PlayerGuid.Value;
            }

            if (timeline.DamageCategory == "Damage_BlueZone")
            {
                return new Guid("00000000-0000-0000-0000-000000000002");
            }

            if (timeline.DamageCategory == "Damage_Explosion_RedZone")
            {
                return new Guid("00000000-0000-0000-0000-000000000003");
            }

            return null;
        }

        private static Guid? GetAttackerGuid(MatchTimeline timeline)
        {
            if (timeline.SecondaryPlayerGuid.HasValue)
            {
                return timeline.SecondaryPlayerGuid.Value;
            }

            if (timeline.SecondaryPlayerIsNPC.GetValueOrDefault()
                && !string.IsNullOrEmpty(timeline.SecondaryPlayerAccountId)
                && timeline.SecondaryPlayerAccountId.StartsWith("ai", StringComparison.OrdinalIgnoreCase))
            {
                return new Guid("00000000-0000-0000-0000-000000000001");
            }

            if (timeline.DamageCategory == "Damage_BlueZone")
            {
                return new Guid("00000000-0000-0000-0000-000000000002");
            }

            if (timeline.DamageCategory == "Damage_Explosion_RedZone")
            {
                return new Guid("00000000-0000-0000-0000-000000000003");
            }

            return null;
        }
    }

    public sealed class WeaponCellStats
    {
        public int Kills { get; set; }
        public int Knocks { get; set; }
        public int Damage { get; set; }
    }

    public sealed class WeaponMatrixViewModel
    {
        public required List<Player> Players { get; init; }
        public required List<string> Weapons { get; init; }
        public required Dictionary<Guid, Dictionary<string, WeaponCellStats>> Counts { get; init; }
    }
}
