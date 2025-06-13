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
        private const string BASE_URL = "https://fortniteapi.io/v1";

        public FortniteApiService(
            IOptions<FortniteApiSettings> fortniteSettings,
            ILogger<FortniteApiService> logger,
            HttpClient httpClient)
        {
            _fortniteApiKey = fortniteSettings?.Value?.ApiKey?.Trim() ?? throw new InvalidOperationException("Fortnite API key is not set correctly in configuration.");
            _logger = logger;
            _client = httpClient;

            // Set up the headers once in constructor
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Authorization", _fortniteApiKey);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // Parameterless HttpClient constructor for backward compatibility and simpler instantiation
        public FortniteApiService(
            IOptions<FortniteApiSettings> fortniteSettings,
            ILogger<FortniteApiService> logger)
            : this(fortniteSettings, logger, new HttpClient())
        {
        }

        public async Task<FortniteStatsResponse?> GetStatsForUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.LogWarning("Empty username provided");
                return new FortniteStatsResponse { Result = false, Error = "Username cannot be empty" };
            }

            string normalizedUsername = username.Trim();
            _logger.LogInformation("Getting stats for username: {Username}", normalizedUsername);

            // Try exact username first
            var stats = await TryGetStats(normalizedUsername);
            if (stats?.Result == true)
            {
                _logger.LogInformation("Found stats with exact username match");
                return stats;
            }

            // If exact match fails, try with the original case
            if (stats?.Error == "Invalid account")
            {
                _logger.LogInformation("Trying account lookup for username");
                var accountStats = await TryGetStatsByAccountId(normalizedUsername);
                if (accountStats?.Result == true)
                {
                    return accountStats;
                }
            }

            // If both methods fail, try case variations
            var variations = new[]
            {
                normalizedUsername.ToLower(),
                normalizedUsername.ToUpper(),
                char.ToUpper(normalizedUsername[0]) + normalizedUsername.Substring(1).ToLower()
            };

            foreach (var variation in variations)
            {
                if (variation == normalizedUsername) continue; // Skip if we already tried this

                _logger.LogInformation("Trying variation: {Variation}", variation);
                stats = await TryGetStats(variation);
                if (stats?.Result == true)
                {
                    _logger.LogInformation("Found stats with username variation: {Variation}", variation);
                    return stats;
                }

                if (stats?.Error == "Invalid account")
                {
                    var accountStats = await TryGetStatsByAccountId(variation);
                    if (accountStats?.Result == true)
                    {
                        return accountStats;
                    }
                }

                // Add delay between requests to avoid rate limiting
                await Task.Delay(1000);
            }

            // If all attempts fail, return an error
            return new FortniteStatsResponse
            {
                Result = false,
                Error = "No player stats available. Please check the username and try again."
            };
        }

        private async Task<FortniteStatsResponse?> TryGetStats(string username)
        {
            try
            {
                var requestUrl = $"{BASE_URL}/stats?username={Uri.EscapeDataString(username)}";
                _logger.LogDebug("Making request to: {Url}", requestUrl);

                var response = await _client.GetAsync(requestUrl);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                _logger.LogDebug("Stats API response for {Username}: {Content}", username, responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode == 429)
                    {
                        _logger.LogWarning("Rate limit hit, waiting before retry");
                        await Task.Delay(2000);
                        return null;
                    }
                    return null;
                }

                var stats = JsonConvert.DeserializeObject<FortniteStatsResponse>(responseContent);
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stats for username {Username}", username);
                return null;
            }
        }

        private async Task<FortniteStatsResponse?> TryGetStatsByAccountId(string username)
        {
            try
            {
                var lookupUrl = $"{BASE_URL}/lookup?username={Uri.EscapeDataString(username)}";
                _logger.LogDebug("Making lookup request to: {Url}", lookupUrl);

                var lookupResponse = await _client.GetAsync(lookupUrl);
                var lookupContent = await lookupResponse.Content.ReadAsStringAsync();
                
                _logger.LogDebug("Lookup API response for {Username}: {Content}", username, lookupContent);

                if (!lookupResponse.IsSuccessStatusCode)
                {
                    return null;
                }

                dynamic? lookupData = JsonConvert.DeserializeObject(lookupContent);
                string? accountId = lookupData?.account_id;

                if (string.IsNullOrWhiteSpace(accountId))
                {
                    return null;
                }

                var statsUrl = $"{BASE_URL}/stats?account={accountId}";
                _logger.LogDebug("Making stats request by account ID to: {Url}", statsUrl);

                var statsResponse = await _client.GetAsync(statsUrl);
                var statsContent = await statsResponse.Content.ReadAsStringAsync();
                
                _logger.LogDebug("Account stats response for {Username}: {Content}", username, statsContent);

                if (!statsResponse.IsSuccessStatusCode)
                {
                    return null;
                }

                var stats = JsonConvert.DeserializeObject<FortniteStatsResponse>(statsContent);
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in account lookup for username {Username}", username);
                return null;
            }
        }

        private Task<string[]> SuggestSimilarUsernames(string input)
        {
            // You might want to expand this list or make it configurable
            string[] known = ["Ninja", "Tfue", "Bugha", "SypherPK", "NickEh30"];
            var suggestions = known
                .Where(x => x.StartsWith(input, StringComparison.OrdinalIgnoreCase) && !x.Equals(input, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            return Task.FromResult(suggestions);
        }
    }
} 