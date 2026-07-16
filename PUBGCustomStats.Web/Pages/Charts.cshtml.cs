using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PUBGCustomStats.Data;

namespace PUBGCustomStats.Web.Pages
{
    public class ChartsModel : PageModel
    {
        public Dictionary<string, PlayerStat> KnockKnock { get; set; } = new Dictionary<string, PlayerStat>();
        public Dictionary<string, PlayerStat> Whoshere { get; set; } = new Dictionary<string, PlayerStat>();
        public Dictionary<string, PlayerStat> ElectricBlue { get; set; } = new Dictionary<string, PlayerStat>();
        public Dictionary<string, PlayerStat> RUOK { get; set; } = new Dictionary<string, PlayerStat>();
        public Dictionary<string, PlayerStat> LongestKill { get; set; } = new Dictionary<string, PlayerStat>();
        public Dictionary<string, PlayerStat> RideDistance { get; set; } = new Dictionary<string, PlayerStat>();
        public Dictionary<string, PlayerStat> RoadKills { get; set; } = new Dictionary<string, PlayerStat>();
        public Dictionary<string, PlayerStat> SwimDistance { get; set; } = new Dictionary<string, PlayerStat>();
        public Dictionary<string, PlayerStat> TeamKills { get; set; } = new Dictionary<string, PlayerStat>();
        public Dictionary<string, PlayerStat> VehicleDestroys { get; set; } = new Dictionary<string, PlayerStat>();
        public Dictionary<string, PlayerStat> WalkDistance { get; set; } = new Dictionary<string, PlayerStat>();
        public Dictionary<string, PlayerStat> WeaponsAcquired { get; set; } = new Dictionary<string, PlayerStat>();
        public Dictionary<string, PlayerStat> Games { get; set; } = new Dictionary<string, PlayerStat>();
        public Dictionary<string, PlayerStat> Maps { get; set; } = new Dictionary<string, PlayerStat>();

        public class PlayerStat
        {
            public string PlayerName { get; set; }
            public double StatGameMax { get; set; }
            public double StatGameMin { get; set; }
            public double StatTotal { get; set; }

            public PlayerStat(string playerName)
            {
                PlayerName = playerName;
                StatGameMax = 0;
                StatGameMin = 999;
                StatTotal = 0;
            }

        }

        private readonly PUBGCustomStatsContext _context;

        public ChartsModel(PUBGCustomStatsContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            PopulateModel();
        }

