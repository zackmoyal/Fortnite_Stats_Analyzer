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
    public interface IFortniteApiService
    {
        Task<FortniteStatsResponse?> GetStatsForUser(string username);
    }

    public class FortniteApiService : IFortniteApiService
    {
        private readonly HttpClient _client;
        private readonly string _fortniteApiKey;
        private readonly ILogger<FortniteApiService> _logger;

        public FortniteApiService(
            IOptions<FortniteApiSettings> fortniteSettings,
            ILogger<FortniteApiService> logger)
        {
            _fortniteApiKey = fortniteSettings?.Value?.ApiKey?.Trim() ?? throw new InvalidOperationException("Fortnite API key is not set correctly in configuration.");
            _logger = logger;
            _client = new HttpClient();
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
                const int maxRetries = 3;
                int currentRetry = 0;

                while (currentRetry < maxRetries)
                {
                    _client.DefaultRequestHeaders.Clear();
                    _client.DefaultRequestHeaders.Add("Authorization", _fortniteApiKey);
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

                return new FortniteStatsResponse 
                { 
                    Result = false, 
                    Error = "Unable to retrieve stats after multiple attempts. Please try again later." 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Fortnite stats for user {Username}", normalizedUsername);
                return new FortniteStatsResponse 
                { 
                    Result = false, 
                    Error = "An error occurred while retrieving stats. Please try again later." 
                };
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
            return (int)Math.Pow(2, retryAttempt) * 1000;
        }
    }
} 