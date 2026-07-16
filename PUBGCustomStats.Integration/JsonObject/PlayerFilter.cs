using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUBGCustomStats.Integration.JsonObject
{
    public class PlayerFilter : JsonBaseClass
    {
        public PlayerDatum[]? data { get; set; }
        public Links? links { get; set; }
        public Meta? meta { get; set; }
    }


    public class PlayerDatum
    {
        public string? type { get; set; }
        public string? id { get; set; }
        public Attributes? attributes { get; set; }
        public Relationships? relationships { get; set; }
        public Links? links { get; set; }
    }

    public class Attributes
    {
        public string? name { get; set; }
        public object? stats { get; set; }
        public string? titleId { get; set; }
        public string? shardId { get; set; }
        public string? patchVersion { get; set; }
        public string? banType { get; set; }
        public string? clanId { get; set; }
    }

    public class Relationships
    {
        public Assets? assets { get; set; }
        public Matches? matches { get; set; }
    }


    public class Matches
    {
        public Datum[]? data { get; set; }
    }

}
