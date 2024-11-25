using api.Classes;

namespace api.Interfaces
{
    public interface IRiotService
    {
        Task<Profile> GetGameProfile(string gameName, string tagLine);
        Task<UserResponse> GetUserByRiotId(string gameName, string tagLine);
        Task<List<string>> GetMatchHistory(string puuid);
        Task<List<PlayerMastery>> GetChampionMasteries(string puuid);
        Task<Match> GetMatchData(string matchId);
        Task<string> GetNextPlayer(string puuid); 
        Task<UserResponse> GetPlayerByPuuid(string puuid);
    }
} 