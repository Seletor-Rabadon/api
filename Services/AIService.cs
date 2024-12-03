using System.Diagnostics;
using api.Interfaces;
using System.Text.Json;
using api.Classes;
using api.Constants;

namespace api.Services
{
    public class AIService(IConfiguration configuration) : IAIService
    {
        private readonly IConfiguration _configuration = configuration;
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        private static void ExecutePythonScript(string scriptPath)
        {
            ProcessStartInfo start = new()
            {
                FileName = "python",
                Arguments = scriptPath,
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Directory.GetCurrentDirectory() + "/AI",
                Environment =
                {
                    ["TF_CPP_MIN_LOG_LEVEL"] = "2",
                    ["TF_ENABLE_ONEDNN_OPTS"] = "0"
                }
            };

            using Process? process = Process.Start(start) ?? throw new Exception("Failed to start Python process");
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (!string.IsNullOrEmpty(error))
            {
                var errorLines = error.Split('\n')
                    .Where(line => !line.Contains("tensorflow") && 
                                  !string.IsNullOrWhiteSpace(line))
                    .ToList();

                if (errorLines.Any())
                {
                    throw new Exception($"Python error: {string.Join("\n", errorLines)}");
                }
            }
        }

        public async Task<TrainingResult> Train()
        {
            // Execute Python script which generates the result file
            await Task.Run(() => ExecutePythonScript("autoencoder.py"));

            // Read the generated result file
            string resultFilePath = Path.Combine(Directory.GetCurrentDirectory(), "AI", "training_result.json");

            if (!File.Exists(resultFilePath))
            {
                throw new Exception("Training result file was not generated");
            }

            try
            {
                string jsonResult = File.ReadAllText(resultFilePath);
                var trainingResult = JsonSerializer.Deserialize<TrainingResult>(jsonResult, _jsonOptions)
                    ?? throw new Exception("Failed to deserialize training result");

                return trainingResult;
            }
            catch (JsonException ex)
            {
                throw new Exception($"Failed to parse training result file as JSON: {ex.Message}");
            }
            catch (IOException ex)
            {
                throw new Exception($"Failed to read training result file: {ex.Message}");
            }
        }
        public async Task<PlayerImage> Predict(PlayerImage playerImage)
        {
            // Generate prediction data file
            var aiFolder = Path.Combine(Directory.GetCurrentDirectory(), "AI");
            var filePath = Path.Combine(aiFolder, "prediction_data.csv");
            
            // Create the data file
            using (var writer = new StreamWriter(filePath))
            {
                var line = playerImage.Puuid;
                
                for (int i = 1; i <= ChampionConstants.COUNT; i++)
                {
                    var championLevel = playerImage.GetType().GetProperty($"Champion_{i}")?.GetValue(playerImage) ?? 0;
                    line += $";{championLevel}";
                }
                
                await writer.WriteLineAsync(line);
            }
            
            // Execute Python prediction script
            await Task.Run(() => ExecutePythonScript("predictor.py"));
            
            // Read the prediction result
            string resultFilePath = Path.Combine(Directory.GetCurrentDirectory(), "AI", "prediction_result.json");
            
            if (!File.Exists(resultFilePath))
            {
                throw new Exception("Prediction result file was not generated");
            }
            
            try
            {
                string jsonResult = await File.ReadAllTextAsync(resultFilePath); 
                var result = JsonSerializer.Deserialize<PredictionResult>(jsonResult, _jsonOptions)
                    ?? throw new Exception("Failed to deserialize prediction result"); 
                // Convert prediction result back to PlayerImage
                var predictedImage = new PlayerImage { Puuid = playerImage.Puuid };
                var reconstructedValues = result.Reconstructed_values; 
                
                // Add null check and length validation
                if (reconstructedValues == null || reconstructedValues.Count != ChampionConstants.COUNT)
                {
                    throw new Exception($"Invalid prediction result: Expected {ChampionConstants.COUNT} values but got {reconstructedValues?.Count ?? 0}");
                } 
                
                for (int i = 0; i < ChampionConstants.COUNT; i++)
                {
                    var property = predictedImage.GetType().GetProperty($"Champion_{i + 1}")
                        ?? throw new Exception($"Property Champion_{i + 1} not found");
                    
                        
                    property.SetValue(predictedImage, reconstructedValues[i]);
                }
                
                return predictedImage;
            }
            catch (JsonException ex)
            {
                throw new Exception($"Failed to parse prediction result file as JSON: {ex.Message}");
            }
            catch (IOException ex)
            {
                throw new Exception($"Failed to read prediction result file: {ex.Message}");
            }
        }
    }

}