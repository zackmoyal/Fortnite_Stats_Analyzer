using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using FortniteStatsAnalyzer.Models;
using FortniteStatsAnalyzer.Configuration;
using Microsoft.Extensions.Options;

namespace FortniteStatsAnalyzer.Services
{
    public class FortniteStatsService
    {
        private readonly HttpClient _client;
        private readonly string _apiKey;
        private readonly ILogger<FortniteStatsService> _logger;

        public FortniteStatsService(IOptions<FortniteApiSettings> settings, ILogger<FortniteStatsService> logger)
        {
            _apiKey = settings?.Value?.ApiKey?.Trim() ?? throw new InvalidOperationException("Fortnite API key is not set correctly in configuration.");
            _logger = logger;
            _client = new HttpClient();

            _logger.LogWarning("Raw API key from config: {Key}", _apiKey);

            string maskedKey = _apiKey.Length > 8
                ? _apiKey.Substring(0, 4) + "..." + _apiKey.Substring(_apiKey.Length - 4)
                : "***";
            _logger.LogInformation("Initialized FortniteStatsService with API Key: {MaskedKey}", maskedKey);
        }

        public async Task<FortniteStatsResponse?> GetStatsForUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.LogWarning("Empty username provided");
                return null;
            }

            string normalizedUsername = username.Trim();
            _logger.LogInformation("Getting stats for username: {Username}", normalizedUsername);

            if (!normalizedUsername.Equals(username, StringComparison.Ordinal))
            {
                _logger.LogWarning("Username '{Input}' did not match exact case.", username);
                return new FortniteStatsResponse 
                { 
                    Result = false, 
                    Error = $"Username '{username}' must match the exact case registered in Fortnite." 
                };
            }

            var suggestions = await SuggestSimilarUsernames(username);
            if (suggestions.Any())
            {
                string suggestionText = string.Join(", ", suggestions);
                _logger.LogInformation("Did you mean: {Suggestions}?", suggestionText);
                return new FortniteStatsResponse 
                { 
                    Result = false, 
                    Error = $"Username '{username}' not found. Did you mean: {suggestionText}?" 
                };
            }

            var requestUrl = $"https://fortniteapi.io/v1/stats?username={Uri.EscapeDataString(normalizedUsername)}";

            try
            {
                const int maxRetries = 5;
                int currentRetry = 0;

                while (currentRetry < maxRetries)
                {
                    _client.DefaultRequestHeaders.Clear();
                    _client.DefaultRequestHeaders.Add("Authorization", _apiKey);
                    _client.DefaultRequestHeaders.Accept.Clear();
                    _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = await _client.GetAsync(requestUrl);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Raw API response: {Content}", responseContent);

                    if (response.IsSuccessStatusCode)
                    {
                        var stats = JsonConvert.DeserializeObject<FortniteStatsResponse>(responseContent);

                        if (stats == null || !stats.Result)
                        {
                            if (stats?.Error?.Equals("Invalid account", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                return await GetStatsByAccountIdFallback(normalizedUsername);
                            }

                            if (stats?.Error?.Contains("rate", StringComparison.OrdinalIgnoreCase) == true ||
                                stats?.Error?.Contains("limit", StringComparison.OrdinalIgnoreCase) == true ||
                                stats?.Error?.Contains("try again", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                currentRetry++;
                                await Task.Delay(GetExponentialBackoff(currentRetry));
                                continue;
                            }
                            return null;
                        }

                        return stats;
                    }
                    else
                    {
                        if (responseContent.Contains("INVALID_API_KEY", StringComparison.OrdinalIgnoreCase))
                            throw new UnauthorizedAccessException("Invalid API Key");

                        if ((int)response.StatusCode == 429)
                        {
                            currentRetry++;
                            await Task.Delay(GetExponentialBackoff(currentRetry) * 2);
                            continue;
                        }
                        else if ((int)response.StatusCode >= 500)
                        {
                            currentRetry++;
                            await Task.Delay(GetExponentialBackoff(currentRetry));
                            continue;
                        }
                        else if ((int)response.StatusCode == 401 || (int)response.StatusCode == 403)
                        {
                            return null;
                        }
                    }

                    currentRetry++;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while retrieving stats for user: {Username}", normalizedUsername);
                return null;
            }
        }

        private async Task<FortniteStatsResponse?> GetStatsByAccountIdFallback(string username)
        {
            _logger.LogInformation("Trying fallback lookup for username: {Username}", username);
            var lookupUrl = $"https://fortniteapi.io/v1/lookup?username={Uri.EscapeDataString(username)}";
            var lookupResponse = await _client.GetAsync(lookupUrl);
            var lookupContent = await lookupResponse.Content.ReadAsStringAsync();
            _logger.LogWarning("Lookup API response: {LookupContent}", lookupContent);

            dynamic lookupData = JsonConvert.DeserializeObject(lookupContent);
            string accountId = lookupData?.account_id;

            if (!string.IsNullOrWhiteSpace(accountId))
            {
                string idStatsUrl = $"https://fortniteapi.io/v1/stats?account={accountId}";
                var idStatsResponse = await _client.GetAsync(idStatsUrl);
                var idStatsContent = await idStatsResponse.Content.ReadAsStringAsync();
                _logger.LogWarning("ID-based stats response: {StatsById}", idStatsContent);

                var idStats = JsonConvert.DeserializeObject<FortniteStatsResponse>(idStatsContent);
                if (idStats?.Result == true)
                    return idStats;
            }

            return null;
        }

        private Task<string[]> SuggestSimilarUsernames(string input)
        {
            string[] known = ["ZbiZniZ", "Tfue", "Bugha", "Ninja"];
            var suggestions = known
                .Where(x => x.StartsWith(input, StringComparison.OrdinalIgnoreCase) && !x.Equals(input, StringComparison.Ordinal))
                .ToArray();
            return Task.FromResult(suggestions);
        }

        private int GetExponentialBackoff(int retryAttempt)
        {
            int baseDelay = 1000;
            Random random = new Random();
            double jitter = random.NextDouble() * 0.3 + 0.85;
            return (int)(baseDelay * Math.Pow(2, retryAttempt - 1) * jitter);
        }
    }
}
