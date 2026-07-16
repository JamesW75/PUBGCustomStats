using Microsoft.EntityFrameworkCore;
using PUBGCustomStats.Data;
using PUBGCustomStats.Data.Models;
using PUBGCustomStats.Integration;

namespace PUBGCustomStats.Logic
{
    public class Match
    {
        public PUBGCustomStatsContext DbContext { get; set; }
        private DbContextOptions<PUBGCustomStatsContext> _dbContextOptions { get; set; }
        const int SecondsToTicksFactor = 1000000; // 1 second = 10 million ticks
        private IntegrationService _integrationService;
        private string _baseStoragePath;
        private string _baseTelemetryStoragePath;

        public Match(DbContextOptions<PUBGCustomStatsContext> options, IntegrationService integrationService)
        {
            _dbContextOptions = options;
            _integrationService = integrationService;
            _baseStoragePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PUBGCustomStats\\Matches";
            _baseTelemetryStoragePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PUBGCustomStats\\Telemetry";

            if (!Directory.Exists(_baseStoragePath))
            {
                Directory.CreateDirectory(_baseStoragePath);
            }

            if (!Directory.Exists(_baseTelemetryStoragePath))
            {
                Directory.CreateDirectory(_baseTelemetryStoragePath);
            }

            // Initialize the DbContext with the required options
            DbContext = new PUBGCustomStatsContext(options);
        }

        public void AddMatch(Guid matchGuid, Guid currentSeason)
        {
            // Call the API to get match data
            // Save to the storage folder
            Integration.JsonObject.Match matchData;

            if (!File.Exists(Path.Combine(_baseStoragePath, matchGuid.ToString() + ".json")))
            {
                matchData = _integrationService.GetMatch(matchGuid);
                if (matchData != null)
                {
                    File.WriteAllText(Path.Combine(_baseStoragePath, matchGuid.ToString() + ".json"), matchData.RawData);
                }
            }
            else
            {
                var rawData = File.ReadAllText(Path.Combine(_baseStoragePath, matchGuid.ToString() + ".json"));
                matchData = _integrationService.ParseMatch(rawData);
            }


            // check database

            var match = DbContext.Matches.FirstOrDefault(m => m.MatchGuid == matchGuid);
            if (match == null)
            {
                ParseMatch(matchGuid, currentSeason, matchData);
            }

            // Match should be parsed now (and give us telemetr url)

            match = DbContext.Matches.First(m => m.MatchGuid == matchGuid);

            if (!File.Exists(Path.Combine(_baseTelemetryStoragePath, matchGuid.ToString() + ".json")))
            {
                if (!string.IsNullOrEmpty(match.TelemetryUrl))
                {
                    Console.WriteLine($"Downloading telemetry data for match: {matchGuid}");
                    var telemetryData = _integrationService.GetTelemetry(match.TelemetryUrl);

                    File.WriteAllText(Path.Combine(_baseTelemetryStoragePath, matchGuid.ToString() + ".json"), telemetryData.Result);

                    ParseTelemetry(matchGuid, telemetryData.Result);
                }
            }
            else
            {
                var telemetryRawData = File.ReadAllText(Path.Combine(_baseTelemetryStoragePath, matchGuid.ToString() + ".json"));
                ParseTelemetry(matchGuid, telemetryRawData);
            }
        }

        public List<Data.Models.Match> ListMatches(Guid sessionGuid)
        {
            return DbContext.Matches.Where(m => m.SessionGuid == sessionGuid).ToList();
        }

