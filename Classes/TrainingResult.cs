namespace api.Classes
{
    public class TrainingResult
    {
        public TrainingHistory TrainingHistory { get; set; } = new();
        public List<ReconstructionResult> ReconstructionResults { get; set; } = new();
        public ModelSummary ModelSummary { get; set; } = new();
    }

    public class TrainingHistory
    {
        public List<decimal> Loss { get; set; } = new();
        public List<decimal> ValLoss { get; set; } = new();
    }

    public class ReconstructionResult
    {
        public string Puuid { get; set; } = string.Empty;
        public decimal ReconstructionError { get; set; }
        public List<decimal> OriginalValues { get; set; } = new();
        public List<decimal> ReconstructedValues { get; set; } = new();
    }

    public class ModelSummary
    {
        public int InputDim { get; set; }
        public int LatentDim { get; set; }
        public double FinalLoss { get; set; }
        public double FinalValLoss { get; set; }
    }
}