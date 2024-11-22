using api.Classes;
using api.Interfaces;
using Npgsql;

namespace api.Services
{
    public class DatabaseService(IConfiguration configuration) : IDataBaseService
    {
        private readonly string _connectionString = configuration.GetConnectionString("PostgresConnection")
                ?? throw new ArgumentException("PostgresConnection string is not configured");

        public async Task<bool> InsertPlayerAsync(string puuid, string userName, string tagLine)
        {
            var checkQuery = "SELECT COUNT(*) FROM public.player WHERE puuid = @PUUID";
            var insertQuery = "INSERT INTO public.player (puuid, game_name, tag_line) VALUES (@PUUID, @GAMENAME, @TAGLINE)";
            var updateQuery = "UPDATE public.player SET game_name = @GAMENAME, tag_line = @TAGLINE WHERE puuid = @PUUID";

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var checkCommand = new NpgsqlCommand(checkQuery, connection);
            checkCommand.Parameters.AddWithValue("PUUID", puuid);
            
            var exists = await checkCommand.ExecuteScalarAsync() as long? > 0 || false;

            await using var command = new NpgsqlCommand(exists ? updateQuery : insertQuery, connection);
            command.Parameters.AddWithValue("PUUID", puuid);
            command.Parameters.AddWithValue("GAMENAME", userName);
            command.Parameters.AddWithValue("TAGLINE", tagLine);

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

        public async Task<bool> InsertPlayerMasteryAsync(string puuid, long championId, int championLevel)
        {
            var checkQuery = "SELECT COUNT(*) FROM public.player_mastery WHERE puuid = @PUUID AND champion_id = @CHAMPIONID";
            var insertQuery = "INSERT INTO public.player_mastery (puuid, champion_id, champion_level) VALUES (@PUUID, @CHAMPIONID, @CHAMPIONLEVEL)";
            var updateQuery = "UPDATE public.player_mastery SET champion_level = @CHAMPIONLEVEL WHERE puuid = @PUUID AND champion_id = @CHAMPIONID";

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var checkCommand = new NpgsqlCommand(checkQuery, connection);
            checkCommand.Parameters.AddWithValue("PUUID", puuid);
            checkCommand.Parameters.AddWithValue("CHAMPIONID", championId);
            
            var exists = await checkCommand.ExecuteScalarAsync() as long? > 0 || false;

            await using var command = new NpgsqlCommand(exists ? updateQuery : insertQuery, connection);
            command.Parameters.AddWithValue("PUUID", puuid);
            command.Parameters.AddWithValue("CHAMPIONID", championId);
            command.Parameters.AddWithValue("CHAMPIONLEVEL", championLevel);

            try
            {
                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while {(exists ? "updating" : "inserting")} mastery data: {ex.Message}");
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
    }
}