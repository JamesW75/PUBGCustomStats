using Microsoft.EntityFrameworkCore;
using PUBGCustomStats.Data;

namespace PUBGCustomStats.Logic
{
    public class Season
    {
        public PUBGCustomStatsContext DbContext { get; set; }
        private DbContextOptions<PUBGCustomStatsContext> _dbContextOptions { get; set; }

        public Season(DbContextOptions<PUBGCustomStatsContext> options)
        {
            _dbContextOptions = options;
            DbContext = new PUBGCustomStatsContext(_dbContextOptions);
        }

        public void CreateSeason(string seasonName)
        {
            var season = new Data.Models.Season
            {
                SeasonName = seasonName,
                //                StartDate = startDate,
                //                EndDate = endDate
                IsCurrentSeason = true
            };
            DbContext.Seasons.Add(season);
            DbContext.SaveChanges();
        }

        public Guid? GetCurrentSeason()
        {
            var season = DbContext.Seasons.FirstOrDefault(s => s.IsCurrentSeason);
            return season?.SeasonGuid;
        }

        public List<Data.Models.Season> ListSeasons()
        {
            return DbContext.Seasons.ToList();
        }

        public void EditSeason(Guid seasonGuid, string newSeasonName)
        {
            var season = DbContext.Seasons.FirstOrDefault(s => s.SeasonGuid == seasonGuid);
            if (season != null)
            {
                season.SeasonName = newSeasonName;
                DbContext.SaveChanges();
            }
            else
            {
                throw new Exception("Season not found.");
            }
        }
    }
}