using Newtonsoft.Json;

namespace FortniteStatsAnalyzer.Models
{
    public class FortniteStatsResponse
    {
        [JsonProperty("result")]
        public bool Result { get; set; } // Indicates success or failure of the API response

        [JsonProperty("error")]
        public string? Error { get; set; } // Error message returned by the API in case of failure

        [JsonProperty("name")]
        public string? Name { get; set; } // Username

        [JsonProperty("account")]
        public Account? Account { get; set; }

        [JsonProperty("accountLevelHistory")]
        public List<AccountLevel>? AccountLevelHistory { get; set; }

        [JsonProperty("global_stats")]
        public GlobalStats? GlobalStats { get; set; }

        [JsonProperty("per_input")]
        public PerInput? PerInput { get; set; }

        [JsonProperty("seasons_available")]
        public List<int>? SeasonsAvailable { get; set; }
    }

    public class GlobalStats
    {
        [JsonProperty("solo")]
        public GameMode? Solo { get; set; }

        [JsonProperty("duo")]
        public GameMode? Duo { get; set; }

        [JsonProperty("squad")]
        public GameMode? Squad { get; set; }
    }

    public class GameMode
    {
        [JsonProperty("placetop1")]
        public int PlaceTop1 { get; set; }

        [JsonProperty("kd")]
        public double Kd { get; set; }

        [JsonProperty("winrate")]
        public double Winrate { get; set; }

        [JsonProperty("kills")]
        public int Kills { get; set; }

        [JsonProperty("matchesplayed")]
        public int MatchesPlayed { get; set; }

        // Add any additional fields, like "placetop3" or "placetop10", if needed
    }

    public class PerInput
    {
        [JsonProperty("gamepad")]
        public InputStats? Gamepad { get; set; }

        [JsonProperty("keyboardmouse")]
        public InputStats? KeyboardMouse { get; set; }

        [JsonProperty("touch")]
        public InputStats? Touch { get; set; }
    }

    public class InputStats
    {
        [JsonProperty("solo")]
        public GameMode? Solo { get; set; }

        [JsonProperty("duo")]
        public GameMode? Duo { get; set; }

        [JsonProperty("squad")]
        public GameMode? Squad { get; set; }
    }

    public class Account
    {
        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("progress_pct")]
        public int ProgressPct { get; set; }
    }

    public class AccountLevel
    {
        [JsonProperty("season")]
        public int Season { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("progress_pct")]
        public int ProgressPct { get; set; }
    }
}
