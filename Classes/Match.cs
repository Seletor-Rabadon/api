public class Match
{
    public Metadata Metadata { get; set; }
    public MatchInfo Info { get; set; }
}

public class Metadata
{
    public string DataVersion { get; set; }
    public string MatchId { get; set; }  // Match ID as a string
}

public class MatchInfo
{
    public long GameId { get; set; }
    public int MapId { get; set; }
    public string GameMode { get; set; }
    public string GameType { get; set; }
    public int GameDuration { get; set; }  // Duration in seconds
    public List<Team> Teams { get; set; }
    public List<Participant> Participants { get; set; }
}

public class Team
{
    public int TeamId { get; set; }
    public bool Win { get; set; }
    public List<TeamParticipant> Participants { get; set; }
}

public class TeamParticipant
{
    public string Puuid { get; set; }
    public int ChampionId { get; set; }
    public int TeamId { get; set; }
    public string TeamPosition { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public bool Win { get; set; }
}

public class Participant
{
    public string Puuid { get; set; }
    public int ChampionId { get; set; }
    public int TeamId { get; set; }
    public string TeamPosition { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public bool Win { get; set; }
}