        public void EditMatch(Guid matchGuid, string newMatchName)
        {
            var match = DbContext.Matches.FirstOrDefault(m => m.MatchGuid == matchGuid);
            if (match != null)
            {
                match.MatchName = newMatchName;
                DbContext.SaveChanges();
            }
        }
        public void ParseMatch(Guid matchGuid, Guid currentSeason, Integration.JsonObject.Match matchData)
        {
            var match = new Data.Models.Match();

            DbContext.Matches.Add(match);

            match.MatchGuid = matchGuid;
            match.SessionGuid = currentSeason;

            match.MatchLength = new TimeOnly(TimeSpan.FromSeconds(matchData.data.attributes.duration).Ticks);
            match.StartTime = matchData.data.attributes.createdAt;
            //match.Winner = matchData.data.attributes.winner;

            switch (matchData.data.attributes.mapName)
            {
                case "Desert_Main":
                    match.Map = "Sanhok";
                    break;
                case "Erangel_Main":
                case "Baltic_Main":
                    match.Map = "Erangel";
                    break;
                case "Vikendi_Main":
                case "DihorOtok_Main":
                    match.Map = "Vikendi";
                    break;
                case "Savage_Main":
                    match.Map = "Sanhok";
                    break;
                case "Taego_Main":
                case "Tiger_Main":
                    match.Map = "Taego";
                    break;
                case "Karakin_Main":
                case "Summerland_Main":
                    match.Map = "Karakin";
                    break;
                case "Paramo_Main":
                case "Chimera_Main":
                    match.Map = "Paramo";
                    break;
                case "Deston_Main":
                case "Kiki_Main":
                    match.Map = "Deston";
                    break;
                case "Arenas_Main":
                    match.Map = "Arenas";
                    break;
                case "Training_Main":
                    match.Map = "Training";
                    break;
                case "Custom_Main":
                    match.Map = "Custom";
                    break;
                case "Neon_Main":
                    match.Map = "Rondo";
                    break;
                case "Heaven_Main":
                    match.Map = "Haven";
                    break;

                default:
                    match.Map = matchData.data.attributes.mapName;
                    break;
            }


            match.MatchType = matchData.data.attributes.matchType;

            switch (matchData.data.attributes.gameMode)
            {
                case "normal-solo":
                case "normal-solo-fpp":
                case "solo":
                    match.GameMode = "Solo";
                    break;
                case "normal-duo":
                case "normal-duo-fpp":
                case "duo":
                    match.GameMode = "Duo";
                    break;
                case "normal-squad":
                case "normal-squad-fpp":
                case "squad":
                    match.GameMode = "Squad";
                    break;
                case "tdm":
                    match.GameMode = "TDM";
                    break;
                default:
                    match.GameMode = matchData.data.attributes.gameMode;
                    break;
            }

            match.Perspective = "TPP";
            switch (matchData.data.attributes.gameMode)
            {
                case "normal-solo-fpp":
                case "normal-duo-fpp":
                case "normal-squad-fpp":
                    match.Perspective = "FPP";
                    break;
            }

            // Find the players in the match
            var players = matchData.included
            .Where(i => i.type == "participant")
            .ToList();

            // Find the teams in the match
            var teams = matchData.included
                .Where(i => i.type == "roster")
                .ToList();

            // Find the assets in the match
            var assets = matchData.included
                .Where(i => i.type == "asset")
                .ToList();

            var telemetry = assets.FirstOrDefault(a => a.attributes.name == "telemetry");
            if (telemetry != null)
            {
                match.TelemetryUrl = telemetry.attributes.URL;
            }

            // Find the winner team
            var winnerTeam = teams.FirstOrDefault(t => t.attributes.won == "true");
            var winningParticipants = new List<string>();

            if (winnerTeam != null)
            {
                //match.Winner = winnerTeam.attributes.stats.teamId.ToString();
                match.Winner = string.Empty;


                foreach (var participant in winnerTeam.relationships.participants.data)
                {
                    winningParticipants.Add(participant.id);
                }
            }

            // Delete existing player stats for this match
            DbContext.MatchPlayerStats.RemoveRange(DbContext.MatchPlayerStats.Where(mps => mps.MatchGuid == match.MatchGuid));

            int damageDealtPosition = 0;
            float lastDamageDealt = 0;
            int lastDamagePosition = 0;

            Player player = new Player(_dbContextOptions, _integrationService);

            foreach (var playerData in players.OrderByDescending(s => s.attributes.stats.damageDealt))
            {
                damageDealtPosition++;

                if (winningParticipants.Contains(playerData.id))
                {
                    if (string.IsNullOrEmpty(match.Winner))
                    {
                        match.Winner = playerData.attributes.stats.name;
                    }
                    else
                    {
                        match.Winner += ", " + playerData.attributes.stats.name;
                    }
                }
                // Get team data for the player from the match data
                //var teamData = matchData.included
                //  .FirstOrDefault(i => i.type == "roster" && i.id == playerData.relationships.roster.data.id);

                var matchPlayerStat = new Data.Models.MatchPlayerStat
                {
                    MatchGuid = match.MatchGuid,
                    //PlayerGuid = Guid.Parse(playerData.id),
                    PlayerName = playerData.attributes.stats.name,
                    Platform = playerData.attributes.shardId,
                    TeamId = playerData.attributes.stats.teamId,
                    Kills = playerData.attributes.stats.kills,
                    Assists = playerData.attributes.stats.assists,
                    DamageDealt = playerData.attributes.stats.damageDealt,
                    HeadshotKills = playerData.attributes.stats.headshotKills,
                    DBNOs = playerData.attributes.stats.DBNOs,
                    Heals = playerData.attributes.stats.heals,
                    Boosts = playerData.attributes.stats.boosts,
                    KillPlace = playerData.attributes.stats.killPlace,
                    KillStreaks = playerData.attributes.stats.killStreaks,
                    DeathType = playerData.attributes.stats.deathType,
                    PUBGPlayerId = playerData.attributes.stats.playerId,
                    Revives = playerData.attributes.stats.revives,
                    TimeSurvived = new TimeOnly(TimeSpan.FromSeconds(playerData.attributes.stats.timeSurvived).Ticks),
                    LongestKill = playerData.attributes.stats.longestKill,
                    RideDistance = playerData.attributes.stats.rideDistance,
                    RoadKills = playerData.attributes.stats.roadKills,
                    SwimDistance = playerData.attributes.stats.swimDistance,
                    TeamKills = playerData.attributes.stats.teamKills,
                    WalkDistance = playerData.attributes.stats.walkDistance,
                    VehicleDestroys = playerData.attributes.stats.vehicleDestroys,
                    WeaponsAcquired = playerData.attributes.stats.weaponsAcquired,
                };

                // Lookup Player data (exclude ai)
                if (matchPlayerStat.PUBGPlayerId != null)
                {
                    if (!matchPlayerStat.PUBGPlayerId.StartsWith("ai.") && !matchPlayerStat.PUBGPlayerId.StartsWith("Monster."))
                    {
                        matchPlayerStat.PlayerGuid = player.LookupPlayer(matchPlayerStat.PUBGPlayerId);
                    }
                }

                // only give a damage delt position if the player has dealt damage
                if (playerData.attributes.stats.damageDealt > 0)
                {
                    if (playerData.attributes.stats.damageDealt == lastDamageDealt)
                    {
                        matchPlayerStat.DamagePlaceEqual = true;
                        matchPlayerStat.DamagePlace = lastDamagePosition;
                    }
                    else
                    {
                        matchPlayerStat.DamagePlaceEqual = false;
                        matchPlayerStat.DamagePlace = damageDealtPosition;
                        lastDamagePosition = damageDealtPosition;
                        lastDamageDealt = playerData.attributes.stats.damageDealt;
                    }
                }
                else
                {
                    matchPlayerStat.DamagePlace = null; // No damage dealt, no position
                }

                // Find the team with poarticipant data
                foreach (var team in teams)
                {
                    // Check if the player is part of the team
                    if (team.relationships.participants.data.Any(p => p.id == playerData.id))
                    {
                        matchPlayerStat.TeamId = team.attributes.stats.teamId;
                        matchPlayerStat.Rank = team.attributes.stats.rank;

                        matchPlayerStat.Score = CalculateScore(matchPlayerStat.Rank, matchPlayerStat.KillPlace, matchPlayerStat.DamagePlace);

                        break; // Exit the loop once the team is found
                    }
                }

                DbContext.MatchPlayerStats.Add(matchPlayerStat);
            }

            // Update the match with the parsed data
            DbContext.SaveChanges();

        }

