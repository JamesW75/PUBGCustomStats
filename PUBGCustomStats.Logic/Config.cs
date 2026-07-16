using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PUBGCustomStats.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace PUBGCustomStats.Logic
{
    public class Config
    {
        public PUBGCustomStatsContext DbContext { get; set; }
        private DbContextOptions<PUBGCustomStatsContext> _dbContextOptions { get; set; }

        public Config(DbContextOptions<PUBGCustomStatsContext> dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
            DbContext = new PUBGCustomStatsContext(_dbContextOptions);
        }

        public void SetAPIKey(string apiKey)
        {
            var config = DbContext.Config.FirstOrDefault(c => c.Key == "APIKey");
            if (config == null)
            {
                config = new PUBGCustomStats.Data.Models.Config { Key = "APIKey", Value = apiKey };
                DbContext.Config.Add(config);
            }
            else
            {
                config.Value = apiKey;
                DbContext.Config.Update(config);
            }

            DbContext.SaveChanges();
        }

        public string? GetAPIKey()
        {
            return DbContext.Config.FirstOrDefault(c => c.Key == "APIKey")?.Value;
        }
    }
}
