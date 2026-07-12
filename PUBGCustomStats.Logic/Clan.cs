using Microsoft.EntityFrameworkCore;
using PUBGCustomStats.Data;
using PUBGCustomStats.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUBGCustomStats.Logic
{
    public class Clan
    {
        public PUBGCustomStatsContext DbContext { get; set; }
        private DbContextOptions<PUBGCustomStatsContext> _dbContextOptions { get; set; }
        private IntegrationService integrationService;

        public Clan(DbContextOptions<PUBGCustomStatsContext> options, IntegrationService integrationService)
        {
            _dbContextOptions = options;
            this.integrationService = integrationService;
            // Initialize the DbContext with the required options
            DbContext = new PUBGCustomStatsContext(options);


        }

        public Guid LookupClan(string pubgClanId)
        {
            Console.WriteLine($"Looking up clan: {pubgClanId}");
            // Ensure the database is created
            DbContext.Database.EnsureCreated();
            // Check if the clan exists in the database
            var clan = DbContext.Clans.FirstOrDefault(c => c.PUBGClanId == pubgClanId);
            if (clan != null)
            {
                Console.WriteLine($"Clan found: {clan.ClanName}, GUID: {clan.ClanGuid}");
            }
            else
            {
                Console.WriteLine("Clan not found locally, creating new clan");
         
                try
                {
                    var clanData = integrationService.GetClan(pubgClanId);
                    if (clanData != null)
                    {
                        clan = new Data.Models.Clan
                        {
                            RawData = clanData.RawData,
                            PUBGClanId = pubgClanId,
                            ClanTag = clanData.data.attributes.clanTag,
                            ClanName = clanData.data.attributes.clanName,
                            ClanGuid = Guid.NewGuid()
                        };
                        // Add the clan to the database
                        DbContext.Clans.Add(clan);
                        DbContext.SaveChanges();
                        Console.WriteLine($"Clan added: {clan.ClanName}, GUID: {clan.ClanGuid}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching match data: {ex.Message}");
                }
            }
            return clan.ClanGuid;

        }
    }
}

