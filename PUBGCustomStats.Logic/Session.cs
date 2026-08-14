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

        public Guid CreateSession(string sessionName, DateTime sessionTime, Guid currentSeason)
        {
            var session = new Data.Models.Session
            {
                SessionName = sessionName,
                StartDateTime = sessionTime,
                SeasonGuid = currentSeason
            };
            DbContext.Sessions.Add(session);
            DbContext.SaveChanges();

            return session.SessionGuid;
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

        public void DeleteSession(Guid sessionGuid)
        {
            var session = DbContext.Sessions.FirstOrDefault(s => s.SessionGuid == sessionGuid);
            if (session == null)
            {
                throw new Exception("Session not found.");
            }

            // Find all matches for the session and delete related child entities first
            var matches = DbContext.Matches.Where(m => m.SessionGuid == sessionGuid).ToList();

            foreach (var match in matches)
            {
                // Delete MatchPlayerStats
                var playerStats = DbContext.MatchPlayerStats.Where(mps => mps.MatchGuid == match.MatchGuid);
                DbContext.MatchPlayerStats.RemoveRange(playerStats);

                // Delete MatchTimelinePlayer entries related to timelines for this match
                var timelines = DbContext.MatchTimeline.Where(mt => mt.MatchGuid == match.MatchGuid).ToList();
                foreach (var timeline in timelines)
                {
                    var timelinePlayers = DbContext.MatchTimelinePlayer.Where(mtp => mtp.MatchTimelineGuid == timeline.MatchTimelineGuid);
                    DbContext.MatchTimelinePlayer.RemoveRange(timelinePlayers);
                }

                // Delete timelines
                DbContext.MatchTimeline.RemoveRange(timelines);

                // Delete blue zone entries
                var blueZones = DbContext.MatchBlueZone.Where(bz => bz.MatchGuid == match.MatchGuid);
                DbContext.MatchBlueZone.RemoveRange(blueZones);

                // Finally delete the match itself
                DbContext.Matches.Remove(match);
            }

            // Remove the session
            DbContext.Sessions.Remove(session);

            DbContext.SaveChanges();
        }
    }
}
