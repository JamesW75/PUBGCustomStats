using Microsoft.EntityFrameworkCore;
using PUBGCustomStats.Data;

namespace PUBGCustomStats.Logic
{
    public class Session
    {
        public PUBGCustomStatsContext DbContext { get; set; }
        private DbContextOptions<PUBGCustomStatsContext> _dbContextOptions { get; set; }

        public Session(DbContextOptions<PUBGCustomStatsContext> options)
        {
            _dbContextOptions = options;
            DbContext = new PUBGCustomStatsContext(_dbContextOptions);
        }

        public void CreateSession(string sessionName, DateTime sessionTime, Guid currentSeason)
        {
            var session = new Data.Models.Session
            {
                SessionName = sessionName,
                StartDateTime = sessionTime,
                SeasonGuid = currentSeason
            };
            DbContext.Sessions.Add(session);
            DbContext.SaveChanges();
        }

        public Guid GetCurrentSession()
        {
            var currentSession = DbContext.Sessions.OrderByDescending(s => s.StartDateTime).FirstOrDefault();
            if (currentSession != null)
            {
                return currentSession.SessionGuid;
            }
            else
            {
                throw new Exception("No sessions found in the database.");
            }
        }

        public List<Data.Models.Session> ListSessions(Guid seasonGuid)
        {
            return DbContext.Sessions.Where(s => s.SeasonGuid == seasonGuid).ToList();
        }

        public void EditSession(Guid sessionGuid, string newSessionName, DateTime newSessionTime)
        {
            var session = DbContext.Sessions.FirstOrDefault(s => s.SessionGuid == sessionGuid);
            if (session != null)
            {
                session.SessionName = newSessionName;
                session.StartDateTime = newSessionTime;
                DbContext.SaveChanges();
            }
            else
            {
                throw new Exception("Session not found.");
            }
        }
    }
}
