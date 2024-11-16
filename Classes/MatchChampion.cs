namespace api.Classes
{
    public class MatchChampion
    {
        public required string MatchId { get; set; }
        public required string Puuid { get; set; }
        public int ChampionId { get; set; }
        public required string Position { get; set; }
        public float Kda { get; set; }
        public bool Win { get; set; }
    }
}