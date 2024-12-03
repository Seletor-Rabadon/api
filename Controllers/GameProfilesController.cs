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

        [HttpGet("{gameName}/{tagLine}/affinity")]
        public async Task<ActionResult> GetGameProfileAffinity([FromRoute] string gameName, [FromRoute] string tagLine)
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
                    property?.SetValue(playerImage, mastery?.ChampionLevel ?? 0);
                }

                Console.WriteLine("Predicting");
                PlayerImage result = await _aiService.Predict(playerImage);

                var affinity = new List<double>();

                for (int i = 0; i < ChampionConstants.COUNT; i++)
                {
                    var property = result.GetType().GetProperty($"Champion_{i + 1}");
                    var value = (double?)property?.GetValue(result) ?? 0;
                    if ((double?)property?.GetValue(playerImage) >= 2)
                        value = 0;

                    var normalizedValue = value / 20;
                    affinity.Add(normalizedValue);
                }
                

                return Ok(new {
                    Affinity = affinity,
                    Profile = profile, 
                    playerImage
                });
            }
            catch (Exception e)
            {
                return BadRequest($"Request error: {e.Message}");
            }
        }
    }
}