using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUBGCustomStats.Integration.JsonObject
{
    public class PlayerDirect : JsonBaseClass
    {
        public PlayerDatum? data { get; set; }
        public Links? links { get; set; }
        public Meta? meta { get; set; }
    }

}
