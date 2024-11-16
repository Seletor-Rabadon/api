using System.Text.Json;
using api.Classes;
using api.Interfaces;

namespace api.Services
{
    public class RiotService : IRiotService
    {
        private readonly HttpClient _client;
        private readonly string _apiKey;
        private readonly string _region;
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public RiotService(IConfiguration configuration)
        {
            _client = new HttpClient();
            _apiKey = configuration["RiotApi:ApiKey"] ?? throw new ArgumentException("RiotApi:ApiKey configuration is required");
            _region = "americas";
            _client.DefaultRequestHeaders.Add("X-Riot-Token", _apiKey);
        }

        public async Task<Profile> GetGameProfile(string gameName, string tagLine)
        {
            try
            {
                var user = await GetUserByRiotId(gameName, tagLine);
                return await GetProfileByPuuid(user.Puuid);
            }
            catch (HttpRequestException e)
            {
                throw new Exception($"Error fetching game profile: {e.Message}");
            }
        }

        public async Task<UserResponse> GetUserByRiotId(string gameName, string tagLine)
        {
            string url = $"https://{_region}.api.riotgames.com/riot/account/v1/accounts/by-riot-id/{gameName}/{tagLine}";

            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserResponse>(result, _jsonOptions) 
                ?? throw new JsonException("Failed to deserialize user response");
        }

        public async Task<List<string>> GetMatchHistory(string puuid, int count = 100)
        {
            string url = $"https://{_region}.api.riotgames.com/lol/match/v5/matches/by-puuid/{puuid}/ids?count={count}";
            
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<string>>(result, _jsonOptions) 
                ?? throw new JsonException("Failed to deserialize match history");
        }

        public async Task<List<PlayerMastery>> GetChampionMasteries(string puuid, int championCount, string server)
        {
            string url = $"https://{server}.api.riotgames.com/lol/champion-mastery/v4/champion-masteries/by-puuid/{puuid}/top?count={championCount}";
            
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<PlayerMastery>>(result, _jsonOptions) 
                ?? throw new JsonException("Failed to deserialize champion masteries");
        }

        public async Task<Match> GetMatchData(string matchId)
        {
            string url = $"https://{_region}.api.riotgames.com/lol/match/v5/matches/{matchId}";
            
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Match>(result, _jsonOptions) 
                ?? throw new JsonException("Failed to deserialize match data");
        }

        private async Task<Profile> GetProfileByPuuid(string puuid)
        {
            string url = $"https://BR1.api.riotgames.com/lol/summoner/v4/summoners/by-puuid/{puuid}";

            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Profile>(result, _jsonOptions)
                ?? throw new JsonException("Failed to deserialize profile");
        }
    }
}