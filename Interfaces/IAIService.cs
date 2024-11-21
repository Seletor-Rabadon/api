using api.Classes;

namespace api.Interfaces
{
    public interface IAIService
    {
        Task<TrainingResult> Train();
    }
} 