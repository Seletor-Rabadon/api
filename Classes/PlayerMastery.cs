namespace api.Classes
{
    public class PlayerMastery
    {
        public string? Puuid { get; set; } // Player Universal Unique Identifier
        public long ChampionPointsUntilNextLevel { get; set; } // Points needed for next level
        public bool ChestGranted { get; set; } // Is chest granted in current season
        public long ChampionId { get; set; } // Champion ID
        public long LastPlayTime { get; set; } // Last play time in Unix milliseconds
        public int ChampionLevel { get; set; } // Champion level
        public int ChampionPoints { get; set; } // Total champion points
        public long ChampionPointsSinceLastLevel { get; set; } // Points since last level
        public int MarkRequiredForNextLevel { get; set; } // Marks required for next level
        public int ChampionSeasonMilestone { get; set; } // Champion season milestone
        public NextSeasonMilestonesDto? NextSeasonMilestone { get; set; } // Next season milestone details
        public int TokensEarned { get; set; } // Tokens earned at current level
        public List<string>? MilestoneGrades { get; set; } // List of milestone grades
    }

    public class NextSeasonMilestonesDto
    {
        public object? RequireGradeCounts { get; set; } // Object representing required grade counts
        public int RewardMarks { get; set; } // Reward marks
        public bool Bonus { get; set; } // Indicates if there is a bonus
        public RewardConfigDto? RewardConfig { get; set; } // Reward configuration details
    }

    public class RewardConfigDto
    {
        public string? RewardValue { get; set; } // Reward value
        public string? RewardType { get; set; } // Reward type
        public int MaximumReward { get; set; } // Maximum reward
    }
}