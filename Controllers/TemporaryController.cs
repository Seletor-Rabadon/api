using Microsoft.AspNetCore.Mvc;
using api.Interfaces;
using api.Classes;
using System.Diagnostics;
using api.Services;

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
                    Position = participant.Lane,
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
                int totalRequests = 0;

                foreach (string match in matchList)
                {
                    await GetMatchData(match, puuid);
                    totalRequests++;
                    if(totalRequests == 20){
                        //Delay for resquest limit
                        totalRequests = 0;
                        await Task.Delay(1500);
                    }

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

        [HttpPost("insertLoopPlayer")]
        public async Task<ActionResult> InsertLoopPlayer(string puuid, long ms)
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                UserResponse currentPlayer = new UserResponse(){
                    Puuid = puuid,
                    GameName = "",
                    TagLine = ""
                };

                while (stopwatch.ElapsedMilliseconds < ms)
                {
                    currentPlayer = await _riotService.GetPlayerByPuuid(currentPlayer.Puuid);
                    await _dataService.InsertPlayerAsync(currentPlayer.Puuid, currentPlayer.GameName, currentPlayer.TagLine);
                    await InsertMasteryPoints(currentPlayer.Puuid, 168, "BR1");
                    await LoopInsertMatchChampion(currentPlayer.Puuid);
                    currentPlayer.Puuid = await _riotService.GetNextPlayer(currentPlayer.Puuid);
                    await Task.Delay(1500);
                }

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest($"Request error: {e.Message}");
            }
        }

        [HttpPost("insertLoopPlayerMastery")]
        public async Task<ActionResult> InsertLoopPlayerMastery(string puuid, long ms)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int successCount = 0;
            int errorCount = 0;

            UserResponse currentPlayer = new UserResponse(){
                Puuid = puuid,
                GameName = "",
                TagLine = ""
            };

            while (stopwatch.ElapsedMilliseconds < ms)
            {
                try
                {
                    currentPlayer = await _riotService.GetPlayerByPuuid(currentPlayer.Puuid);
                    await _dataService.InsertPlayerAsync(currentPlayer.Puuid, currentPlayer.GameName, currentPlayer.TagLine);
                    await InsertMasteryPoints(currentPlayer.Puuid, 168, "BR1");
                    successCount++;
                }
                catch (Exception e)
                {
                    errorCount++;
                    // Log the error but continue with the loop
                    Console.WriteLine($"Error processing player {currentPlayer.Puuid}: {e.Message}");
                }

                try
                {
                    currentPlayer.Puuid = await _riotService.GetNextPlayer(currentPlayer.Puuid);
                    await Task.Delay(1500);
                }
                catch (Exception e)
                {
                    errorCount++;
                    Console.WriteLine($"Error getting next player: {e.Message}");
                    // If we can't get the next player, break the loop
                    break;
                }
            }

            return Ok(new { 
                message = "Loop completed", 
                successfulOperations = successCount, 
                failedOperations = errorCount 
            });
        }
    }
}