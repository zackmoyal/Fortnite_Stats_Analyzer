using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

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
        private readonly ILogger<OpenAIService> _logger;
        private const string API_URL = "https://api.openai.com/v1/chat/completions";
        private readonly string _apiKey;

        public OpenAIService(IConfiguration configuration, HttpClient httpClient, ILogger<OpenAIService> logger)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;
            
            _apiKey = _configuration["OpenAISettings:ApiKey"];
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("OpenAI API key is not configured");
            }

            // Set up headers for OpenAI API
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> GenerateStatsFeedback(double kd, double winrate, int topPlacements, int totalKills, int matchesPlayed, string gameMode)
        {
            try
            {
                var systemPrompt = @"You are an expert Fortnite coach analyzing player statistics. 
                Provide personalized, specific feedback based on their stats.
                Consider:
                - K/D ratio trends and combat effectiveness
                - Win rate and strategic decision-making
                - Match volume and experience level
                - Game mode specific strategies
                - Recent meta changes and optimal strategies
                
                Use authentic Fortnite terminology like:
                - Box fights, piece control, storm surge
                - Rotations, tarping, tunneling
                - Early/mid/late game strategies
                - Zone positioning, height control
                - Mechanical skills vs game sense
                
                Keep feedback encouraging but honest.";

                var userPrompt = $@"Analyze these Fortnite {gameMode} stats:
                K/D: {kd:F2}
                Win Rate: {(winrate * 100):F1}%
                Wins: {topPlacements}
                Kills: {totalKills}
                Matches: {matchesPlayed}

                Provide detailed feedback that:
                1. Evaluates their skill level (beginner/intermediate/advanced)
                2. Analyzes combat effectiveness (K/D ratio analysis)
                3. Reviews strategic success (win rate analysis)
                4. Suggests 2-3 specific areas for improvement
                5. Gives mode-specific tips for {gameMode}
                6. Ends with an encouraging note

                Format as 3-4 concise paragraphs with natural flow.
                Use proper grammar and Fortnite terminology.
                Be specific and actionable in advice.";

                _logger.LogInformation("Generating feedback for {GameMode} stats", gameMode);

                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    temperature = 0.7,
                    max_tokens = 500
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                _logger.LogDebug("OpenAI Request: {Request}", jsonContent);

                // Create a new request message
                var request = new HttpRequestMessage(HttpMethod.Post, API_URL)
                {
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                };

                // Ensure headers are set correctly for this specific request
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("OpenAI Response: {Response}", responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("OpenAI API error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    throw new Exception($"OpenAI API error: {response.StatusCode}");
                }

                using JsonDocument doc = JsonDocument.Parse(responseContent);
                var completion = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrEmpty(completion))
                {
                    throw new Exception("Empty feedback received from OpenAI API");
                }

                _logger.LogInformation("Successfully generated feedback for {GameMode}", gameMode);
                return completion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating feedback: {Message}", ex.Message);
                return $"Unable to generate AI feedback at this time. Here's a basic analysis for your {gameMode} stats:\n\n" +
                       $"Your K/D ratio of {kd:F2} and win rate of {(winrate * 100):F1}% show your current performance level. " +
                       $"With {matchesPlayed} matches played and {topPlacements} wins, keep practicing to improve your skills. " +
                       "Focus on building mechanics, positioning, and game awareness to enhance your gameplay.";
            }
        }
    }
}