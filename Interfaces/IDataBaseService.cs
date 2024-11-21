using api.Classes;

namespace api.Interfaces
{
    public interface IDataBaseService
    {
        Task<bool> InsertPlayerAsync(string puuid, string userName, string tagLine);
        Task<bool> InsertPlayerMasteryAsync(string puuid, long championId, int championLevel);

        Task<bool> InsertMatchAsync(MatchChampion matchChampion);

        Task<bool> UpdatePlayerDataAsync(string puuid);
    }
}