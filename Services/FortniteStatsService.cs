using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using FortniteStatsAnalyzer.Models;
using FortniteStatsAnalyzer.Configuration;
using Microsoft.Extensions.Options;

namespace FortniteStatsAnalyzer.Services
{
    public class FortniteStatsService(IOptions<FortniteApiSettings> settings, ILogger<FortniteStatsService> logger)
    {
        private readonly HttpClient _client = new();
        private readonly string _apiKey = settings.Value.ApiKey;
        private readonly ILogger<FortniteStatsService> _logger = logger;

        public async Task<FortniteStatsResponse?> GetStatsForUser(string username)
        {
            var requestUrl = $"https://fortniteapi.io/v1/stats?username={username}";
            _client.DefaultRequestHeaders.Clear(); // Clear previous headers to avoid duplicates
            _client.DefaultRequestHeaders.Add("Authorization", _apiKey);

            try
            {
                var response = await _client.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    // Deserialize the JSON response
                    var stats = JsonConvert.DeserializeObject<FortniteStatsResponse>(jsonResponse);

                    // Log the raw JSON response for debugging
                    _logger.LogInformation("API Response: {ApiResponse}", jsonResponse);

                    // Check if the API explicitly states the username is invalid
                    if (stats != null && !stats.Result)
                    {
                        _logger.LogWarning("API Response indicates invalid username: {Error}", stats.Error);
                        return null; // Invalid username
                    }

                    // Log the deserialized stats object for debugging
                    _logger.LogInformation("Deserialized stats object: {@Stats}", stats);

                    return stats;
                }

                _logger.LogWarning("Failed to retrieve stats for user: {Username}. Status Code: {StatusCode}", username, response.StatusCode);
                return null; // Return null if the API call fails
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving stats for user: {Username}", username);
                return null; // Return null if an exception occurs
            }
        }
    }
}
