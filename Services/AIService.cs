using System.Diagnostics;
using api.Interfaces;
using System.IO;
using System.Text.Json;
using api.Classes;

namespace api.Services
{
    public class AIService(IConfiguration configuration) : IAIService
    {
        private readonly IConfiguration _configuration = configuration;        
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
        private static string ExecutePythonScript(string scriptPath)
        {
            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = scriptPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Directory.GetCurrentDirectory()
            };

            using Process? process = Process.Start(start) ?? throw new Exception("Failed to start Python process");
            string result = process.StandardOutput.ReadToEnd().Trim();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (!string.IsNullOrEmpty(error))
            {
                throw new Exception($"Python error: {error}");
            }

            if (string.IsNullOrEmpty(result))
            {
                throw new Exception("No output received from Python script");
            }

            return result;
        }

        public TrainingResult Train()
        {
            string jsonResult = ExecutePythonScript("AI/autoencoder.py");

            try
            { 

                var trainingResult = JsonSerializer.Deserialize<TrainingResult>(jsonResult, _jsonOptions)
                    ?? throw new Exception("Failed to deserialize training result");

                return trainingResult;
            }
            catch (JsonException ex)
            {
                throw new Exception($"Failed to parse Python output as JSON: {ex.Message}");
            }
        }
    }
}