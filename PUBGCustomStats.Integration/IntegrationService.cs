using Newtonsoft.Json;
using PUBGCustomStats.Integration.JsonObject;
using System.IO.Compression;
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

        public PlayerDirect GetPlayer(string pubgPlayerId)
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

        public PlayerFilter GetPlayer(string playerName, string console)
        {
            var url = $"https://api.pubg.com/shards/{console}/players?filter[playerNames]={playerName}";

            return GetPlayerFilter(url);
        }

        private PlayerFilter GetPlayerFilter(string url)
        {
            System.Threading.Thread.Sleep(10000); // Wait for 10 seconds before making the request

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.api+json");
                var response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;

                    var playerData = JsonConvert.DeserializeObject<PlayerFilter>(content);

                    playerData.RawData = content; // Store the raw JSON response 

                    return playerData;
                }
                else
                {
                    throw new Exception($"Error fetching player data: {response.ReasonPhrase}");
                }
            }
        }

        private PlayerDirect GetPlayerDirect(string url)
        {
            System.Threading.Thread.Sleep(5000); // Wait for 10 seconds before making the request

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.api+json");
                var response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;

                    var playerData = JsonConvert.DeserializeObject<PlayerDirect>(content);

                    playerData.RawData = content; // Store the raw JSON response 

                    return playerData;
                }
                else
                {
                    throw new Exception($"Error fetching player data: {response.ReasonPhrase}");
                }
            }
        }

        public Clan GetClan(string pubgClanId)
        {
            System.Threading.Thread.Sleep(10000); // Wait for 10 seconds before making the request

            var url = $"https://api.pubg.com/shards/xbox/clans/{pubgClanId}";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.api+json");
                var response = client.GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    var clanData = JsonConvert.DeserializeObject<Clan>(content);
                    clanData.RawData = content; // Store the raw JSON response 
                    return clanData;
                }
                else
                {
                    throw new Exception($"Error fetching clan data: {response.ReasonPhrase}");
                }
            }
        }
        public Match GetMatch(Guid matchGuid)
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
                    var matchData = JsonConvert.DeserializeObject<Match>(content);
                    matchData.RawData = content; // Store the raw JSON response 
                    return matchData;
                }
                else
                {
                    throw new Exception($"Error fetching match data: {response.ReasonPhrase}");
                }
            }
        }

        public Match ParseMatch( string jsonPayload)
        {
            var matchData = JsonConvert.DeserializeObject<Match>(jsonPayload);
            matchData.RawData = jsonPayload; // Store the raw JSON response 
            return matchData;
        }

        public async Task< string> GetTelemetry(string url)
        {
            var clientHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            using (var client = new HttpClient(clientHandler))
            {
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.api+json");

                var response = client.GetAsync(url).Result;
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

        public Telemetry[] ParseTelemetry(string jsonPayload)
        {
            return JsonConvert.DeserializeObject<Telemetry[]>(jsonPayload);
        }

        public MatchBlueZone[] ParseMatchBlueZone(string jsonPayload)
        {
            return JsonConvert.DeserializeObject<MatchBlueZone[]>(jsonPayload);
        }
    }
}
