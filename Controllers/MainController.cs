using System.Text.Json;
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

        [HttpGet("getUser")]
        public async Task<ActionResult> GetUserAsync(string username, string tagName){

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

                bool insertSuccess = await _dataService.InsertPlayerAsync(user.puuid, user.gameName, user.tagLine);

                if (insertSuccess)
                {
                    return Ok("Jogador inserido com sucesso.");
                }
                else
                {
                    return BadRequest("Erro ao inserir o jogador.");
                }
            }
            catch (HttpRequestException e)
            {
                return BadRequest($"Erro na solicitação: {e.Message}");
            }
        }
        }
        
        [HttpGet("GetMatchHistory")]
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


        [HttpGet("GetAllChampions")]
        public async Task<ActionResult> GetAllChampionsAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = $"https://ddragon.leagueoflegends.com/cdn/14.21.1/data/en_US/champion.json";
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


    }
}