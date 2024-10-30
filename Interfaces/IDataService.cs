using Microsoft.AspNetCore.Mvc;

public interface IDataService
{
    Task<bool> InsertPlayerAsync(string puuid, string userName, string tagLine);
}