using Microsoft.AspNetCore.Mvc;
using api.Interfaces;
using api.Classes;
using api.Constants;

namespace api.Controllers
{
    [ApiController]
    [Route("game-profiles")]
    public class GameProfilesController : ControllerBase
    {
        private readonly IDataBaseService _dataService;
        private readonly IAIService _aiService;
        private readonly IRiotService _riotService;

        public GameProfilesController(
            IDataBaseService dataService,
            IAIService aiService,
            IRiotService riotService)
        {
            _dataService = dataService;
            _aiService = aiService;
            _riotService = riotService;
        }

        [HttpGet("{gameName}/{tagLine}/compatibility")]
        public async Task<ActionResult> GetGameProfileCompatibility([FromRoute] string gameName, [FromRoute] string tagLine)
        {
            try
            {
                Console.WriteLine("Getting game profile");
                var profile = await _riotService.GetGameProfile(gameName, tagLine); 
                await Task.Delay(1500);
                Console.WriteLine("Getting champion masteries");
                var championMasteries = await _riotService.GetChampionMasteries(profile.Puuid);

                var playerImage = new PlayerImage { Puuid = profile.Puuid };
                var championIds = ChampionConstants.Champions
                    .OrderBy(x => long.Parse(x.Key))
                    .Select(x => long.Parse(x.Key))
                    .ToList();

                Console.WriteLine("Mapping champion masteries to player image");
                for (int i = 0; i < championIds.Count; i++)
                {
                    var mastery = championMasteries.FirstOrDefault(m => m.ChampionId == championIds[i]);
                    var propertyName = $"Champion_{i + 1}";
                    var property = typeof(PlayerImage).GetProperty(propertyName);
                    property?.SetValue(playerImage, mastery?.ChampionPoints ?? 0);
                }

                Console.WriteLine("Predicting");
                var result = await _aiService.Predict(playerImage);
                return Ok(new {
                    PredictionResult = result,
                    Profile = profile, 
                });
            }
            catch (Exception e)
            {
                return BadRequest($"Request error: {e.Message}");
            }
        }
    }
}