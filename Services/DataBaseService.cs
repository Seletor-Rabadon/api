using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;

public class DatabaseService : IDataService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("PostgresConnection");
    }

    public async Task<bool> InsertPlayerAsync(string puuid, string userName, string tagLine)
    {

        var query = "INSERT INTO public.player (puuid, game_name, tag_line) VALUES (@PUUID, @GAMENAME, @TAGLINE)";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);
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
            Console.WriteLine($"Erro ao inserir dados: {ex.Message}");
            return false;
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}