        public void ParseTelemetry(Guid matchGuid, string telemetryRawData)
        {

            Console.WriteLine($"Parsing telemetry events for: {matchGuid}");

            var telemetryData = _integrationService.ParseTelemetry(telemetryRawData);

            Console.WriteLine($"Total telemetry events: {telemetryData.Count()}");


            DbContext.Database.EnsureCreated();

            Player player = new Player(_dbContextOptions, _integrationService);

            var existingData = DbContext.MatchTimeline.Where(mt => mt.MatchGuid == matchGuid).Include("MatchTimelinePlayers");
            // Delete existing timeline events for this match
            foreach (var timeline in existingData)
            {
                DbContext.MatchTimelinePlayer.RemoveRange(timeline.MatchTimelinePlayers);
            }
            DbContext.MatchTimeline.RemoveRange(existingData);

            foreach (var telemetryEvent in telemetryData)
            {
                bool eventProcessed = false;

                var matchTimeline = new MatchTimeline
                {
                    MatchGuid = matchGuid,
                    EventType = telemetryEvent._T,
                    EventTimestamp = telemetryEvent._D,
                    //EventSeconds = (int)(telemetryEvent._D - telemetryData.First()._D).TotalSeconds,
                    //RawData = telemetryEvent.RawData
                };

                if (telemetryEvent.common != null)
                {
                    matchTimeline.IsGame = (decimal)telemetryEvent.common.isGame;
                }

                // https://documentation.pubg.com/en/telemetry-events.html#telemetry-events
                switch (telemetryEvent._T)
                {
                    case "LogMatchEnd":
                        if (telemetryEvent.gameResultOnFinished != null)
                        {
                            if (telemetryEvent.gameResultOnFinished.results != null)
                            {
                                foreach (var result in telemetryEvent.gameResultOnFinished.results)
                                {
                                    if (result.rank == 1)
                                    {
                                        var winningPlayer = new MatchTimelinePlayer
                                        {
                                            MatchTimelineGuid = matchTimeline.MatchTimelineGuid,
                                            PlayerAccountId = result.accountId
                                        };
                                        if (!winningPlayer.PlayerAccountId.StartsWith("ai.") && !winningPlayer.PlayerAccountId.StartsWith("Monster."))
                                        {
                                            winningPlayer.PlayerGuid = player.LookupPlayer(winningPlayer.PlayerAccountId);
                                        }
                                        else
                                        {
                                            winningPlayer.PlayerIsNPC = true;
                                        }

                                        if (matchTimeline.MatchTimelinePlayers == null)
                                        {
                                            matchTimeline.MatchTimelinePlayers = new List<MatchTimelinePlayer>();
                                        }
                                        matchTimeline.MatchTimelinePlayers.Add(winningPlayer);
                                    }
                                }
                            }
                        }

                        eventProcessed = true;
                        break;

                    case "LogMatchStart":
                        eventProcessed = true;
                        SetMatchDetails(matchGuid, telemetryEvent.weatherId, telemetryEvent.blueZoneCustomOptions);
                        break;

                    case "LogPlayerKillV2":
                        if (telemetryEvent.killer != null)
                        {
                            matchTimeline.SecondaryPlayerAccountId = telemetryEvent.killer.accountId;
                            if (!matchTimeline.SecondaryPlayerAccountId.StartsWith("ai.") && !matchTimeline.SecondaryPlayerAccountId.StartsWith("Monster."))
                            {
                                matchTimeline.SecondaryPlayerGuid = player.LookupPlayer(matchTimeline.SecondaryPlayerAccountId);
                            }
                            else
                            {
                                if (telemetryEvent.killer.name == "Guard" ||
                                    telemetryEvent.killer.name == "Commander")
                                {
                                    // Guard are a type of AI, so prepend Guard
                                    matchTimeline.SecondaryPlayerAccountId = telemetryEvent.killer.name + "." + matchTimeline.SecondaryPlayerAccountId;
                                }
                                matchTimeline.SecondaryPlayerIsNPC = true;
                            }
                        }
                        matchTimeline.PlayerAccountId = telemetryEvent.victim.accountId;
                        if (!matchTimeline.PlayerAccountId.StartsWith("ai.") && !matchTimeline.PlayerAccountId.StartsWith("Monster."))
                        {
                            matchTimeline.PlayerGuid = player.LookupPlayer(matchTimeline.PlayerAccountId);
                        }
                        else
                        {
                            if (telemetryEvent.victim.name == "Guard" ||
                                    telemetryEvent.victim.name == "Commander")
                            {
                                // Guard are a type of AI, so prepend Guard
                                matchTimeline.PlayerAccountId = telemetryEvent.victim.name + "." + matchTimeline.PlayerAccountId;
                            }
                            matchTimeline.PlayerIsNPC = true;
                        }
                        matchTimeline.DamageReason = telemetryEvent.killerDamageInfo.damageReason;
                        matchTimeline.DamageCategory = telemetryEvent.killerDamageInfo.damageTypeCategory;
                        matchTimeline.Distance = telemetryEvent.killerDamageInfo.distance;
                        matchTimeline.Weapon = telemetryEvent.killerDamageInfo.damageCauserName;

                        if (string.IsNullOrEmpty(matchTimeline.DamageReason) && telemetryEvent.finishDamageInfo != null)
                        {
                            matchTimeline.DamageCategory = telemetryEvent.finishDamageInfo.damageTypeCategory;
                            matchTimeline.DamageReason = telemetryEvent.finishDamageInfo.damageReason;
                        }


                        matchTimeline.IsSuicide = telemetryEvent.isSuicide;

                        if (telemetryEvent.assists_AccountId != null)
                        {
                            foreach (var account in telemetryEvent.assists_AccountId)
                            {
                                // add assist players
                                var assistPlayer = new MatchTimelinePlayer
                                {
                                    MatchTimelineGuid = matchTimeline.MatchTimelineGuid,
                                    PlayerAccountId = account.ToString()
                                };

                                if (assistPlayer.PlayerAccountId != null)
                                {
                                    if (!assistPlayer.PlayerAccountId.StartsWith("ai.") && !assistPlayer.PlayerAccountId.StartsWith("Monster."))
                                    {
                                        assistPlayer.PlayerGuid = player.LookupPlayer(assistPlayer.PlayerAccountId);
                                    }
                                    else
                                    {
                                        assistPlayer.PlayerIsNPC = true;
                                    }
                                }
                                if (matchTimeline.MatchTimelinePlayers == null)
                                {
                                    matchTimeline.MatchTimelinePlayers = new List<MatchTimelinePlayer>();
                                }
                                matchTimeline.MatchTimelinePlayers.Add(assistPlayer);
                            }
                        }

                        if (telemetryEvent.victim.zone != null && telemetryEvent.victim.zone.Length > 0)
                        {
                            matchTimeline.Zone = ArrayToCsv(telemetryEvent.victim.zone);
                        }
                        matchTimeline.Ranking = telemetryEvent.victim.ranking;
                        matchTimeline.IndividualRanking = telemetryEvent.victim.individualRanking;

                        eventProcessed = true;
                        break;

                    case "LogPlayerMakeGroggy":
                        if (telemetryEvent.attacker != null)
                        {
                            if (!string.IsNullOrEmpty(telemetryEvent.attacker.accountId))
                            {
                                matchTimeline.SecondaryPlayerAccountId = telemetryEvent.attacker.accountId;
                                if (!matchTimeline.SecondaryPlayerAccountId.StartsWith("ai.") && !matchTimeline.SecondaryPlayerAccountId.StartsWith("Monster."))
                                {
                                    matchTimeline.SecondaryPlayerGuid = player.LookupPlayer(matchTimeline.SecondaryPlayerAccountId);
                                }
                                else
                                {
                                    if (telemetryEvent.attacker.name == "Guard" ||
                                    telemetryEvent.attacker.name == "Commander")
                                    {
                                        // Guard are a type of AI, so prepend Guard
                                        matchTimeline.SecondaryPlayerAccountId = telemetryEvent.attacker.name + "." + matchTimeline.SecondaryPlayerAccountId;
                                    }
                                    matchTimeline.SecondaryPlayerIsNPC = true;
                                }
                            }
                        }
                        matchTimeline.PlayerAccountId = telemetryEvent.victim.accountId;
                        if (!matchTimeline.PlayerAccountId.StartsWith("ai.") && !matchTimeline.PlayerAccountId.StartsWith("Monster."))
                        {
                            matchTimeline.PlayerGuid = player.LookupPlayer(matchTimeline.PlayerAccountId);
                        }
                        else
                        {
                            if (telemetryEvent.victim.name == "Guard" || telemetryEvent.victim.name == "Commander")
                            {
                                // Guard are a type of AI, so prepend Guard
                                matchTimeline.PlayerAccountId = telemetryEvent.victim.name + "." + matchTimeline.PlayerAccountId;
                            }

                            matchTimeline.PlayerIsNPC = true;
                        }
                        //if (telemetryEvent.killerDamageInfo != null)
                        //{
                        matchTimeline.DamageReason = telemetryEvent.damageReason;
                        matchTimeline.DamageCategory = telemetryEvent.damageTypeCategory;
                        matchTimeline.Distance = telemetryEvent.distance;
                        matchTimeline.Weapon = telemetryEvent.damageCauserName;
                        //}
                        if (string.IsNullOrEmpty(matchTimeline.DamageReason) && telemetryEvent.finishDamageInfo != null)
                        {
                            matchTimeline.DamageCategory = telemetryEvent.finishDamageInfo.damageTypeCategory;
                            matchTimeline.DamageReason = telemetryEvent.finishDamageInfo.damageReason;
                        }
                        matchTimeline.DBNOId = telemetryEvent.dBNOId;

                        if (telemetryEvent.victim.zone != null && telemetryEvent.victim.zone.Length > 0)
                        {
                            matchTimeline.Zone = ArrayToCsv(telemetryEvent.victim.zone);
                        }

                        eventProcessed = true;
                        break;

                    case "LogPhaseChange":
                        matchTimeline.Phase = telemetryEvent.phase;
                        eventProcessed = true;
                        break;

                    case "LogPlayerUseFlareGun":
                        //matchTimeline.PlayerAccountId = telemetryEvent.victim.accountId;
                        //matchTimeline.PlayerGuid = player.LookupPlayer(matchTimeline.PlayerAccountId);
                        if (telemetryEvent.attacker != null)
                        {
                            matchTimeline.PlayerAccountId = telemetryEvent.attacker.accountId;
                            if (!matchTimeline.PlayerAccountId.StartsWith("ai.") && !matchTimeline.PlayerAccountId.StartsWith("Monster."))
                            {
                                matchTimeline.PlayerGuid = player.LookupPlayer(matchTimeline.PlayerAccountId);
                            }
                            else
                            {
                                matchTimeline.PlayerIsNPC = true;
                            }

                            if (telemetryEvent.attacker.zone != null && telemetryEvent.attacker.zone.Length > 0)
                            {
                                matchTimeline.Zone = ArrayToCsv(telemetryEvent.attacker.zone);
                            }
                        }
                        eventProcessed = true;
                        break;

                    case "LogPlayerRevive":
                        if (telemetryEvent.reviver != null)
                        {
                            matchTimeline.SecondaryPlayerAccountId = telemetryEvent.reviver.accountId;
                            if (!matchTimeline.SecondaryPlayerAccountId.StartsWith("ai.") && !matchTimeline.SecondaryPlayerAccountId.StartsWith("Monster."))
                            {
                                matchTimeline.SecondaryPlayerGuid = player.LookupPlayer(matchTimeline.SecondaryPlayerAccountId);
                            }
                            else
                            {
                                matchTimeline.SecondaryPlayerIsNPC = true;
                            }
                        }
                        matchTimeline.PlayerAccountId = telemetryEvent.victim.accountId;
                        if (!matchTimeline.PlayerAccountId.StartsWith("ai.") && !matchTimeline.PlayerAccountId.StartsWith("Monster."))
                        {
                            matchTimeline.PlayerGuid = player.LookupPlayer(matchTimeline.PlayerAccountId);
                        }
                        else
                        {
                            matchTimeline.PlayerIsNPC = true;
                        }

                        if (telemetryEvent.victim.zone != null && telemetryEvent.victim.zone.Length > 0)
                        {
                            matchTimeline.Zone = ArrayToCsv(telemetryEvent.victim.zone);
                        }
                        eventProcessed = true;
                        break;

                    case "LogWheelDestroy":
                        if (telemetryEvent.attacker != null)
                        {
                            if (telemetryEvent.attacker.accountId != "")
                            {
                                matchTimeline.PlayerAccountId = telemetryEvent.attacker.accountId;
                                if (!matchTimeline.PlayerAccountId.StartsWith("ai.") && !matchTimeline.PlayerAccountId.StartsWith("Monster."))
                                {
                                    matchTimeline.PlayerGuid = player.LookupPlayer(matchTimeline.PlayerAccountId);
                                }
                                else
                                {
                                    matchTimeline.PlayerIsNPC = true;
                                }
                            }
                            if (telemetryEvent.attacker.zone != null && telemetryEvent.attacker.zone.Length > 0)
                            {
                                matchTimeline.Zone = ArrayToCsv(telemetryEvent.attacker.zone);
                            }
                        }
                        matchTimeline.DamageReason = telemetryEvent.damageCauserName;
                        matchTimeline.DamageCategory = telemetryEvent.damageTypeCategory;
                        matchTimeline.Weapon = telemetryEvent.damageCauserName;
                        matchTimeline.Vehicle = telemetryEvent.vehicle.vehicleId;

                        eventProcessed = true;
                        break;

                    case "LogVehicleDestroy":
                        if (telemetryEvent.attacker != null)
                        {
                            if (telemetryEvent.attacker.accountId != "")
                            {
                                matchTimeline.PlayerAccountId = telemetryEvent.attacker.accountId;
                                if (!matchTimeline.PlayerAccountId.StartsWith("ai.") && !matchTimeline.PlayerAccountId.StartsWith("Monster."))
                                {
                                    matchTimeline.PlayerGuid = player.LookupPlayer(matchTimeline.PlayerAccountId);
                                }
                                else
                                {
                                    matchTimeline.PlayerIsNPC = true;
                                }
                            }

                            if (telemetryEvent.attacker.zone != null && telemetryEvent.attacker.zone.Length > 0)
                            {
                                matchTimeline.Zone = ArrayToCsv(telemetryEvent.attacker.zone);
                            }
                        }

                        matchTimeline.DamageReason = telemetryEvent.damageCauserName;
                        matchTimeline.DamageCategory = telemetryEvent.damageTypeCategory;
                        matchTimeline.Weapon = telemetryEvent.damageCauserName;
                        matchTimeline.Distance = telemetryEvent.distance;
                        matchTimeline.Vehicle = telemetryEvent.vehicle.vehicleId;


                        eventProcessed = true;
                        break;

                    case "LogEmPickupLiftOff":
                        matchTimeline.PlayerAccountId = telemetryEvent.instigator.accountId;
                        if (!matchTimeline.PlayerAccountId.StartsWith("ai.") && !matchTimeline.PlayerAccountId.StartsWith("Monster."))
                        {
                            matchTimeline.PlayerGuid = player.LookupPlayer(matchTimeline.PlayerAccountId);
                        }
                        else
                        {
                            matchTimeline.PlayerIsNPC = true;
                        }

                        if (telemetryEvent.riders != null)
                        {
                            foreach (var account in telemetryEvent.riders)
                            {
                                // add assist players
                                var assistPlayer = new MatchTimelinePlayer
                                {
                                    MatchTimelineGuid = matchTimeline.MatchTimelineGuid,
                                    PlayerAccountId = account.accountId
                                };
                                if (!assistPlayer.PlayerAccountId.StartsWith("ai.") && !assistPlayer.PlayerAccountId.StartsWith("Monster."))
                                {
                                    assistPlayer.PlayerGuid = player.LookupPlayer(assistPlayer.PlayerAccountId);
                                }
                                else
                                {
                                    assistPlayer.PlayerIsNPC = true;
                                }
                                if (matchTimeline.MatchTimelinePlayers == null)
                                {
                                    matchTimeline.MatchTimelinePlayers = new List<MatchTimelinePlayer>();
                                }
                                matchTimeline.MatchTimelinePlayers.Add(assistPlayer);
                            }
                        }

                        if (telemetryEvent.instigator.zone != null && telemetryEvent.instigator.zone.Length > 0)
                        {
                            matchTimeline.Zone = ArrayToCsv(telemetryEvent.instigator.zone);
                        }

                        eventProcessed = true;
                        break;

                    case "LogCharacterCarry":
                        matchTimeline.PlayerAccountId = telemetryEvent.character.accountId;
                        if (!matchTimeline.PlayerAccountId.StartsWith("ai.") && !matchTimeline.PlayerAccountId.StartsWith("Monster."))
                        {
                            matchTimeline.PlayerGuid = player.LookupPlayer(matchTimeline.PlayerAccountId);
                        }
                        else
                        {
                            matchTimeline.PlayerIsNPC = true;
                        }

                        matchTimeline.Status = telemetryEvent.carryState;
                        if (telemetryEvent.character.zone != null && telemetryEvent.character.zone.Length > 0)
                        {
                            matchTimeline.Zone = ArrayToCsv(telemetryEvent.character.zone);
                        }

                        eventProcessed = true;
                        break;

                    case "LogCarePackageSpawn":
                    case "LogCarePackageLand":
                        // item package
                        eventProcessed = true;
                        break;

                    case "LogRedZoneEnded":

                        // TODO: Drivers 
                        eventProcessed = true;
                        break;

                    case "LogBlackZoneEnded":
                        // ODO: Survivors
                        eventProcessed = true;
                        break;

                    case "LogPlayerRedeploy":
                        matchTimeline.PlayerAccountId = telemetryEvent.character.accountId;
                        if (!matchTimeline.PlayerAccountId.StartsWith("ai.") && !matchTimeline.PlayerAccountId.StartsWith("Monster."))
                        {
                            matchTimeline.PlayerGuid = player.LookupPlayer(matchTimeline.PlayerAccountId);
                        }
                        else
                        {
                            matchTimeline.PlayerIsNPC = true;
                        }

                        if (telemetryEvent.character.zone != null && telemetryEvent.character.zone.Length > 0)
                        {
                            matchTimeline.Zone = ArrayToCsv(telemetryEvent.character.zone);
                        }

                        eventProcessed = true;
                        break;

                    case "LogPlayerRedeployBRStart":

                        // TODO: Characters loop
                        if (telemetryEvent.characters != null && telemetryEvent.characters.Length > 0)
                        {
                            matchTimeline.PlayerAccountId = telemetryEvent.characters[0].character.accountId;
                            if (!matchTimeline.PlayerAccountId.StartsWith("ai.") && !matchTimeline.PlayerAccountId.StartsWith("Monster."))
                            {
                                matchTimeline.PlayerGuid = player.LookupPlayer(matchTimeline.PlayerAccountId);
                            }
                            else
                            {
                                matchTimeline.PlayerIsNPC = true;
                            }
                        }
                        eventProcessed = true;
                        break;

                    case "LogParachuteLanding":
                        matchTimeline.PlayerAccountId = telemetryEvent.character.accountId;
                        if (!matchTimeline.PlayerAccountId.StartsWith("ai.") && !matchTimeline.PlayerAccountId.StartsWith("Monster."))
                        {
                            matchTimeline.PlayerGuid = player.LookupPlayer(matchTimeline.PlayerAccountId);
                        }
                        else
                        {
                            matchTimeline.PlayerIsNPC = true;
                        }

                        matchTimeline.Distance = telemetryEvent.distance;

                        if (telemetryEvent.character.zone != null && telemetryEvent.character.zone.Length > 0)
                        {
                            matchTimeline.Zone = ArrayToCsv(telemetryEvent.character.zone);
                        }

                        eventProcessed = true;
                        break;

                    case "LogPlayerDestroyBreachableWall":
                        if (telemetryEvent.attacker != null)
                        {
                            if (!string.IsNullOrEmpty(telemetryEvent.attacker.accountId))
                            {
                                matchTimeline.PlayerAccountId = telemetryEvent.attacker.accountId;
                                if (!matchTimeline.PlayerAccountId.StartsWith("ai.") && !matchTimeline.PlayerAccountId.StartsWith("Monster."))
                                {
                                    matchTimeline.PlayerGuid = player.LookupPlayer(matchTimeline.PlayerAccountId);
                                }
                                else
                                {
                                    matchTimeline.PlayerIsNPC = true;
                                }
                            }

                            if (telemetryEvent.attacker.zone != null && telemetryEvent.attacker.zone.Length > 0)
                            {
                                matchTimeline.Zone = ArrayToCsv(telemetryEvent.attacker.zone);
                            }
                        }

                        matchTimeline.DamageReason = telemetryEvent.weapon.subCategory;
                        matchTimeline.DamageCategory = telemetryEvent.weapon.category;
                        matchTimeline.Weapon = telemetryEvent.weapon.itemId;

                        eventProcessed = true;
                        break;

                    case "LogVehicleLeave":
                        matchTimeline.PlayerAccountId = telemetryEvent.character.accountId;
                        if (!matchTimeline.PlayerAccountId.StartsWith("ai.") && !matchTimeline.PlayerAccountId.StartsWith("Monster."))
                        {
                            matchTimeline.PlayerGuid = player.LookupPlayer(matchTimeline.PlayerAccountId);
                        }
                        else
                        {
                            matchTimeline.PlayerIsNPC = true;
                        }
                        matchTimeline.Vehicle = telemetryEvent.vehicle.vehicleId;
                        eventProcessed = true;

                        if (telemetryEvent.character.zone != null && telemetryEvent.character.zone.Length > 0)
                        {
                            matchTimeline.Zone = ArrayToCsv(telemetryEvent.character.zone);
                        }
                        break;

                    case "LogItemUse":
                        matchTimeline.Weapon = telemetryEvent.item.itemId;
                        matchTimeline.DamageCategory = telemetryEvent.item.category;
                        matchTimeline.DamageReason = telemetryEvent.item.subCategory;
                        matchTimeline.PlayerAccountId = telemetryEvent.character.accountId;
                        if (!matchTimeline.PlayerAccountId.StartsWith("ai.") && !matchTimeline.PlayerAccountId.StartsWith("Monster."))
                        {
                            matchTimeline.PlayerGuid = player.LookupPlayer(matchTimeline.PlayerAccountId);
                        }
                        else
                        {
                            matchTimeline.PlayerIsNPC = true;
                        }

                        if (telemetryEvent.character.zone != null && telemetryEvent.character.zone.Length > 0)
                        {
                            matchTimeline.Zone = ArrayToCsv(telemetryEvent.character.zone);
                        }

                        eventProcessed = true;
                        break;

                    case "LogObjectInteraction":
                        switch (telemetryEvent.objectType)
                        {
                            case "VendingMachine":
                            case "FuelPuddle":
                                if (telemetryEvent.objectTypeAdditionalInfo != null && telemetryEvent.objectTypeAdditionalInfo.Length > 0)
                                {
                                    matchTimeline.Weapon = telemetryEvent.objectTypeAdditionalInfo[0].first;
                                }
                                matchTimeline.DamageCategory = telemetryEvent.objectType;
                                matchTimeline.DamageReason = telemetryEvent.objectTypeStatus;

                                matchTimeline.PlayerAccountId = telemetryEvent.character.accountId;
                                if (!matchTimeline.PlayerAccountId.StartsWith("ai.") && !matchTimeline.PlayerAccountId.StartsWith("Monster."))
                                {
                                    matchTimeline.PlayerGuid = player.LookupPlayer(matchTimeline.PlayerAccountId);
                                }
                                else
                                {
                                    matchTimeline.PlayerIsNPC = true;
                                }
                                if (telemetryEvent.character.zone != null && telemetryEvent.character.zone.Length > 0)
                                {
                                    matchTimeline.Zone = ArrayToCsv(telemetryEvent.character.zone);
                                }
                                eventProcessed = true;
                                break;

                            case "Door":
                            case "JerryCan":
                            case "Caraudio":
                            case "FireworkLauncher":
                            case "Ascender":
                            case "LockedDoor":
                            case "ZiplinegunRope":
                            case "Satellite":
                            case "Cartoplights":
                                break; // Skip these

                            default:
                                break;  // Unknown
                        }

                        break;

                    case "LogPlayerAttack":
                        /*
                         * "attackId":             int,
                        "fireWeaponStackCount": int,
                        "attacker":             {Character},
                        "attackType":           string,
                        "weapon":               {Item},
                        "vehicle":              {Vehicle}*/

                        if (telemetryEvent.attacker != null)
                        {
                            matchTimeline.PlayerAccountId = telemetryEvent.attacker.accountId;
                            if (!matchTimeline.PlayerAccountId.StartsWith("ai.") && !matchTimeline.PlayerAccountId.StartsWith("Monster."))
                            {
                                matchTimeline.PlayerGuid = player.LookupPlayer(matchTimeline.PlayerAccountId);
                            }
                            else
                            {
                                if (telemetryEvent.attacker.name == "Guard" || telemetryEvent.attacker.name == "Commander")
                                {
                                    // Guard are a type of AI, so prepend Guard
                                    matchTimeline.PlayerAccountId = telemetryEvent.attacker.name + "." + matchTimeline.PlayerAccountId;
                                }

                                matchTimeline.PlayerIsNPC = true;
                            }

                            if (telemetryEvent.attacker.zone != null && telemetryEvent.attacker.zone.Length > 0)
                            {
                                matchTimeline.Zone = ArrayToCsv(telemetryEvent.attacker.zone);
                            }
                        }
                        //if (telemetryEvent.killerDamageInfo != null)
                        //{
                        matchTimeline.DamageReason = telemetryEvent.damageReason;
                        matchTimeline.DamageCategory = telemetryEvent.damageTypeCategory;
                        matchTimeline.Distance = telemetryEvent.distance;
                        matchTimeline.Weapon = telemetryEvent.damageCauserName;


                        Console.WriteLine(telemetryEvent.attackType);
                        matchTimeline.Weapon = telemetryEvent.weapon.itemId;
                        eventProcessed = true;
                        break;

                    case "LogPlayerTakeDamage":

                        /*
                         * "attackId":           int,
        "attacker":           {Character},
        "victim":             {Character},
        "damageTypeCategory": string,
        "damageReason":       string,
        "damage":             number,        // 1.0 damage = 1.0 health
                                     // Net damage after armor; damage to health
        "damageCauserName":   string,
        "isThroughPenetrableWall" bool*/
                        if (telemetryEvent.attacker != null)
                        {
                            if (!string.IsNullOrEmpty(telemetryEvent.attacker.accountId))
                            {
                                matchTimeline.SecondaryPlayerAccountId = telemetryEvent.attacker.accountId;
                                if (!matchTimeline.SecondaryPlayerAccountId.StartsWith("ai.") && !matchTimeline.SecondaryPlayerAccountId.StartsWith("Monster."))
                                {
                                    matchTimeline.SecondaryPlayerGuid = player.LookupPlayer(matchTimeline.SecondaryPlayerAccountId);
                                }
                                else
                                {
                                    if (telemetryEvent.attacker.name == "Guard" ||
                                    telemetryEvent.attacker.name == "Commander")
                                    {
                                        // Guard are a type of AI, so prepend Guard
                                        matchTimeline.SecondaryPlayerAccountId = telemetryEvent.attacker.name + "." + matchTimeline.SecondaryPlayerAccountId;
                                    }
                                    matchTimeline.SecondaryPlayerIsNPC = true;
                                }
                            }
                        }
                        matchTimeline.PlayerAccountId = telemetryEvent.victim.accountId;
                        if (!matchTimeline.PlayerAccountId.StartsWith("ai.") && !matchTimeline.PlayerAccountId.StartsWith("Monster."))
                        {
                            matchTimeline.PlayerGuid = player.LookupPlayer(matchTimeline.PlayerAccountId);
                        }
                        else
                        {
                            if (telemetryEvent.victim.name == "Guard" || telemetryEvent.victim.name == "Commander")
                            {
                                // Guard are a type of AI, so prepend Guard
                                matchTimeline.PlayerAccountId = telemetryEvent.victim.name + "." + matchTimeline.PlayerAccountId;
                            }

                            matchTimeline.PlayerIsNPC = true;
                        }
                        //if (telemetryEvent.killerDamageInfo != null)
                        //{
                        matchTimeline.DamageReason = telemetryEvent.damageReason;
                        matchTimeline.DamageCategory = telemetryEvent.damageTypeCategory;
                        matchTimeline.Distance = telemetryEvent.distance;
                        matchTimeline.Weapon = telemetryEvent.damageCauserName;
                        //}

                        if (telemetryEvent.victim.zone != null && telemetryEvent.victim.zone.Length > 0)
                        {
                            matchTimeline.Zone = ArrayToCsv(telemetryEvent.victim.zone);
                        }

                        eventProcessed = true;
                        break;

                    case "LogPlayerUseThrowable":
                    case "LogMatchDefinition":
                    case "LogPlayerLogin":
                    case "LogPlayerCreate":
                    case "LogPlayerPosition":
                    case "LogVaultStart":
                    case "LogItemEquip":
                    case "LogItemDetach":
                    case "LogItemAttach":
                    case "LogItemUnequip":
                    case "LogGameStatePeriodic":
                    case "LogItemDrop":
                    case "LogWeaponFireCount":
                    case "LogSwimStart":
                    case "LogSwimEnd":
                    case "LogArmorDestroy":
                    case "LogItemPickup":
                    case "LogItemPickupFromLootBox":
                    case "LogObjectDestroy":
                    case "LogItemPickupFromCarepackage":
                    case "LogItemPickupFromCustomPackage":
                    case "LogPlayerLogout":
                    case "LogVehicleRide":
                    case "LogVehicleDamage":
                    case "LogHeal":
                    case "LogItemPutToVehicleTrunk":
                    case "LogItemPickupFromVehicleTrunk":
                    case "LogPlayerDestroyProp":
                    case "LogSpecialZoneInCharacters":
                        break;

                    // Items generated by copilot
                    case "LogPlayerUseElectricVehicle":
                    case "LogElectricVehicleRide":
                    case "LogElectricVehicleLeave":
                    case "LogElectricVehicleDestroy":

                    case "LogVehicleEnter":
                    case "LogPlayerKill": // old, V2 available
                    case "LogPlayerDeath": // no in doco
                    case "LogParachuteLandingV2":
                    case "LogParachuteJumpV2":
                    case "LogParachuteDeploy":
                    case "LogParachuteJump":
                        Console.WriteLine($"Event Type: {telemetryEvent._T}, Timestamp: {telemetryEvent._D}");
                        break;

                    default:
                        // Handle unknown event type
                        Console.WriteLine($"Event Type: {telemetryEvent._T}, Timestamp: {telemetryEvent._D}");
                        break;
                }
                // Process each telemetry event as needed
                // For example, you can log the event type and timestamp
                //Console.WriteLine($"Event Type: {telemetryEvent._T}, Timestamp: {telemetryEvent._D}");

                if (eventProcessed)
                {
                    // Save to database
                    DbContext.MatchTimeline.Add(matchTimeline);
                }
            }
            DbContext.SaveChanges();
        }

