using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMCS.WebApp.Data;
using CMCS.WebApp.Models;

namespace CMCS.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Get database statistics for the dashboard
                var userCount = await _context.Users.CountAsync();
                var claimCount = await _context.Claims.CountAsync();
                var pendingClaims = await _context.Claims.CountAsync(c => c.Status == "Submitted");

                // Pass data to the view
                ViewBag.UserCount = userCount;
                ViewBag.ClaimCount = claimCount;
                ViewBag.PendingClaims = pendingClaims;
                ViewBag.DatabaseStatus = "Connected successfully!";
            }
            catch (Exception ex)
            {
                // If database connection fails, show error
                ViewBag.DatabaseStatus = $"Error: {ex.Message}";
                ViewBag.UserCount = 0;
                ViewBag.ClaimCount = 0;
                ViewBag.PendingClaims = 0;
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}