        private void PopulateModel()
        {
            _context.Database.EnsureCreatedAsync();

            foreach (var stat in _context.MatchPlayerStats)
            {
                if (stat.PUBGPlayerId == null || stat.PlayerName == null)
                {
                    continue; // Skip if PUBGPlayerId or PlayerName is null
                }
                if (!stat.PUBGPlayerId.StartsWith("ai."))
                {
                    // add plaer or match stats to the respective collections

                    if (!KnockKnock.ContainsKey(stat.PUBGPlayerId))
                    {
                        KnockKnock[stat.PUBGPlayerId] = new PlayerStat(stat.PlayerName);
                    }
                    if (!Whoshere.ContainsKey(stat.PUBGPlayerId))
                    {
                        Whoshere[stat.PUBGPlayerId] = new PlayerStat(stat.PlayerName);
                    }
                    if (!ElectricBlue.ContainsKey(stat.PUBGPlayerId))
                    {
                        ElectricBlue[stat.PUBGPlayerId] = new PlayerStat(stat.PlayerName);
                    }
                    if (!RUOK.ContainsKey(stat.PUBGPlayerId))
                    {
                        RUOK[stat.PUBGPlayerId] = new PlayerStat(stat.PlayerName);
                    }
                    if (!LongestKill.ContainsKey(stat.PUBGPlayerId))
                    {
                        LongestKill[stat.PUBGPlayerId] = new PlayerStat(stat.PlayerName);
                    }
                    if (!RideDistance.ContainsKey(stat.PUBGPlayerId))
                    {
                        RideDistance[stat.PUBGPlayerId] = new PlayerStat (stat.PlayerName);
                    }
                    if (!RoadKills.ContainsKey(stat.PUBGPlayerId))
                    {
                        RoadKills[stat.PUBGPlayerId] = new PlayerStat (stat.PlayerName);
                    }
                    if (!SwimDistance.ContainsKey(stat.PUBGPlayerId))
                    {
                        SwimDistance[stat.PUBGPlayerId] = new PlayerStat (stat.PlayerName);
                    }
                    if (!TeamKills.ContainsKey(stat.PUBGPlayerId))
                    {
                        TeamKills[stat.PUBGPlayerId] = new PlayerStat (stat.PlayerName);
                    }
                    if (!VehicleDestroys.ContainsKey(stat.PUBGPlayerId))
                    {
                        VehicleDestroys[stat.PUBGPlayerId] = new PlayerStat (stat.PlayerName);
                    }
                    if (!WalkDistance.ContainsKey(stat.PUBGPlayerId))
                    {
                        WalkDistance[stat.PUBGPlayerId] = new PlayerStat (stat.PlayerName);
                    }
                    if (!WeaponsAcquired.ContainsKey(stat.PUBGPlayerId))
                    {
                        WeaponsAcquired[stat.PUBGPlayerId] = new PlayerStat (stat.PlayerName);
                    }
                    if (!Games.ContainsKey(stat.PUBGPlayerId))
                    {
                        Games[stat.PUBGPlayerId] = new PlayerStat(stat.PlayerName);
                    }
                    

                    // Increment the stats based on the match player stats
                    KnockKnock[stat.PUBGPlayerId].StatTotal += stat.DBNOs.GetValueOrDefault(0);
                    Whoshere[stat.PUBGPlayerId].StatTotal += stat.Revives.GetValueOrDefault(0);
                    switch (stat.DeathType)
                    {
                        case "byzone":
                            ElectricBlue[stat.PUBGPlayerId].StatTotal++;
                            break;
                        case "bysuicide":
                        case "suicide":
                            RUOK[stat.PUBGPlayerId].StatTotal++;
                            break;
                    }
                    if (stat.LongestKill.HasValue)
                    {
                        LongestKill[stat.PUBGPlayerId].StatGameMax = Math.Max(LongestKill[stat.PUBGPlayerId].StatGameMax, stat.LongestKill.Value);

                        if (stat.LongestKill.Value > 0)
                        {
                            LongestKill[stat.PUBGPlayerId].StatGameMin = Math.Min(LongestKill[stat.PUBGPlayerId].StatGameMin, stat.LongestKill.Value);
                        }
                    }
                    if (stat.RideDistance.HasValue)
                    {
                        RideDistance[stat.PUBGPlayerId].StatTotal += stat.RideDistance.Value;
                        RideDistance[stat.PUBGPlayerId].StatGameMax = Math.Max(RideDistance[stat.PUBGPlayerId].StatGameMax, stat.RideDistance.Value);
                    }
                    if (stat.RoadKills.HasValue)
                    {
                        RoadKills[stat.PUBGPlayerId].StatTotal += stat.RoadKills.Value;
                        RoadKills[stat.PUBGPlayerId].StatGameMax += Math.Max(RoadKills[stat.PUBGPlayerId].StatGameMax, stat.RoadKills.Value);
                    }
                    if (stat.SwimDistance.HasValue)
                    {
                        SwimDistance[stat.PUBGPlayerId].StatTotal += stat.SwimDistance.Value;
                        SwimDistance[stat.PUBGPlayerId].StatGameMax += Math.Max(SwimDistance[stat.PUBGPlayerId].StatGameMax, stat.SwimDistance.Value);
                    }
                    if (stat.TeamKills.HasValue)
                    {
                        TeamKills[stat.PUBGPlayerId].StatTotal += stat.TeamKills.Value;
                        TeamKills[stat.PUBGPlayerId].StatGameMax += Math.Max(TeamKills[stat.PUBGPlayerId].StatGameMax, stat.TeamKills.Value);
                    }
                    if (stat.VehicleDestroys.HasValue)
                    {
                        VehicleDestroys[stat.PUBGPlayerId].StatTotal += stat.VehicleDestroys.Value;
                        VehicleDestroys[stat.PUBGPlayerId].StatGameMax += Math.Max(VehicleDestroys[stat.PUBGPlayerId].StatGameMax, stat.VehicleDestroys.Value);
                    }
                    if (stat.WalkDistance.HasValue)
                    {
                        WalkDistance[stat.PUBGPlayerId].StatTotal += stat.WalkDistance.Value;
                        WalkDistance[stat.PUBGPlayerId].StatGameMax += Math.Max(WalkDistance[stat.PUBGPlayerId].StatGameMax, stat.WalkDistance.Value);
                    }
                    if (stat.WeaponsAcquired.HasValue)
                    {
                        WeaponsAcquired[stat.PUBGPlayerId].StatTotal += stat.WeaponsAcquired.Value;
                        WeaponsAcquired[stat.PUBGPlayerId].StatGameMax += Math.Max(WeaponsAcquired[stat.PUBGPlayerId].StatGameMax, stat.WeaponsAcquired.Value);
                    }

                    Games[stat.PUBGPlayerId].StatTotal++;
                }
            }

            foreach (var match in _context.Matches)
            {
                if (match.Map == null)
                {
                    continue; // Skip if Map is null
                }
                if (!Maps.ContainsKey(match.Map))
                {
                    Maps[match.Map] = new PlayerStat(match.Map);
                }

                if (!match.DoNotCount.GetValueOrDefault(false))
                {
                    Maps[match.Map].StatTotal++;
                }
            }
        }
    }
}
