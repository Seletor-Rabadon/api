using api.Classes;

namespace api.Interfaces
{
    public interface IDataBaseService
    {
        Task<bool> InsertPlayer(UserResponse player);

        Task<bool> InsertMatchAsync(MatchChampion matchChampion);

        Task<bool> UpdatePlayerDataAsync(string puuid);
        Task<bool> InsertChampionMasteries(string puuid, List<PlayerMastery> playerMastery); 
    }
}