using Microsoft.AspNetCore.Mvc;
using api.Interfaces;
using api.Classes;

namespace api.Controllers
{
    [ApiController]     
    public class TemporaryController : ControllerBase
    {
        private readonly IDataBaseService _dataService;
        private readonly IRiotService _riotService;

        public TemporaryController(
            IDataBaseService dataService,
            IRiotService riotService)
        {
            _dataService = dataService;
            _riotService = riotService;
        }

        [HttpGet("insertUser")]
        public async Task<ActionResult> InsertUserAsync(string username, string tagName)
        {
            try
            {
                var user = await _riotService.GetUserByRiotId(username, tagName);
                bool insertSuccess = await _dataService.InsertPlayerAsync(user.Puuid, user.GameName, user.TagLine);

                if (insertSuccess)
                {
                    return Ok("Player inserted successfully.");
                }
                return BadRequest("Error inserting player.");
            }
            catch (Exception e)
            {
                return BadRequest($"Request error: {e.Message}");
            }
        }

        [HttpGet("getMatchHistory")]
        public async Task<ActionResult> GetMatchHistoryAsync(string puuid)
        {
            try
            {
                var matchHistory = await _riotService.GetMatchHistory(puuid);
                return Ok(matchHistory);
            }
            catch (Exception e)
            {
                return BadRequest($"Request error: {e.Message}");
            }
        }

        [HttpGet("insertMasteryPoints")]
        public async Task<ActionResult> InsertMasteryPoints(string puuid, int championCount, string server)
        {
            try
            {
                var playerMasteries = await _riotService.GetChampionMasteries(puuid, championCount, server);
                
                foreach (var mastery in playerMasteries)
                {
                    await _dataService.InsertPlayerMasteryAsync(mastery.Puuid, mastery.ChampionId, mastery.ChampionLevel);
                }

                return Ok("Player masteries inserted successfully.");
            }
            catch (Exception e)
            {
                return BadRequest($"Request error: {e.Message}");
            }
        }

        [HttpGet("insertMatchData")]
        public async Task<ActionResult> GetMatchData(string matchId, string puuid)
        {
            try
            {
                var matchData = await _riotService.GetMatchData(matchId);
                var participant = matchData.Info.Participants.Find(p => p.Puuid == puuid);

                if (participant == null)
                {
                    return NotFound("Participant not found in match");
                }

                float kda = (participant.Deaths == 0) ? 
                    (participant.Kills + participant.Assists) : 
                    (participant.Kills + participant.Assists);

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
            catch (Exception e)
            {
                return BadRequest($"Request error: {e.Message}");
            }
        }

        [HttpGet("updatePlayerData")]
        public async Task<ActionResult> UpdatePlayerData(string puuid)
        {
            try
            {
                await _dataService.UpdatePlayerDataAsync(puuid);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest($"Request error: {e.Message}");
            }
        }

        [HttpGet("loopInsertMatchChampion")]
        public async Task<ActionResult> LoopInsertMatchChampion(string puuid)
        {
            try
            {
                var matchList = await _riotService.GetMatchHistory(puuid);

                foreach (string match in matchList)
                {
                    await GetMatchData(match, puuid);
                }

                await UpdatePlayerData(puuid);
                return Ok("Loop has been completed!");
            }
            catch (Exception e)
            {
                return BadRequest($"Request error: {e.Message}");
            }
        }

        [HttpGet("getUserProfile")]
        public async Task<ActionResult> GetUserProfile(string gameName, string tagLine)
        {
            try
            {
                var profile = await _riotService.GetGameProfile(gameName, tagLine);
                return Ok(profile);
            }
            catch (Exception e)
            {
                return BadRequest($"Request error: {e.Message}");
            }
        }
    }
}