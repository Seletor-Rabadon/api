using Microsoft.AspNetCore.Mvc;
using api.Interfaces;
using api.Classes;
using System.Diagnostics;
using api.Services;
using System.IO;
using api.Constants;

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

        private async Task<bool> RecursivePopulateDataBase(string puuid, string[] addedPlayers)
        {
            if (addedPlayers.Contains(puuid))
            {
                Console.WriteLine($"    - ➖ Player already added");
                return true;
            }

            Console.WriteLine($"=====================================================");
            var player = await _riotService.GetPlayerByPuuid(puuid);
            await Task.Delay(1500);

            Console.WriteLine($" ➜ Player: \u001b[93m{player.GameName}#{player.TagLine}\u001b[0m");
            Console.WriteLine($"    - Getting champion masteries... ");
            var championMasteries = await _riotService.GetChampionMasteries(player.Puuid);

            await Task.Delay(1500);

            Console.WriteLine($"    - Inserting player...");

            await _dataService.InsertPlayer(player);

            await Task.Delay(1500);

            Console.WriteLine($"    - Inserting champion masteries...");
            await _dataService.InsertChampionMasteries(player.Puuid, championMasteries);

            addedPlayers = [.. addedPlayers, player.Puuid];

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
                    if (addedPlayers.Contains(participant.Puuid))
                    {
                        Console.WriteLine($"    - ➖ Participant already added");
                        continue;
                    }

                    if (participant.Puuid == player.Puuid || participant.Puuid == null)
                    {
                        Console.WriteLine($"    - ➖ Participant is the same as the player");
                        continue;
                    }

                    await RecursivePopulateDataBase(participant.Puuid, addedPlayers);
                }
            }

            return true;
        }

        [HttpPost("populate")]
        public async Task<ActionResult> Populate(string GameName, string TagLine, long? ms)
        {
            Stopwatch stopwatch = new();

            var player = await _riotService.GetGameProfile(GameName, TagLine);

            await Task.Delay(1500);
            while (ms == null || stopwatch.ElapsedMilliseconds < ms)
            {
                try
                {
                    await RecursivePopulateDataBase(player.Puuid, []);
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

         [HttpGet("generate-data-file")]
        public async Task<ActionResult> GenerateDataFile()
        {
            try
            {
                Console.WriteLine("Fetching player images...");
                var playerImages = await _dataService.GetPlayerImages();
                var aiFolder = Path.Combine(Directory.GetCurrentDirectory(), "AI");
                var filePath = Path.Combine(aiFolder, "data.csv");
                
                if (!Directory.Exists(aiFolder))
                {
                    Directory.CreateDirectory(aiFolder);
                }
                
                Console.WriteLine("Generating data file...");
                using (var writer = new StreamWriter(filePath))
                {
                    foreach (var player in playerImages)
                    { 
                        var line = player.Puuid; 

                        for (int i = 1; i <= ChampionConstants.COUNT; i++)
                        {
                            var championLevel = player.GetType().GetProperty($"Champion_{i}")?.GetValue(player) ?? 0;
                            line += $";{championLevel}";
                        }

                        await writer.WriteLineAsync(line);
                    }
                }

                Console.WriteLine("Data file generated successfully");
                return Ok(new { message = "Data file generated successfully", path = filePath });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error generating data file: {ex.Message}");
                return BadRequest(new { message = $"Error generating data file: {ex.Message}" });
            }
        }

    }
}