using api.Classes;
using api.Interfaces;
using Npgsql;
using Dapper;
using api.Constants;

namespace api.Services
{
    public class DatabaseService(IConfiguration configuration) : IDataBaseService
    {
        private readonly string _connectionString = configuration.GetConnectionString("PostgresConnection")
                ?? throw new ArgumentException("PostgresConnection string is not configured");

        public async Task<bool> InsertPlayer(UserResponse player)
        {
            var checkQuery = "SELECT COUNT(*) FROM public.player WHERE puuid = @PUUID";
            var insertQuery = "INSERT INTO public.player (puuid, game_name, tag_line) VALUES (@PUUID, @GAMENAME, @TAGLINE)";
            var updateQuery = "UPDATE public.player SET game_name = @GAMENAME, tag_line = @TAGLINE WHERE puuid = @PUUID";

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var checkCommand = new NpgsqlCommand(checkQuery, connection);
            checkCommand.Parameters.AddWithValue("PUUID", player.Puuid);

            var exists = await checkCommand.ExecuteScalarAsync() as long? > 0 || false;

            await using var command = new NpgsqlCommand(exists ? updateQuery : insertQuery, connection);
            command.Parameters.AddWithValue("PUUID", player.Puuid);
            command.Parameters.AddWithValue("GAMENAME", player.GameName);
            command.Parameters.AddWithValue("TAGLINE", player.TagLine);

            try
            {
                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while {(exists ? "updating" : "inserting")} data: {ex.Message}");
                return false;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public async Task<bool> InsertMatchAsync(MatchChampion matchChampion)
        {

            var query = @"
        INSERT INTO public.match_champion (match_id, puuid, champion_id, position, kda, win)
        VALUES (@MATCHID, @PUUID, @CHAMPIONID, @POSITION, @KDA, @WIN)";

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("MATCHID", matchChampion.MatchId);
            command.Parameters.AddWithValue("PUUID", matchChampion.Puuid);
            command.Parameters.AddWithValue("CHAMPIONID", matchChampion.ChampionId);
            command.Parameters.AddWithValue("POSITION", matchChampion.Position ?? (object)DBNull.Value); // Null handling for position
            command.Parameters.AddWithValue("KDA", matchChampion.Kda);
            command.Parameters.AddWithValue("WIN", matchChampion.Win);

            try
            {
                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting match data: {ex.Message}");
                return false;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public async Task<bool> UpdatePlayerDataAsync(string puuid)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            List<int> championIds = [];

            using (var cmd = new NpgsqlCommand("SELECT champion_id FROM public.player_mastery WHERE puuid = @puuid", conn))
            {
                cmd.Parameters.AddWithValue("@puuid", puuid);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    championIds.Add(reader.GetInt32(0));
                }
            }

            foreach (int item in championIds)
            {
                int matchs = 0;
                float kda = 0;

                using (var cmd = new NpgsqlCommand(@"
                SELECT COUNT(*) AS matchs,
                       COALESCE(SUM(kda) / NULLIF(COUNT(*), 0), 0) AS media_kda
                  FROM match_champion
                 WHERE puuid = @puuid
                   AND champion_id = @champion_id", conn))
                {
                    cmd.Parameters.AddWithValue("@puuid", puuid);
                    cmd.Parameters.AddWithValue("@champion_id", item);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            matchs = reader.GetInt32(0);
                            kda = reader.GetFloat(1);
                        }
                    }
                }

                using var cmdUpdate = new NpgsqlCommand(@"
                UPDATE player_mastery
                   SET matchs = @matchs,
                       kda = @mediaKda
                 WHERE puuid = @puuid
                   AND champion_id = @championId", conn);

                cmdUpdate.Parameters.AddWithValue("@matchs", matchs);
                cmdUpdate.Parameters.AddWithValue("@mediaKda", kda);
                cmdUpdate.Parameters.AddWithValue("@puuid", puuid);
                cmdUpdate.Parameters.AddWithValue("@championId", item);

                int rowsAffected = cmdUpdate.ExecuteNonQuery();
            }

            return true;
        }

        public async Task<bool> InsertChampionMasteries(string puuid, List<PlayerMastery> playerMastery)
        {
            var query = @"
            INSERT INTO public.player_mastery (puuid, champion_id, champion_level)
            VALUES (@PUUID, @CHAMPIONID, @CHAMPIONLEVEL)
            ON CONFLICT (puuid, champion_id) 
            DO UPDATE SET champion_level = EXCLUDED.champion_level";

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            try
            {
                foreach (var mastery in playerMastery)
                {
                    await using var command = new NpgsqlCommand(query, connection);
                    command.Parameters.AddWithValue("PUUID", puuid);
                    command.Parameters.AddWithValue("CHAMPIONID", mastery.ChampionId);
                    command.Parameters.AddWithValue("CHAMPIONLEVEL", mastery.ChampionLevel);

                    await command.ExecuteNonQueryAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting champion masteries: {ex.Message}");
                return false;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public async Task<List<PlayerImage>> GetPlayerImages()
        {
            var selectClauses = ChampionConstants.Champions.Keys
                .Select(championId =>
                    $"MAX(CASE WHEN champion_id = '{championId}' THEN champion_level ELSE 0 END) AS champion_{championId}")
                .ToList();

            var sql = @$"
                SELECT  puuid,
                        {string.Join(",\n        ", selectClauses)}
                FROM player_mastery
                GROUP BY puuid
                HAVING COUNT(DISTINCT champion_id) > {ChampionConstants.MIN_COUNT_CONSIDERED};
            ";

            using var connection = new NpgsqlConnection(_connectionString);
            var results = await connection.QueryAsync(sql);

            var playerImages = new List<PlayerImage>();

            foreach (var row in results)
            {
                var championLevels = new int[ChampionConstants.COUNT];

                PlayerImage playerImage = new()
                {
                    Puuid = row.puuid.ToString()
                };
                
                foreach (var (championId, championName) in ChampionConstants.Champions)
                {
                    var columnName = $"champion_{championId}";
                    var level = Convert.ToInt32(((IDictionary<string, object>)row)[columnName]);
                    playerImage.GetType().GetProperty($"Champion_{championId}")?.SetValue(playerImage, level);
                }

                playerImages.Add(playerImage);

            }

            return playerImages;
        }
    }
}