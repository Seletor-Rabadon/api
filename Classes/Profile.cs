namespace api.Classes
{
    public class Profile
    {
        public required string Id { get; set; }
        public required string AccountId { get; set; }
        public required string Puuid { get; set; }
        public int ProfileIconId { get; set; }
        public long RevisionDate { get; set; }
        public int SummonerLevel { get; set; }
    }
}