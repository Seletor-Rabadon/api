namespace api.Classes
{
    public class Match
    {
        public required Metadata Metadata { get; set; }
        public required MatchInfo Info { get; set; }
    }

    public class Metadata
    {
        public required string DataVersion { get; set; }
        public required string MatchId { get; set; }  // Match ID as a string
    }

    public class MatchInfo
    {
        public long GameId { get; set; }
        public int MapId { get; set; }
        public required string GameMode { get; set; }
        public required string GameType { get; set; }
        public int GameDuration { get; set; }  // Duration in seconds
        public required List<Team> Teams { get; set; }
        public required List<Participant> Participants { get; set; }
    }

    public class Team
    {
        public int TeamId { get; set; }
        public bool Win { get; set; }
        public required List<TeamParticipant> Participants { get; set; }
    }

    public class TeamParticipant
    {
        public required string Puuid { get; set; }
        public int ChampionId { get; set; }
        public int TeamId { get; set; }
        public required string TeamPosition { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public bool Win { get; set; }
    }

    public class Participant
    {
        public required string Puuid { get; set; }
        public int ChampionId { get; set; }
        public int TeamId { get; set; }
        public required string TeamPosition { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public bool Win { get; set; }
    }
}