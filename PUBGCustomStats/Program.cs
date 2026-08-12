using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PUBGCustomStats.Data;
using PUBGCustomStats.Integration;
using PUBGCustomStats.Logic;

// Console application that builds stats from custom PUBG matches and players.
// Uses the PUBG API to retrieve data and stores it in a local SQLite database.
// The application is driven by command-line options listed below.
/* Possible command line arguments
 * --help                                Show this help message
 * --setup                               Initialise the database and create the tables
 * --apikey <key>                        Set the PUBG API key
 * --createseason <name>                 Create a new season in the database
 * --createsession <name> <datetime>     Create a new session for the current season
 * --editsession <sessionGuid> <newName> <newDateTime>  Edit a session
 * --editseason <seasonGuid> <newName>   Edit the specified season
 * --deletesession <sessionGuid>         Delete a session and all associated matches and data
 * --addmatch <matchId> [matchName]      Add a match to the current session (matchName optional)
 * --editmatch <matchId> <newMatchName>  Edit a match name
 * --movematch <matchId> <sessionGuid>   Move a match to a different session
 * --listmatches                         List all matches in the current session
 * --listsessions                        List all sessions in the current season
 * --listseasons                         List all seasons in the database
 * --deletematch <matchId>               Delete a match from the current session
 * --excludematch <matchId>              Mark a match as excluded (DoNotCount = true)
 * --includematch <matchId>              Mark a match as included (DoNotCount = false)
 * --getmatches <gamerTag>               Get recent matches for a player
 * --setrandom <playerId>                Mark the specified player as random in the database
 * --cleanup                             Delete players with no matches and clans with no players
 */

