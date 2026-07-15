PUBG Custom Stats
=================

This applicaion and web page is used to keep track of stats from custom games in PUBG. 
It is not affiliated with PUBG corp or any of its subsidiaries. This is a personal project and is not intended for commercial use.


Season, Session, and Match Structure
------------------------------------
Matches are grouped into sessions, and sessions are grouped into seasons. This allows you to track your stats over time, and compare your performance across different seasons and sessions.
Our group plays a session of 4 games on friday nights through the year. Points are awarded for each match, and contribute to points for the night (session) and the season.

Scoring
-------
To encourge battles over camping, points are awarded for both placement and damage dealt. Winning the match doesn't necessarily get the most points.
Points are awarded as follows:
* 1 Point for participation
* 10 points for a win, 9 for 2nd place, 8 for 3rd place, and so on down to 1 point for 10th place.
* 10 points for most damage dealt, 9 for 2nd place, and so on down to 1 point for 10th most damage. No points are awarded for zero damage dealt.

In matches with bots, they can take a place in the match and damage rankings. However they are not shown on the match score table.

Prerequisites
-------------
* .NET 10.0 SDK or later
* An API key from the PUBG Developer Portal (https://developer.pubg.com/)
* Guids for the matches you want to track. You can get these by looking at your match history on sites like pubglookup.com or chickendinner.gg.

Installation
------------

1. Clone the repository:
   ```bash
   git clone <url>
   ```
 2. Navigate to the project directory:
   ```bash
   cd PUBGCustomStats
   ```
 3. Build the solution:
   ```bash
   dotnet build
   ```
	
Configuration
-------------
0. Change to the folder, where the console application was built.
```cd PUBGCustomStats/bin/Debug/net10.0```

1. Create the database. (Command may need ./ at the start, depening on your PATH setup)
```PUBGCustomStats --setup```

2. Enter your API key. The application will save your API key for future use.
```PUBGCustomStats --apikey <apikey>```

3. Create a season, and give it a name. This will be used to group your stats together. Seasons can have multiple sessions, and sessions can have multiple matches.
```PUBGCustomStats --createseason <name>```

4. Create a session, with a name and start time. Sessions can have multiple matches.
```PUBGCustomStats --createsession "<session name>" "<yyyy-MM-dd HH:mm>"```

5. Add matches to the session. Repeat this step for each match you want to add. the first time you run this command, it will take longer, as player and clan details are downloaded for the first time. Subsequent matches will add faster, as these details are in the database.
```PUBGCustomStats --addmatch <match guid>```

your database should now be filled with the match stats. You can view your stats by running PUBGCustomStats.Web and visiting the web page at http://localhost:5000.

Viewing
-------

1. Change to the Web project directory
```cd PUBGCustomStats.Web```

2. Run the project, which will open the site using Kestral, the web server included with .NET.
```dotnet run```

3. Open the website in a browser. The output from above command will give a URL which Kestral is listening on. Eg. http://localhost:5209

Publishing
----------
Web site has a reference to the library package ASPNetStatic, and can produce static HTML pages from your data. 

No automated way exists is publish at the moment, but uncomment the following line in `Program.cs` in the web project

```//app.GenerateStaticContent(@"c:\PUBGCustoms\");```

Replace the path as needed. Compile and run the web project again.

