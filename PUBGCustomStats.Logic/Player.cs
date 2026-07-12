using Microsoft.EntityFrameworkCore;
using PUBGCustomStats.Data;
using PUBGCustomStats.Integration;

namespace PUBGCustomStats.Logic
{
    public class Player
    {
        // Add db context
        public PUBGCustomStatsContext DbContext { get; set; }
        private DbContextOptions<PUBGCustomStatsContext> _dbContextOptions { get; set; }
        private IntegrationService integrationService;

        public Player(DbContextOptions<PUBGCustomStatsContext> options, IntegrationService integrationService)
        {
            _dbContextOptions = options;
            this.integrationService = integrationService;
            // Initialize the DbContext with the required options
            DbContext = new PUBGCustomStatsContext(options);
        }

        public void LookupPlayers()
        {
            Console.WriteLine("Looking up players");
            // Ensure the database is created
            DbContext.Database.EnsureCreated();
            // Check if the player exists in the database
            var players = DbContext.Players.ToList();

            foreach (var player in players)
            {
                LookupPlayer(player.PUBGPlayerId);
            }
        }
        public Guid LookupPlayer(string pubgPlayerId)
        {
            if (string.IsNullOrEmpty(pubgPlayerId))
            {
                throw new ArgumentException("Player ID cannot be null or empty", nameof(pubgPlayerId));
            }

            Console.WriteLine($"Looking up player: {pubgPlayerId}");
            // Ensure the database is created
            DbContext.Database.EnsureCreated();
            // Check if the player exists in the database
            var player = DbContext.Players.FirstOrDefault(p => p.PUBGPlayerId == pubgPlayerId);
            if (player != null && player.RawData != null )
            {
                Console.WriteLine($"Player found: {player.PlayerName}, GUID: {player.PlayerGuid}");
            }
            else
            {
                Console.WriteLine("Player not found locally, checking API");

                var playerData = integrationService.GetPlayer(pubgPlayerId);
                if (playerData != null)
                {
                    if (player == null)
                    {
                        player = new Data.Models.Player();

                        // Add the player to the database
                        DbContext.Players.Add(player);
                    }
                    player.PUBGPlayerId = playerData.data.id;
                    player.PlayerName = playerData.data.attributes.name;
                    player.PlayerConsole = playerData.data.attributes.shardId;
                    player.RawData = playerData.RawData;

                    var clan = new Clan(_dbContextOptions, integrationService);
                    if (!string.IsNullOrEmpty(playerData.data.attributes.clanId))
                    {
                        player.ClanGuid = clan.LookupClan(playerData.data.attributes.clanId);
                    }


                    DbContext.SaveChanges();
                    Console.WriteLine($"Player added: {player.PlayerName}, GUID: {player.PlayerGuid}");
                }
            }

            return player.PlayerGuid;
        }

        public Guid LookupPlayer(string playerName, string console)
        {
            Console.WriteLine($"Looking up player: {playerName} on console: {console}");

            // Ensure the database is created
            DbContext.Database.EnsureCreated();
            // Check if the player exists in the database
            var player = DbContext.Players.FirstOrDefault(p => p.PlayerName == playerName && p.PlayerConsole == console);
            if (player != null && player.RawData != null )
            {
                Console.WriteLine($"Player found: {player.PlayerName}, GUID: {player.PlayerGuid}");
            }
            else
            {
                Console.WriteLine("Player not found locally, checking API");

                var playerData = integrationService.GetPlayer(playerName, console);
                if (playerData != null)
                {
                    if (player == null)
                    {
                        player = new Data.Models.Player();
                        // Add the player to the database
                        DbContext.Players.Add(player);
                    }

                    var dataAttribute = playerData.data.First();

                    player.PUBGPlayerId = dataAttribute.id;
                    player.PlayerName = dataAttribute.attributes.name;
                    player.PlayerConsole = dataAttribute.attributes.shardId;
                    player.RawData = playerData.RawData;

                    var clan = new Clan(_dbContextOptions, integrationService);
                    if (!string.IsNullOrEmpty(dataAttribute.attributes.clanId))
                    {
                        player.ClanGuid = clan.LookupClan(dataAttribute.attributes.clanId);
                    }

                    DbContext.SaveChanges();
                    Console.WriteLine($"Player added: {player.PlayerName}, GUID: {player.PlayerGuid}");
                }
            }

            return player.PlayerGuid;
        }
    }
}
