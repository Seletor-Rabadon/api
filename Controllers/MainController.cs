using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MainController : ControllerBase
    {
        private readonly IDataService _dataService;
        private readonly string _apiKey;
        private readonly string _region;

        public MainController(IDataService dataService,
                              IConfiguration configuration)
        {
            _dataService = dataService;
            _apiKey = configuration["RiotApi:ApiKey"];
            _region = "americas";
        }

        [HttpGet("insertUser")]
        public async Task<ActionResult> InsertUserAsync(string username, string tagName)
        {

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-Riot-Token", _apiKey);

                try
                {
                    string url = $"https://{_region}.api.riotgames.com/riot/account/v1/accounts/by-riot-id/{username}/{tagName}?api_key={_apiKey}";

                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var result = await response.Content.ReadAsStringAsync();
                    var user = JsonSerializer.Deserialize<UserResponse>(result);

                    bool insertSuccess = await _dataService.InsertPlayerAsync(user.Puuid, user.GameName, user.TagLine);

                    if (insertSuccess)
                    {
                        return Ok("Player inserted successfully.");
                    }
                    else
                    {
                        return BadRequest("Error inserting player.");
                    }
                }
                catch (HttpRequestException e)
                {
                    return BadRequest($"Erro na solicitação: {e.Message}");
                }
            }
        }

        [HttpGet("getMatchHistory")]
        public async Task<ActionResult> GetMatchHistoryAsync(string puuid)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-Riot-Token", _apiKey);

                try
                {
                    string url = $"https://{_region}.api.riotgames.com/lol/match/v5/matches/by-puuid/{puuid}/ids";
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var result = await response.Content.ReadAsStringAsync();
                    return Ok(result);
                }
                catch (HttpRequestException e)
                {
                    return BadRequest($"Erro na solicitação: {e.Message}");
                }
            }
        }

        [HttpGet("insertMasteryPoints")]
        public async Task<ActionResult> InsertMasteryPoints(string puuid, int championCount, string server)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-Riot-Token", _apiKey);

                try
                {
                    string url = $"https://{server}.api.riotgames.com/lol/champion-mastery/v4/champion-masteries/by-puuid/{puuid}/top?count={championCount}";
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    var result = await response.Content.ReadAsStringAsync();
                    var playerMastery = JsonSerializer.Deserialize<List<PlayerMastery>>(result, options);

                    foreach (PlayerMastery item in playerMastery)
                    {
                        await _dataService.InsertPlayerMasteryAsync(item.Puuid, item.ChampionId, item.ChampionLevel);
                    }

                    return Ok("Player inserted successfully.");
                }
                catch (HttpRequestException e)
                {
                    return BadRequest($"Erro na solicitação: {e.Message}");
                }
            }
        }

        [HttpGet("insertMatchData")]
        public async Task<ActionResult> GetMatchData(string matchId, string puuid)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-Riot-Token", _apiKey);
                string url = $"https://{_region}.api.riotgames.com/lol/match/v5/matches/{matchId}";

                var response = await client.GetStringAsync(url);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var matchApiResponse = JsonSerializer.Deserialize<Match>(response, options);

                // Find the participant that matches the specified PUUID
                var participant = matchApiResponse.Info.Participants.Find(p => p.Puuid == puuid);

                float kda = (participant.Deaths == 0) ? (float)(participant.Kills + participant.Assists) : (float)(participant.Kills + participant.Assists);

                var matchChampion = new MatchChampion
                {
                    MatchId = matchId,
                    Puuid = participant.Puuid,
                    ChampionId = participant.ChampionId,
                    Position = participant.TeamPosition,
                    Kda = kda,
                    Win = participant.Win
                };

                await _dataService.InsertMatchAsync(matchChampion);

                return Ok();
            }
        }

        [HttpGet("updatePlayerData")]
        public async Task<ActionResult> UpdataPlayerData(string puuid)
        {
            await _dataService.UpdatePlayerDataAsync(puuid);

            return Ok();
        }

        [HttpGet("loopInsertMatchChampion")]
        public async Task<ActionResult> LoopInsertMatchChampion(string puuid)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-Riot-Token", _apiKey);

                try
                {
                    string url = $"https://{_region}.api.riotgames.com/lol/match/v5/matches/by-puuid/{puuid}/ids?count=100";
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    var result = await response.Content.ReadAsStringAsync();
                    var matchList = JsonSerializer.Deserialize<List<string>>(result, options);

                    foreach (string match in matchList)
                    {
                        await GetMatchData(match, puuid);
                    }

                    await UpdataPlayerData(puuid);

                    return Ok("Loop has been done!");
                }
                catch (HttpRequestException e)
                {
                    return BadRequest($"Erro na solicitação: {e.Message}");
                }
            }
        }
    }
}