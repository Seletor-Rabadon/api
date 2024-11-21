using System.Diagnostics;
using api.Interfaces;
using System.Text.Json;
using api.Classes;

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
                WorkingDirectory = Directory.GetCurrentDirectory() + "/AI"
            };

            using Process? process = Process.Start(start) ?? throw new Exception("Failed to start Python process");
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (!string.IsNullOrEmpty(error))
            {
                throw new Exception($"Python error: {error}");
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
    }
}