using api.Classes;

namespace api.Interfaces
{
    public interface IRiotService
    {
        Task<Profile> GetGameProfile(string gameName, string tagLine);
        Task<UserResponse> GetUserByRiotId(string gameName, string tagLine);
        Task<List<string>> GetMatchHistory(string puuid, int count = 100);
        Task<List<PlayerMastery>> GetChampionMasteries(string puuid, int championCount, string server);
        Task<Match> GetMatchData(string matchId);
    }
} 