        private int CalculateScore(int? rank, int? killPlace, int? damagePlace)
        {
            int score = 1; // Participation point

            // Win poins, 10 for first, 9 for second, etc.
            if (rank.HasValue && rank.Value <= 10)
            {
                score += 11 - rank.Value; // Assuming rank is 1-based (1st place = 10 points, 2nd place = 9 points, etc.)
            }

            // Kill points, nothing for the moment

            // Damage points, 10 points for first, 9 for second, etc.
            if (damagePlace.HasValue && damagePlace <= 10)
            {
                score += 11 - damagePlace.Value;
            }

            return score;
        }

        private void SetMatchDetails(Guid matchGuid, string weatherId, string blueZoneCustomOptions)
        {
            // Update the match
            DbContext.Database.EnsureCreated();

            // Check if the match exists in the database
            var match = DbContext.Matches.FirstOrDefault(m => m.MatchGuid == matchGuid);
            if (match != null)
            {
                match.Weather = weatherId;
                match.BlueZoneSettings = blueZoneCustomOptions;

                DbContext.SaveChanges();

                if (!DbContext.MatchBlueZone.Any(mbz => mbz.MatchGuid == matchGuid))
                {
                    var blueZoneInfo = _integrationService.ParseMatchBlueZone(blueZoneCustomOptions);

                    foreach (var zone in blueZoneInfo)
                    {
                        var blueZone = new Data.Models.MatchBlueZone();
                        {
                            blueZone.MatchBlueZoneGuid = Guid.NewGuid();
                            blueZone.MatchGuid = matchGuid;
                            blueZone.BlueZonePhase = zone.blueZonePhase;
                            blueZone.StartDelay = zone.startDelay;
                            blueZone.WarningDuration = zone.warningDuration;
                            blueZone.ReleaseDuration = zone.releaseDuration;
                            blueZone.BlueZoneDamagePerSecond = zone.blueZoneDamagePerSecond;
                            blueZone.RadiusRate = zone.radiusRate;
                            blueZone.SpreadRatio = zone.spreadRatio;
                            blueZone.LandRatio = zone.landRatio;
                            blueZone.CircleAlgorithm = zone.circleAlgorithm;
                        }

                        DbContext.MatchBlueZone.Add(blueZone);
                    }

                    DbContext.SaveChanges();
                }
            }
        }

        public string ArrayToCsv(string[] array)
        {
            return string.Join(",", array);
        }
    }
}
