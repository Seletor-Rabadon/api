namespace api.Classes;

public class Match
{
    public MetadataDto? Metadata { get; set; }
    public InfoDto? Info { get; set; }
}

public class MetadataDto
{
    public string? DataVersion { get; set; }
    public string? MatchId { get; set; }
    public List<string>? Participants { get; set; }
}

public class InfoDto
{
    public string? EndOfGameResult { get; set; }
    public long GameCreation { get; set; }
    public long GameDuration { get; set; }
    public long GameEndTimestamp { get; set; }
    public long GameId { get; set; }
    public string? GameMode { get; set; }
    public string? GameName { get; set; }
    public long GameStartTimestamp { get; set; }
    public string? GameType { get; set; }
    public string? GameVersion { get; set; }
    public int MapId { get; set; }
    public List<ParticipantDto>? Participants { get; set; }
    public string? PlatformId { get; set; }
    public int QueueId { get; set; }
    public List<TeamDto>? Teams { get; set; }
    public string? TournamentCode { get; set; }
}

public class ParticipantDto
{
    public int AllInPings { get; set; }
    public int AssistMePings { get; set; }
    public int Assists { get; set; }
    public int BaronKills { get; set; }
    public int BountyLevel { get; set; }
    public int ChampExperience { get; set; }
    public int ChampLevel { get; set; }
    public int ChampionId { get; set; }
    public string? ChampionName { get; set; }
    public int CommandPings { get; set; }
    public int ChampionTransform { get; set; }
    public int ConsumablesPurchased { get; set; }
    public ChallengesDto? Challenges { get; set; }
    public int DamageDealtToBuildings { get; set; }
    public int DamageDealtToObjectives { get; set; }
    public int DamageDealtToTurrets { get; set; }
    public int DamageSelfMitigated { get; set; }
    public int Deaths { get; set; }
    public int DetectorWardsPlaced { get; set; }
    public int DoubleKills { get; set; }
    public int DragonKills { get; set; }
    public bool EligibleForProgression { get; set; }
    public bool FirstBloodAssist { get; set; }
    public bool FirstBloodKill { get; set; }
    public bool FirstTowerAssist { get; set; }
    public bool FirstTowerKill { get; set; }
    public bool GameEndedInEarlySurrender { get; set; }
    public bool GameEndedInSurrender { get; set; }
    public int GoldEarned { get; set; }
    public int GoldSpent { get; set; }
    public string? IndividualPosition { get; set; }
    public int InhibitorKills { get; set; }
    public int InhibitorTakedowns { get; set; }
    public int InhibitorsLost { get; set; }
    public int Item0 { get; set; }
    public int Item1 { get; set; }
    public int Item2 { get; set; }
    public int Item3 { get; set; }
    public int Item4 { get; set; }
    public int Item5 { get; set; }
    public int Item6 { get; set; }
    public int ItemsPurchased { get; set; }
    public int KillingSprees { get; set; }
    public int Kills { get; set; }
    public string? Lane { get; set; }
    public int LargestCriticalStrike { get; set; }
    public int LargestKillingSpree { get; set; }
    public int LargestMultiKill { get; set; }
    public int LongestTimeSpentLiving { get; set; }
    public int MagicDamageDealt { get; set; }
    public int MagicDamageDealtToChampions { get; set; }
    public int MagicDamageTaken { get; set; }
    public MissionsDto? Missions { get; set; }
    public int NeutralMinionsKilled { get; set; }
    public int NexusKills { get; set; }
    public int NexusTakedowns { get; set; }
    public int NexusLost { get; set; }
    public int ObjectivesStolen { get; set; }
    public int ObjectivesStolenAssists { get; set; }
    public int ParticipantId { get; set; }
    public string? Puuid { get; set; }
    public int QuadraKills { get; set; }
    public string? SummonerName { get; set; }
    public bool Win { get; set; }
}

public class ChallengesDto
{
    public int AssistStreakCount { get; set; }
    public float MaxCsAdvantageOnLaneOpponent { get; set; }
}

public class MissionsDto
{
    public int PlayerScore0 { get; set; }
    public int PlayerScore1 { get; set; }
}

public class TeamDto
{
    public List<BanDto>? Bans { get; set; }
    public ObjectivesDto? Objectives { get; set; }
    public int TeamId { get; set; }
    public bool Win { get; set; }
}

public class BanDto
{
    public int ChampionId { get; set; }
    public int PickTurn { get; set; }
}

public class ObjectivesDto
{
    public ObjectiveDto? Baron { get; set; }
    public ObjectiveDto? Dragon { get; set; }
    public ObjectiveDto? Tower { get; set; }
}

public class ObjectiveDto
{
    public bool First { get; set; }
    public int Kills { get; set; }
}

