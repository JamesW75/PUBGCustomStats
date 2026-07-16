using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUBGCustomStats.Integration.JsonObject
{
    public class Match : JsonBaseClass
    {
        public Data? data { get; set; }
        public Included[]? included { get; set; }
        public Links? links { get; set; }
        public Meta? meta { get; set; }
    }

    public class Data
    {
        public string? type { get; set; }
        public string? id { get; set; }
        public MatchAttributes? attributes { get; set; }
        public MatchRelationships? relationships { get; set; }
        public Links? links { get; set; }
    }

    public class MatchAttributes
    {
        public bool isCustomMatch { get; set; }
        public string? matchType { get; set; }
        public string? seasonState { get; set; }
        public DateTime createdAt { get; set; }
        public int duration { get; set; }
        public object? stats { get; set; }
        public string? gameMode { get; set; }
        public object? tags { get; set; }
        public string? titleId { get; set; }
        public string? shardId { get; set; }
        public string? mapName { get; set; }
    }

    public class MatchRelationships
    {
        public Rosters? rosters { get; set; }
        public Assets? assets { get; set; }
    }

    public class Rosters
    {
        public Datum[]? data { get; set; }
    }


    public class Included
    {
        public string? type { get; set; }
        public string? id { get; set; }
        public Attributes1? attributes { get; set; }
        public Relationships1? relationships { get; set; }
    }

    public class Attributes1
    {
        public Stats? stats { get; set; }
        public string? actor { get; set; }
        public string? shardId { get; set; }
        public string? won { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public DateTime createdAt { get; set; }
        public string? URL { get; set; }
    }

    public class Stats
    {
        public int DBNOs { get; set; }
        public int assists { get; set; }
        public int boosts { get; set; }
        public float damageDealt { get; set; }
        public string? deathType { get; set; }
        public int headshotKills { get; set; }
        public int heals { get; set; }
        public int killPlace { get; set; }
        public int killStreaks { get; set; }
        public int kills { get; set; }
        public float longestKill { get; set; }
        public string? name { get; set; }
        public string? playerId { get; set; }
        public int revives { get; set; }
        public float rideDistance { get; set; }
        public int roadKills { get; set; }
        public float swimDistance { get; set; }
        public int teamKills { get; set; }
        public int timeSurvived { get; set; }
        public int vehicleDestroys { get; set; }
        public float walkDistance { get; set; }
        public int weaponsAcquired { get; set; }
        public int winPlace { get; set; }
        public int rank { get; set; }
        public int teamId { get; set; }
    }

    public class Relationships1
    {
        public Team? team { get; set; }
        public Participants? participants { get; set; }
    }

    public class Team
    {
        public object? data { get; set; }
    }

    public class Participants
    {
        public Datum[]? data { get; set; }
    }

}
