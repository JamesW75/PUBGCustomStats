using System;
using System.Collections.Generic;
using System.Text;

namespace PUBGCustomStats.Data.Models
{
    public class Config
    {
        public required string Key { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}
