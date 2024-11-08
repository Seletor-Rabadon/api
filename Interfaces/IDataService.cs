using Microsoft.AspNetCore.Mvc;

public interface IDataService
{
    Task<bool> InsertPlayerAsync(string puuid, string userName, string tagLine);
    Task<bool> InsertPlayerMasteryAsync(string puuid, int championId, int championLevel);
}