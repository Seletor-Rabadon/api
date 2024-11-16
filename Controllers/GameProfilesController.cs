using Microsoft.AspNetCore.Mvc;
using api.Interfaces;

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
                // var profile = await _riotService.GetGameProfile(gameName, tagLine);
                var result = _aiService.Train();
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest($"Request error: {e.Message}");
            }
        }
    }
}