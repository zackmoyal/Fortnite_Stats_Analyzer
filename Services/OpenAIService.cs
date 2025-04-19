using System.Text.Json;
using System.Text;

namespace FortniteStatsAnalyzer.Services
{
    public interface IOpenAIService
    {
        Task<string> GenerateStatsFeedback(double kd, double winrate, int topPlacements, int totalKills, int matchesPlayed, string gameMode);
    }

    public class OpenAIService : IOpenAIService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private const string API_URL = "https://api.openai.com/v1/chat/completions";

        public OpenAIService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuration["OpenAISettings:ApiKey"]}");
        }

        public async Task<string> GenerateStatsFeedback(double kd, double winrate, int topPlacements, int totalKills, int matchesPlayed, string gameMode)
        {
            var systemPrompt = @"You are an expert Fortnite coach analyzing player statistics. 
            Your feedback should be comprehensive, specific, actionable, and encouraging.
            Use Fortnite terminology naturally (e.g., Victory Royales, rotations, box fights, piece control, storm surge, tarping, tunneling).
            Focus on both mechanical skills (building, editing, aiming) and game sense (positioning, rotations, decision-making, storm awareness).
            Tailor advice based on their skill level shown in the stats.";

            var userPrompt = $@"Analyze these Fortnite stats for {gameMode}:
            - K/D Ratio: {kd:F2}
            - Win Rate: {(winrate * 100):F1}%
            - Victory Royales: {topPlacements}
            - Total Kills: {totalKills}
            - Matches Played: {matchesPlayed}

            Provide detailed feedback (4-9 sentences) that:
            1. Start with an overview of their playstyle based on stats
            2. Highlight their strongest aspects (K/D, wins, etc.)
            3. Analyze their combat effectiveness
            4. Discuss their survival and winning strategies
            5. Give 2-3 specific, actionable tips for improvement
            6. End with an encouraging note about their potential
            7. Use proper grammar and capitalization
            8. Keep a positive, coaching tone throughout";

            var requestBody = new
            {
                model = "gpt-4.1-mini",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = 0.7,
                max_tokens = 250,
                presence_penalty = 0.1,
                frequency_penalty = 0.1
            };

            var response = await _httpClient.PostAsync(
                API_URL,
                new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Knocked! The Battle Bus lost connection. Ready up and try dropping in again!");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);
            var feedback = responseObject.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            
            if (string.IsNullOrEmpty(feedback))
            {
                throw new Exception("Eliminated! The storm surge knocked us out. Head back to the lobby and queue up again!");
            }

            return feedback;
        }
    }
} 