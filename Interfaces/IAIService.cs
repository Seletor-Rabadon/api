using api.Classes;

namespace api.Interfaces
{
    public interface IAIService
    {
        Task<TrainingResult> Train();
        Task<PlayerImage> Predict(PlayerImage playerImage);
    }
} 