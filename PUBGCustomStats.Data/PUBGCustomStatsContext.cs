using Microsoft.EntityFrameworkCore;
using PUBGCustomStats.Data.Models;

namespace PUBGCustomStats.Data
{
    public class PUBGCustomStatsContext : DbContext
    {
        public PUBGCustomStatsContext(DbContextOptions<PUBGCustomStatsContext> options)
            : base(options)
        {

        }


        // Define DbSet properties for your entities
        public DbSet<Config> Config { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Season> Seasons { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Clan> Clans { get; set; }
        public DbSet<MatchPlayerStat> MatchPlayerStats { get; set; }
        public DbSet<MatchTimeline> MatchTimeline { get; set; }
        public DbSet<MatchTimelinePlayer> MatchTimelinePlayer { get; set; }
        
        public DbSet<MatchBlueZone> MatchBlueZone { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure your entity mappings here
            modelBuilder.Entity<Config>().ToTable("Config").HasKey(c => c.Key);
            modelBuilder.Entity<Player>().ToTable("Player");
            modelBuilder.Entity<Player>().HasKey(p => p.PlayerGuid);
            modelBuilder.Entity<Player>().HasOne(c => c.Clan)
                .WithMany(c => c.Players)
                .HasForeignKey(p => p.ClanGuid);
            modelBuilder.Entity<Player>().HasMany(m => m.MatchPlayerStats)
                                .WithOne(mps => mps.Player)
                .HasForeignKey(mps => mps.PlayerGuid);
            modelBuilder.Entity<Player>().HasMany(m => m.MatchTimelines)
                .WithOne(mt => mt.Player)
                .HasForeignKey(mt => mt.PlayerGuid);

            modelBuilder.Entity<Match>().ToTable("Match").HasKey(m => m.MatchGuid);
            modelBuilder.Entity<Match>().HasOne(m => m.Session)
                .WithMany(s => s.Matches)
                .HasForeignKey(m => m.SessionGuid);
            modelBuilder.Entity<Match>().HasMany(m => m.MatchPlayerStats)
                .WithOne(mps => mps.Match)
                .HasForeignKey(mps => mps.MatchGuid);
            modelBuilder.Entity<Match>().HasMany(m => m.MatchTimelines)
                .WithOne(mt => mt.Match)
                .HasForeignKey(mt => mt.MatchGuid);

            modelBuilder.Entity<MatchPlayerStat>().ToTable("MatchPlayerStat").HasKey(mps => mps.MatchPlayerStatGuid);

            modelBuilder.Entity<MatchBlueZone>().ToTable("MatchBlueZone").HasKey(mps => mps.MatchBlueZoneGuid);

            modelBuilder.Entity<MatchTimeline>().ToTable("MatchTimeline").HasKey(mt => mt.MatchTimelineGuid);
            modelBuilder.Entity<MatchTimeline>()
                .HasOne(mt => mt.Match);
            modelBuilder.Entity<MatchTimeline>()
                  .HasOne(mt => mt.Player)
                  .WithMany(p => p.MatchTimelines)
                  .HasForeignKey(fk => fk.PlayerGuid);
            modelBuilder.Entity<MatchTimeline>()
                .HasOne(mt => mt.SecondaryPlayer)
                .WithMany(p => p.MatchTimelinesAsKiller).HasForeignKey(fk => fk.SecondaryPlayerGuid);
            modelBuilder.Entity<Season>().ToTable("Season").HasKey(s => s.SeasonGuid);
            modelBuilder.Entity<Session>().ToTable("Session").HasKey(s => s.SessionGuid);
            modelBuilder.Entity<Session>().HasOne(s => s.Season)
                .WithMany(se => se.Sessions)
                .HasForeignKey(s => s.SeasonGuid);
            modelBuilder.Entity<Clan>().ToTable("Clan").HasKey(c => c.ClanGuid);

            modelBuilder.Entity<MatchTimelinePlayer>().ToTable("MatchTimelinePlayer").HasKey(mtp => mtp.MatchTimelinePlayerGuid);
            modelBuilder.Entity<MatchTimelinePlayer>().ToTable("MatchTimelinePlayer")
                .HasOne(mtp => mtp.Player).WithMany(p => p.MatchTimelinePlayers).HasForeignKey(mtp => mtp.PlayerGuid);
            modelBuilder.Entity<MatchTimelinePlayer>().ToTable("MatchTimelinePlayer")
                .HasOne(mtp => mtp.MatchTimeline).WithMany(p => p.MatchTimelinePlayers).HasForeignKey(mtp => mtp.MatchTimelineGuid);
            /*
            modelBuilder.Entity<MatchRawData>().ToTable("MatchRawData").HasKey(mrd => mrd.MatchGuid);
            modelBuilder.Entity<MatchRawData>().ToTable("MatchRawData")
                .HasOne(mrd => mrd.Match)
                .WithOne(m => m.MatchRawData)
                .HasForeignKey<MatchRawData>(mrd => mrd.MatchGuid);
            */
        }

        // Define DbSet properties for your entities
        // public DbSet<Player> Players { get; set; }
        // Add other DbSet properties as needed

    }
}