// Process command line arguments
//
if (args.Length > 0)
{
    // Add db context configured for SQLite
    var optionsBuilder = new DbContextOptionsBuilder<PUBGCustomStatsContext>();

    var configurationBuilder = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

    var connectionString = configurationBuilder.Build().GetConnectionString("PUBGCustomStatsContext");

    if (string.IsNullOrEmpty(connectionString))
    {
        Console.WriteLine("Error: Connection string is not set in appsettings.json.");
        return;
    }
    connectionString = connectionString.Replace("{AppDataPath}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

    //    optionsBuilder.UseSqlite("Data Source={AppDataPath}\\PUBGCustomStats\\PUBGCustomStats.db");
    optionsBuilder.UseSqlite(connectionString);
    var dbContextOptions = optionsBuilder.Options;

    if (args[0].ToLower() == "--setup")
    {
        // Create the database and tables
        var dbContext = new PUBGCustomStatsContext(dbContextOptions);
        Console.WriteLine("Creating database and tables...");

        Console.WriteLine("Database location: " + connectionString);

        if (dbContext.Database.EnsureCreated())
        {
            Console.WriteLine("Database and tables created successfully.");
        }
        else
        {
            Console.WriteLine("Database already exists.");
        }

        Console.WriteLine($"Database location: {connectionString}");

        // Check if the API key is set, if not prompt the user to set it
        var apiKey = new Config(dbContextOptions).GetAPIKey();
        if (apiKey == null || string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("API key is not set. Please set the API key using the --apikey option.");
        }

    }
    else if (args[0].ToLower() == "--apikey")
    {
        // Set the API key
        if (args.Length < 2)
        {
            Console.WriteLine("Error: No API key provided. Use --help for usage information.");
            return;
        }

        var config = new Config(dbContextOptions);

        config.SetAPIKey(args[1]);

        Console.WriteLine($"API key set to: {args[1]}");
    }
    else
    {
        // Check if the API key is set, if not prompt the user to set it
        var apiKey = new Config(dbContextOptions).GetAPIKey();

        if (apiKey == null || string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("Error: API key is not set. Please set the API key using the --apikey option.");
            return;
        }
        var integrationService = new IntegrationService(apiKey);

        // Objecs from he logic class
        var season = new Season(dbContextOptions);
        var session = new Session(dbContextOptions);

        var currentSeason = season.GetCurrentSeason();


        switch (args[0].ToLower())
        {

            case "--createseason":

                // Create a new season in the database
                if (args.Length < 2)
                {
                    Console.WriteLine("Error: No season name provided. Use --help for usage information.");
                    return;
                }
                var seasonName = args[1];

                season.CreateSeason(seasonName);
                Console.WriteLine($"Season created: {seasonName}");
                break;

            case "--cleanup":
                {
                    // Remove players with no match stats and clans with no players
                    var db = new PUBGCustomStatsContext(dbContextOptions);
                    db.Database.EnsureCreated();

                    // Remove matchbluezone entries with no match (old orphaned data, before foreign keys fixed)
                    var matchBlueZonesToDelete = db.MatchBlueZone
                        .Where(mbz => mbz.Match == null)
                        .ToList();

                    Console.WriteLine($"Found {matchBlueZonesToDelete.Count} orphaned matchbluezone entries.");
                   /* if (matchBlueZonesToDelete.Count > 0)
                    {
                        foreach (var mbz in matchBlueZonesToDelete)
                        {
                            Console.WriteLine($"Deleting orphaned matchbluezone entry: {mbz.MatchBlueZoneGuid}");
                            db.MatchBlueZone.Remove(mbz);
                        }
                        Console.WriteLine($"Deleted {matchBlueZonesToDelete.Count} orphaned matchbluezone entries.");
                    }*/

                    // Remove matchtimeline entries with no match (old orphaned data, before foreign keys fixed)
                    var matchTimelineToDelete = db.MatchTimeline
                        .Where(mt => mt.Match == null)
                        .ToList();

                    Console.WriteLine($"Found {matchTimelineToDelete.Count} orphaned matchtimeline entries.");
                    /*if (matchTimelineToDelete.Count > 0)
                    {
                        foreach (var mt in matchTimelineToDelete)
                        {
                            Console.WriteLine($"Deleting orphaned matchtimeline entry: {mt.MatchTimelineGuid}");
                            db.MatchTimeline.Remove(mt);
                        }
                        Console.WriteLine($"Deleted {matchTimelineToDelete.Count} orphaned matchtimeline entries.");
                    }*/

                    // Remove matchtimelineplayer entries with no matchtimeline (old orphaned data, before foreign keys fixed)
                    var matchTimelinePlayerToDelete = db.MatchTimelinePlayer
                        .Where(mtp => !db.MatchTimeline.Any(mt => mt.MatchTimelineGuid == mtp.MatchTimelineGuid))
                        .ToList();  

                    Console.WriteLine($"Found {matchTimelinePlayerToDelete.Count} orphaned matchtimelineplayer entries.");
                    /*if (matchTimelinePlayerToDelete.Count > 0)
                    {
                        foreach (var mtp in matchTimelinePlayerToDelete)
                        {           
                            Console.WriteLine($"Deleting orphaned matchtimelineplayer entry: {mtp.MatchTimelinePlayerGuid}");
                            db.MatchTimelinePlayer.Remove(mtp);
                        }
                        Console.WriteLine($"Deleted {matchTimelinePlayerToDelete.Count} orphaned matchtimelineplayer entries.");
                    }*/


                    // Find players that have no MatchPlayerStat entries
                    var playersToDelete = db.Players
                        .Where(p => !db.MatchPlayerStats.Any(mps => mps.PlayerGuid == p.PlayerGuid))
                        .ToList();

                    Console.WriteLine($"Found {playersToDelete.Count} players with no match stats.");
                    if (playersToDelete.Count > 0)
                    {
                        foreach (var p in playersToDelete)
                        {
                            // CHeck for player records which should already be deleted
                            var playerRecord = db.MatchPlayerStats.FirstOrDefault(pr => pr.PlayerGuid == p.PlayerGuid);
                            if (playerRecord != null)
                            {
                                Console.WriteLine($"Skipping player: {p.PlayerName} ({p.PlayerGuid}) - as they have MatchPlayerStats for match: {playerRecord.MatchGuid}");
                                continue;
                            }
                            var matchTimelineRecord = db.MatchTimeline.FirstOrDefault(mt => mt.PlayerGuid == p.PlayerGuid);
                            if (matchTimelineRecord != null)
                            {
                                Console.WriteLine($"Skipping player: {p.PlayerName} ({p.PlayerGuid}) - as they have MatchTimeline for match: {matchTimelineRecord.MatchGuid}");
                                continue;
                            }
                            var matchTimelineRecord2 = db.MatchTimeline.FirstOrDefault(mt => mt.SecondaryPlayerGuid == p.PlayerGuid);
                            if (matchTimelineRecord2 != null)
                            {
                                Console.WriteLine($"Skipping player: {p.PlayerName} ({p.PlayerGuid}) - as they have MatchTimeline for match: {matchTimelineRecord2.MatchGuid}");
                                continue;
                            }
                            var matchTimelinePlayerRecord = db.MatchTimelinePlayer.FirstOrDefault(mtp => mtp.PlayerGuid == p.PlayerGuid);
                            if (matchTimelinePlayerRecord != null)
                            {
                                Console.WriteLine($"Skipping player: {p.PlayerName} ({p.PlayerGuid}) - as they have MatchTimelinePlayer for matchtimeline: {matchTimelinePlayerRecord.MatchTimelineGuid}");
                                continue;
                            }
                            
                            Console.WriteLine($"Deleting player: {p.PlayerName} ({p.PlayerGuid})");
                            db.Players.Remove(p);
                            db.SaveChanges();
                        }
                        Console.WriteLine($"Deleted {playersToDelete.Count} players.");
                    }

                    // Find clans that have no players
                    var clansToDelete = db.Clans
                        .Where(c => !db.Players.Any(p => p.ClanGuid == c.ClanGuid))
                        .ToList();

                    Console.WriteLine($"Found {clansToDelete.Count} clans with no players.");
                    if (clansToDelete.Count > 0)
                    {
                        foreach (var c in clansToDelete)
                        {
                            Console.WriteLine($"Deleting clan: {c.ClanName} ({c.ClanGuid})");
                            db.Clans.Remove(c);
                        }
                        db.SaveChanges();
                        Console.WriteLine($"Deleted {clansToDelete.Count} clans.");
                    }
                }
                break;

            case "--createsession":
                // Create a new session for the current season
                if (args.Length < 3)
                {
                    Console.WriteLine("Error: No session name or time provided. Use --help for usage information.");
                    return;
                }
                var sessionName = args[1];
                var sessionTime = args[2];


                if (currentSeason == null)
                {
                    Console.WriteLine("Error: No current season found. Please create a season first using --createseason.");
                    return;
                }
                if (!DateTime.TryParse(sessionTime, out DateTime parsedSessionTime))
                {
                    Console.WriteLine("Error: Invalid session time format. Please use a valid date and time format (e.g., '2024-06-01 14:30').");
                    return;
                }

                var createdSessionGuid = session.CreateSession(sessionName, parsedSessionTime, currentSeason.Value);
                Console.WriteLine($"Session created: {sessionName}");
                Console.WriteLine($"Session GUID: {createdSessionGuid}");
                break;

            case "--editsession":
                // Edit the current session
                if (args.Length < 4)
                {
                    Console.WriteLine("Error: No session name or time provided. Use --help for usage information.");
                    return;
                }
                var editSessionGuid = args[1];
                var editSessionName = args[2];
                var editSessionTime = args[3];

                if (!Guid.TryParse(editSessionGuid, out Guid parsedSessionGuid))
                {
                    Console.WriteLine("Error: Invalid session GUID format. Please provide a valid GUID.");
                    return;
                }

                if (!DateTime.TryParse(editSessionTime, out DateTime parsedEditSessionTime))
                {
                    Console.WriteLine("Error: Invalid session time format. Please use a valid date and time format (e.g., '2024-06-01 14:30').");
                    return;
                }

                session.EditSession(parsedSessionGuid, editSessionName, parsedEditSessionTime);
                Console.WriteLine($"Session edited: {editSessionName}");
                break;

            case "--deletesession":
                // Delete a session and all associated matches and data
                if (args.Length < 2)
                {
                    Console.WriteLine("Error: No session GUID provided. Use --help for usage information.");
                    return;
                }

                var deleteSessionGuid = args[1];

                if (!Guid.TryParse(deleteSessionGuid, out Guid parsedDeleteSessionGuid))
                {
                    Console.WriteLine("Error: Invalid session GUID format. Please provide a valid GUID.");
                    return;
                }

                try
                {
                    session.DeleteSession(parsedDeleteSessionGuid);
                    Console.WriteLine($"Session deleted: {deleteSessionGuid}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting session: {ex.Message}");
                }
                break;

            case "--listsessions":
                if (currentSeason == null)
                {
                    Console.WriteLine("Error: No current season found. Please create a season first using --createseason.");
                    return;
                }
                var sessions = session.ListSessions(currentSeason.Value);
                Console.WriteLine("Current sessions:");
                Console.WriteLine($"   Session Guid                         | Start Date Time  | Session Name");
                Console.WriteLine($"   =====================================|==================|==================");
                foreach (var s in sessions)
                {
                    Console.WriteLine($" - {s.SessionGuid} | {s.StartDateTime.GetValueOrDefault(DateTime.MinValue).ToString("yyyy-MM-dd HH:mm")} | {s.SessionName}");
                }
                break;

            case "--listseasons":
                var seasons = season.ListSeasons();
                Console.WriteLine("Current seasons:");
                Console.WriteLine($"   Season Guid                          | Season Name");
                Console.WriteLine($"   =====================================|==================");
                foreach (var s in seasons)
                {
                    Console.WriteLine($" - {s.SeasonGuid} | {s.SeasonName}");
                }
                break;

            case "--editseason":

                // Edit the current season
                if (args.Length < 3)
                {
                    Console.WriteLine("Error: No season name provided. Use --help for usage information.");
                    return;
                }

                var seasonGuid = args[1];
                var editSeasonName = args[2];

                if (!Guid.TryParse(seasonGuid, out Guid parsedSeasonGuid))
                {
                    Console.WriteLine("Error: Invalid season GUID format. Please provide a valid GUID.");
                    return;
                }

                season.EditSeason(parsedSeasonGuid, editSeasonName);
                Console.WriteLine($"Season edited: {editSeasonName}");

                break;

            case "--addmatch":
                // Add a match to the current session
                if (args.Length < 2)
                {
                    Console.WriteLine("Error: No match ID provided. Use --help for usage information.");
                    return;
                }
                var matchId = args[1];
                var match = new Match(dbContextOptions, integrationService);

                // Match name is optional, so we check if it was provided
                string? matchName = null;
                if (args.Length >= 3)
                {
                    matchName = args[2];
                }

                var currentSessionGuid = session.GetCurrentSession();
                match.AddMatch(Guid.Parse(matchId), currentSessionGuid, matchName);
                Console.WriteLine($"Match added: {matchId}");
                break;

            case "--editmatch":
                // Edit a match
                if (args.Length < 3)
                {
                    Console.WriteLine("Error: No match ID or new match ID provided. Use --help for usage information.");
                    return;
                }
                var editMatchId = args[1];
                var newMatchName = args[2];
                var editMatch = new Match(dbContextOptions, integrationService);
                editMatch.EditMatch(Guid.Parse(editMatchId), newMatchName);
                Console.WriteLine($"Match edited: {editMatchId} set to {newMatchName}");
                break;

            case "--deletematch":
                // Delete a match from the current session
                if (args.Length < 2)
                {
                    Console.WriteLine("Error: No match ID provided. Use --help for usage information.");
                    return;
                }
                var deleteMatchId = args[1];
                var deleteMatch = new Match(dbContextOptions, integrationService);
                if (deleteMatch.DeleteMatch(Guid.Parse(deleteMatchId)))
                {
                    Console.WriteLine($"Match deleted: {deleteMatchId}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Match not found: {deleteMatchId}");
                    Console.ResetColor();
                }
                break;

            case "--excludematch":
                // Mark a match as excluded (DoNotCount = true)
                if (args.Length < 2)
                {
                    Console.WriteLine("Error: No match ID provided. Use --help for usage information.");
                    return;
                }
                var excludeMatchId = args[1];
                if (!Guid.TryParse(excludeMatchId, out Guid parsedExcludeGuid))
                {
                    Console.WriteLine("Error: Invalid match GUID format. Please provide a valid GUID.");
                    return;
                }
                var excludeMatch = new Match(dbContextOptions, integrationService);
                var excluded = excludeMatch.IncludeExcludeMatch(parsedExcludeGuid, false);
                if (excluded)
                {
                    Console.WriteLine($"Match {excludeMatchId} marked as excluded (DoNotCount = true).");
                }
                else
                {
                    Console.WriteLine($"Match not found: {excludeMatchId}");
                }
                break;

            case "--includematch":
                // Unset DoNotCount for a match (DoNotCount = false)
                if (args.Length < 2)
                {
                    Console.WriteLine("Error: No match ID provided. Use --help for usage information.");
                    return;
                }
                var includeMatchId = args[1];
                if (!Guid.TryParse(includeMatchId, out Guid parsedIncludeGuid))
                {
                    Console.WriteLine("Error: Invalid match GUID format. Please provide a valid GUID.");
                    return;
                }

                var includeMatch = new Match(dbContextOptions, integrationService);
                var included = includeMatch.IncludeExcludeMatch(parsedIncludeGuid, true);
                if (included)
                {
                    Console.WriteLine($"Match {includeMatchId} marked as included (DoNotCount = false).");
                }
                else
                {
                    Console.WriteLine($"Match not found: {includeMatchId}");
                }
                break;

            case "--movematch":
                // Edit a match
                if (args.Length < 3)
                {
                    Console.WriteLine("Error: No match ID or new match ID provided. Use --help for usage information.");
                    return;
                }
                var moveMatchId = args[1];
                var newSessionId = args[2];
                var moveMatch = new Match(dbContextOptions, integrationService);
                moveMatch.MoveMatch(Guid.Parse(moveMatchId), Guid.Parse(newSessionId));
                Console.WriteLine($"Moved match {moveMatchId} to session {newSessionId}");
                break;
            case "--listmatches":
                // List all matches in the current session
                var listMatches = new Match(dbContextOptions, integrationService);
                var currentSessionGuidForList = session.GetCurrentSession();
                var matches = listMatches.ListMatches(currentSessionGuidForList);
                Console.WriteLine("Matches in the current session:");
                Console.WriteLine($"   Match Guid                           | Start Time       | Match Name  ");
                Console.WriteLine($"   =====================================|==================|==================");
                foreach (var m in matches)
                {
                    Console.WriteLine($" - {m.MatchGuid} | {m.StartTime.GetValueOrDefault().ToLocalTime().ToString("yyyy-MM-dd HH:mm")} | {m.MatchName}");
                }
                break;

            case "--getmatches":
                // Get recent matches for a player
                if (args.Length < 2)
                {
                    Console.WriteLine("Error: No player ID provided. Use --help for usage information.");
                    return;
                }
                var playerId = args[1];
                var player = new Player(dbContextOptions, integrationService);
                var recentMatches = player.GetRecentMatches(playerId);
                var currentSessionGuidforMatches = session.GetCurrentSession();
                var matchGet = new Match(dbContextOptions, integrationService);
                var allMatches = matchGet.ListMatches().Select(m => m.MatchGuid);

                Console.WriteLine($"Found {recentMatches.Count()} recent matches for player");
                Console.WriteLine($"Skipping any matches already in the database");
                foreach (var matchGuid in recentMatches)
                {
                    //Console.WriteLine($" - Match ID: {matchGuid}");
                    // Lookup each match guid, show times and ask to add yes/No/Quit
                    if (!allMatches.Contains(matchGuid))
                    {
                        var recentMatch = integrationService.GetMatch(matchGuid);
                        if (recentMatch != null)
                        {
                            var isCustom = recentMatch?.data?.attributes?.isCustomMatch;
                            var custom = isCustom.GetValueOrDefault() ? "Custom" : "Normal";

                            Console.WriteLine();
                            Console.WriteLine($"{recentMatch?.data?.id} {custom} {Match.GetGameMode(recentMatch?.data?.attributes?.gameMode)} on {Match.GetMapName(recentMatch?.data?.attributes?.mapName)} at {recentMatch?.data?.attributes?.createdAt.ToLocalTime()} ");
                            Console.Write("Do you wish to add this match (Yes/No/Quit) ? ");
                            var key = Console.ReadKey();
                            Console.WriteLine();
                            
                            switch (key.Key)
                            {
                                case ConsoleKey.Y:
                                    Console.Write("Please enter a name for this match: ");
                                    var enterMatchName = Console.ReadLine();
                                    matchGet.AddMatch(matchGuid, currentSessionGuidforMatches, enterMatchName);
                                    break;
                                case ConsoleKey.N:
                                    continue;
                                case ConsoleKey.Q:
                                    return;
                                default:
                                    Console.WriteLine("Unknown response, skipping");
                                    break;
                            }
                        }
                    }
                }
                break;

            case "--setrandom":
                if (args.Length < 2)
                {
                    Console.WriteLine("Error: No player ID provided. Use --help for usage information.");
                    return;
                }
                var randomPlayerId = args[1];
                var randomPlayer = new Player(dbContextOptions, integrationService);
                var randomSet = randomPlayer.SetRandomFlag(randomPlayerId);
                if (randomSet)
                {
                    Console.WriteLine($"Player {randomPlayerId} marked as random.");
                }
                else
                {
                    Console.WriteLine($"Player not found: {randomPlayerId}");
                }
                break;

case "--setrandominteractive":
                // Present a list of players with 1 match and ask if they are random, then set the flag in the database
                var randomPlayerInteractive = new Player(dbContextOptions, integrationService);
                var playersWithOneMatch = randomPlayerInteractive.GetPlayersWithNumMatch(1);
                Console.WriteLine($"Found {playersWithOneMatch.Count()} players with only 1 match");
                foreach (var p in playersWithOneMatch)
                {
                    Console.WriteLine($" - {p.PlayerName} ({p.PlayerGuid})");
                    Console.Write("Is this player random? (Yes/No/Quit) ? ");
                    var key = Console.ReadKey();
                    Console.WriteLine();
                    switch (key.Key)
                    {
                        case ConsoleKey.Y:
                            randomPlayerInteractive.SetRandomFlag(p.PlayerName); 
                            break;
                        case ConsoleKey.N:
                            continue;
                        case ConsoleKey.Q:
                            return;
                        default:
                            Console.WriteLine("Unknown response, skipping");
                            break;
                    }
                }
                break;
            case "--help":
                DisplayHelp();
                break;

            default:
                Console.WriteLine($"Unknown option: {args[0]}");
                break;
        }
    }
}
else
{
    // If no command line arguments, print a default message
    Console.WriteLine("No command line arguments provided.");

    DisplayHelp();
}
void DisplayHelp()
{
    Console.WriteLine("Usage: PUBGCustomStats [command] <parameter>");
    Console.WriteLine("Options:");

    Console.WriteLine("  --setup                               Initialise the database and create the tables");
    Console.WriteLine("  --apikey <key>                        Set the PUBG API key");
    Console.WriteLine("  --createseason <name>                 Create a new season in the database");
    Console.WriteLine("  --createsession <name> <datetime>     Create a new session for the current season. Format: \"yyyy-MM-dd HH:mm\"");
    Console.WriteLine("  --editseason <seasonGuid> <newName>   Edit the specified season");
    Console.WriteLine("  --editsession <sessionGuid> <newName> <newDateTime>  Edit a session. Format: \"yyyy-MM-dd HH:mm\"");
    Console.WriteLine("  --deletesession <sessionGuid>         Delete a session and all associated matches and data");
    Console.WriteLine("  --addmatch <matchId> [matchName]      Add a match to the current session. Match name is optional.");
    Console.WriteLine("  --editmatch <matchId> <newMatchName>  Edit a match name");
    Console.WriteLine("  --listsessions                        List all sessions in the current season");
    Console.WriteLine("  --listseasons                         List all seasons in the database");
    Console.WriteLine("  --listmatches                         List all matches in the current session");
    Console.WriteLine("  --deletematch <matchId>               Delete a match from the current session");
    Console.WriteLine("  --excludematch <matchId>              Mark a match as excluded (DoNotCount = true)");
    Console.WriteLine("  --includematch <matchId>              Mark a match as included (DoNotCount = false)");
    Console.WriteLine("  --movematch <matchId> <sessionGuid>   Move a match to a different session");
    Console.WriteLine("  --getmatches <gamerTag>               Get recent matches for a player");    
    Console.WriteLine("  --setrandom <playerId>                Mark the specified player as random in the database");
    Console.WriteLine("  --cleanup                             Delete players with no matches and clans with no players");
    Console.WriteLine("  --help                                Display this help message");
    Console.WriteLine();
    Console.WriteLine("If a name contains spaces, enclose it in quotes. For example: --createsession \"My Session\" \"2024-06-01 14:30\"");

}