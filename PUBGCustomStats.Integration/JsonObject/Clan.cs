using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUBGCustomStats.Integration.JsonObject
{
    public class Clan : JsonBaseClass

    {
        public ClanData? data { get; set; }
        public Links? links { get; set; }
        public Meta? meta { get; set; }
    }

    public class ClanData
    {
        public string? type { get; set; }
        public string? id { get; set; }
        public ClanAttributes? attributes { get; set; }
    }

    public class ClanAttributes
    {
        public string? clanName { get; set; }
        public string? clanTag { get; set; }
        public int? clanLevel { get; set; }
        public int? clanMemberCount { get; set; }
    }

}
