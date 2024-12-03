namespace api.Classes
{
    public class PredictionResult
    {
        public string Puuid { get; set; } = string.Empty;
        public double Reconstruction_error { get; set; }
        public List<double> Original_values { get; set; } = new();
        public List<double> Reconstructed_values { get; set; } = new(); 
    }
} 