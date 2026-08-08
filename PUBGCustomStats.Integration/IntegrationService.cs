using Newtonsoft.Json;
using PUBGCustomStats.Integration.JsonObject;
using System.Net;

namespace PUBGCustomStats.Integration
{
    public class IntegrationService
    {
        private string apiKey = "";

        public IntegrationService(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public PlayerDirect? GetPlayer(string pubgPlayerId)
        {
            var url = $"https://api.pubg.com/shards/xbox/players/{pubgPlayerId}";

            try
            {
                return GetPlayerDirect(url);
            }
            catch (Exception)
            {
                url = $"https://api.pubg.com/shards/psn/players/{pubgPlayerId}";
                return GetPlayerDirect(url);
            }
        }

        public PlayerFilter? GetPlayer(string playerName, string console)
        {
            var url = $"https://api.pubg.com/shards/{console}/players?filter[playerNames]={playerName}";

            return GetPlayerFilter(url);
        }

        private PlayerFilter? GetPlayerFilter(string url)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.api+json");
                var response = SendGetRequestWithRateLimitHandling(client, url);

                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;

                    var playerData = JsonConvert.DeserializeObject<PlayerFilter>(content);

                    if (playerData != null)
                    {
                        playerData.RawData = content; // Store the raw JSON response 
                    }
                    return playerData;
                }
                else
                {
                    throw new Exception($"Error fetching player data: {response.ReasonPhrase}");
                }
            }
        }

        private PlayerDirect? GetPlayerDirect(string url)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.api+json");
                var response = SendGetRequestWithRateLimitHandling(client, url);

                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;

                    var playerData = JsonConvert.DeserializeObject<PlayerDirect>(content);

                    if (playerData != null)
                    {
                        playerData.RawData = content; // Store the raw JSON response
                    }

                    return playerData;
                }
                else
                {
                    throw new Exception($"Error fetching player data: {response.ReasonPhrase}");
                }
            }
        }

        public Clan? GetClan(string pubgClanId)
        {
            var url = $"https://api.pubg.com/shards/xbox/clans/{pubgClanId}";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.api+json");
                var response = SendGetRequestWithRateLimitHandling(client, url);
                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    var clanData = JsonConvert.DeserializeObject<Clan>(content);
                    if (clanData != null)
                    {
                        clanData.RawData = content; // Store the raw JSON response
                    }
                    return clanData;
                }
                else
                {
                    throw new Exception($"Error fetching clan data: {response.ReasonPhrase}");
                }
            }
        }
        public Match? GetMatch(Guid matchGuid)
        {
            var url = $"https://api.pubg.com/shards/steam/matches/{matchGuid}";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.api+json");
                var response = client.GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    return ParseMatch(content);
                }
                else
                {
                    throw new Exception($"Error fetching match data: {response.ReasonPhrase}");
                }
            }
        }

        public Match? ParseMatch(string jsonPayload)
        {
            var matchData = JsonConvert.DeserializeObject<Match>(jsonPayload);
            if (matchData != null)
            {
                matchData.RawData = jsonPayload; // Store the raw JSON response
            }
            return matchData;
        }

        public async Task<string> GetTelemetry(string url)
        {
            var clientHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            using (var client = new HttpClient(clientHandler))
            {
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.api+json");

                var response = SendGetRequestWithRateLimitHandling(client, url);
                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    return content; // Return the raw telemetry data as a string    

                }
                else
                {
                    throw new Exception($"Error fetching telemetry URL: {response.ReasonPhrase}");
                }

            }
        }

        // Sends GET requests and respects rate limit headers returned by the PUBG API.
        // If the response indicates the rate limit has been reached (remaining <= 0 or 429 status),
        // this method will sleep until the reset time (from X-Ratelimit-Reset or Retry-After) and retry.
        private HttpResponseMessage SendGetRequestWithRateLimitHandling(HttpClient client, string url)
        {
            const int maxRetries = 5;
            int attempt = 0;

            while (true)
            {
                attempt++;
                var response = client.GetAsync(url).Result;

                // Inspect rate limit headers
                var remaining = -1;
                if (response.Headers.TryGetValues("X-Ratelimit-Remaining", out var remVals))
                {
                    var remStr = remVals.FirstOrDefault();
                    if (!string.IsNullOrEmpty(remStr) && int.TryParse(remStr, out var r)) remaining = r;
                }

                // If remaining header indicates no quota left, compute wait and either retry or sleep
                if (remaining == 0 || response.StatusCode == (HttpStatusCode)429)
                {
                    // First try Retry-After header
                    int waitSeconds = -1;
                    if (response.Headers.RetryAfter != null)
                    {
                        if (response.Headers.RetryAfter.Delta.HasValue)
                        {
                            waitSeconds = (int)Math.Ceiling(response.Headers.RetryAfter.Delta.Value.TotalSeconds);
                        }
                        else if (response.Headers.RetryAfter.Date.HasValue)
                        {
                            var dt = response.Headers.RetryAfter.Date.Value;
                            waitSeconds = (int)Math.Ceiling((dt - DateTimeOffset.UtcNow).TotalSeconds);
                        }
                    }

                    // Fallback to X-Ratelimit-Reset header
                    if (waitSeconds <= 0 && response.Headers.TryGetValues("X-Ratelimit-Reset", out var resetVals))
                    {
                        var resetStr = resetVals.FirstOrDefault();
                        if (!string.IsNullOrEmpty(resetStr) && long.TryParse(resetStr, out var resetVal))
                        {
                            // If header is large (epoch seconds), convert to seconds-until-reset
                            if (resetVal > 1000000000)
                            {
                                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                waitSeconds = (int)Math.Max(0, resetVal - now);
                            }
                            else
                            {
                                // Otherwise assume it's seconds until reset
                                waitSeconds = (int)Math.Max(0, resetVal);
                            }
                        }
                    }

                    // Ensure we have a positive wait time
                    if (waitSeconds <= 0) waitSeconds = 1;

                    // Try to read the total limit header for logging
                    var limit = -1;
                    if (response.Headers.TryGetValues("X-Ratelimit-Limit", out var limitVals))
                    {
                        var limitStr = limitVals.FirstOrDefault();
                        if (!string.IsNullOrEmpty(limitStr) && int.TryParse(limitStr, out var l)) limit = l;
                    }

                    // Dispose the response before sleeping and retrying
                    response.Dispose();

                    if (attempt >= maxRetries)
                    {
                        var _prevColor = Console.ForegroundColor;
                        try
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Rate limited for {url}. Giving up after {attempt} attempts.");
                        }
                        finally
                        {
                            Console.ForegroundColor = _prevColor;
                        }

                        throw new Exception($"Rate limited and max retries reached for {url}");
                    }

                    // Log to console the rate limit and wait time (in yellow)
                    var prevColor = Console.ForegroundColor;
                    try
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Rate limit reached for {url}. Remaining={remaining}, Limit={limit}, waiting {waitSeconds} seconds before retry (attempt {attempt}/{maxRetries}).");
                    }
                    finally
                    {
                        Console.ForegroundColor = prevColor;
                    }

                    // Sleep with a small buffer
                    System.Threading.Thread.Sleep((waitSeconds * 1000) + 1000);
                    continue; // retry
                }

                // If there is a remaining header with value > 0, we can optionally log limit info.
                // If remaining is present and greater than 0, but reset indicates an upcoming reset, we don't need to wait here.

                return response;
            }
        }

        public Telemetry[]? ParseTelemetry(string jsonPayload)
        {
            return JsonConvert.DeserializeObject<Telemetry[]>(jsonPayload);
        }

        public MatchBlueZone[]? ParseMatchBlueZone(string jsonPayload)
        {
            return JsonConvert.DeserializeObject<MatchBlueZone[]>(jsonPayload);
        }
    }
}
