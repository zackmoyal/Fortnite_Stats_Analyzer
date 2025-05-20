using Microsoft.AspNetCore.Mvc;
using FortniteStatsAnalyzer.Services;
using FortniteStatsAnalyzer.Models;
using System.Threading.Tasks;
using System.Diagnostics;

namespace FortniteStatsAnalyzer.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFortniteStatsService _fortniteStatsService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IFortniteStatsService fortniteStatsService, ILogger<HomeController> logger)
        {
            _fortniteStatsService = fortniteStatsService;
            _logger = logger;
        }

        // Index action to load the home page with the search form
        public IActionResult Index()
        {
            return View();
        }

        // AJAX endpoint to validate username
        [HttpGet]
        public async Task<IActionResult> ValidateUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("Validation failed: No username provided.");
                return Json(new { success = false, message = "Please enter a Fortnite username." });
            }

            var stats = await _fortniteStatsService.GetStatsForUser(username);

            if (stats == null)
            {
                _logger.LogWarning("Validation failed: No stats found for username '{Username}'", username);
                return Json(new { success = false, message = "Invalid username. Please try again." });
            }

            _logger.LogInformation("Validation success for username '{Username}'", username);
            return Json(new { success = true });
        }

        // Action to handle the form submission and display stats
        [HttpPost]
        public async Task<IActionResult> GetStats(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("No username provided by the user.");
                ViewBag.Error = "Please enter a Fortnite username.";
                return View("Index");
            }

            // Fetch stats for the given username using the service
            var stats = await _fortniteStatsService.GetStatsForUser(username);

            if (stats == null || !stats.Result)
            {
                _logger.LogWarning("No stats found for the user: {Username}", username);
                ViewBag.Error = "No player stats available. Please check the username and try again.";
                return View("Index");
            }

            // Log success and pass the stats to the view
            _logger.LogInformation("Successfully retrieved stats for user: {Username}", username);
            return View("StatsView", stats);
        }

        // Privacy action (default in ASP.NET Core projects)
        public IActionResult Privacy()
        {
            return View();
        }

        // Default error handler for exceptions
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
