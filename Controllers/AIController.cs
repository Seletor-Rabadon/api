using Microsoft.AspNetCore.Mvc;
using api.Interfaces;

namespace api.Controllers
{
    [ApiController]    
    [Route("ai")]  
    public class AIController : ControllerBase
    { 
        private readonly IAIService _aiService; 
        public AIController(
            IAIService aiService)
        { 
            _aiService = aiService; 
        }

        [HttpGet("train")]
        public async Task<ActionResult> Train()
        {
            try
            {
                var result = await _aiService.Train();
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest($"Request error: {e.Message}");
            }
        }
    }
}