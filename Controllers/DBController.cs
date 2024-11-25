using Microsoft.AspNetCore.Mvc;
using api.Interfaces;
using api.Classes;
using System.Diagnostics;
using api.Services;

namespace api.Controllers
{
    [ApiController]
    [Route("db")]
    public class DBController(
        IDataBaseService dataService,
        IRiotService riotService) : ControllerBase
    {
        private readonly IDataBaseService _dataService = dataService;
        private readonly IRiotService _riotService = riotService;

        private async Task<bool> RecursivePopulateDataBase(string puuid)
        {
            Console.WriteLine($"=====================================================");
            var player = await _riotService.GetPlayerByPuuid(puuid);
            await Task.Delay(1500);

            Console.WriteLine($" ➜ Player: \u001b[93m{player.GameName}#{player.TagLine}\u001b[0m");
            Console.WriteLine($"    - Getting champion masteries... ");
            var championMasteries = await _riotService.GetChampionMasteries(player.Puuid);

            await Task.Delay(1500);

            Console.WriteLine($"    - Inserting player...");
            await _dataService.InsertPlayer(player);

            Console.WriteLine($"    - Inserting champion masteries...");
            await _dataService.InsertChampionMasteries(player.Puuid, championMasteries);

            Console.WriteLine($"    - Getting match history...");
            var matchHistory = await _riotService.GetMatchHistory(player.Puuid);

            await Task.Delay(1500);

            foreach (var match in matchHistory)
            {
                Console.WriteLine($"    - Getting match data {match}...");
                var matchData = await _riotService.GetMatchData(match);

                await Task.Delay(1500);

                Console.WriteLine($"    - Getting participants data...");
                var participants = matchData?.Info?.Participants;

                if (participants == null)
                {
                    Console.WriteLine($"    - ❌ No participants found");
                    continue;
                }

                foreach (var participant in participants)
                {
                    if (participant.Puuid == player.Puuid || participant.Puuid == null)
                    {
                        Console.WriteLine($"    - ➖ Participant is the same as the player");
                        continue;
                    }

                    await RecursivePopulateDataBase(participant.Puuid);
                }
            }

            return true;
        }

        [HttpPost("populate")]
        public async Task<ActionResult> populate(string puuid, long? ms)
        {
            Stopwatch stopwatch = new();

            await Task.Delay(1500);
            while (ms == null || stopwatch.ElapsedMilliseconds < ms)
            {
                try
                {
                    await RecursivePopulateDataBase(puuid);
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("403"))
                    {
                        return BadRequest("API key expired or invalid permissions");
                    }
                    Console.WriteLine($"❌ Error processing player: {e.Message}");
                }
            }

            return Ok(new { message = "Loop completed" });
        }

    }
}