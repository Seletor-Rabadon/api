using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MainController : ControllerBase
    {

        [HttpGet("getUser")]
        public async Task<ActionResult> GetHistorySummonerAsync(string username, string tagName){

        string apiKey = "RGAPI-c6352017-69e4-42af-a202-8f13ee6b5070"; 
        string region = "americas"; 

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("X-Riot-Token", apiKey);

            try
            {
                string url = $"https://{region}.api.riotgames.com/riot/account/v1/accounts/by-riot-id/{username}/{tagName}?api_key={apiKey}";

                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();

                return Ok(result);
                // The response will contain a list of match IDs
            }
            catch (HttpRequestException e)
            {
                return BadRequest($"Erro na solicitação: {e.Message}");
            }
        }
        }
    }
}