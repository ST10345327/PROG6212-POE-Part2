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
            // Check if user is logged in
            if (!IsLoggedIn())
            {
                // Show public landing page without sensitive data
                ViewBag.IsLoggedIn = false;
                return View();
            }

            // User is logged in - show personalized dashboard
            try
            {
                var currentUser = GetCurrentUser();
                
                // Get database statistics for the dashboard
                var userCount = await _context.Users.CountAsync();
                var claimCount = await _context.Claims.CountAsync();
                var pendingClaims = await _context.Claims.CountAsync(c => c.Status == "Submitted");

                // Pass data to the view
                ViewBag.UserCount = userCount;
                ViewBag.ClaimCount = claimCount;
                ViewBag.PendingClaims = pendingClaims;
                ViewBag.DatabaseStatus = "Connected successfully!";
                ViewBag.CurrentUser = currentUser;
                ViewBag.IsLoggedIn = true;

                // FIXED: Added null check for currentUser before accessing UserId
                if (currentUser != null && IsInRole("Lecturer"))
                {
                    var myClaims = await _context.Claims
                        .Where(c => c.LecturerId == currentUser.UserId)
                        .ToListAsync();
                    ViewBag.MyClaims = myClaims;
                }
                else if (IsInRole("Coordinator") || IsInRole("Manager"))
                {
                    var recentClaims = await _context.Claims
                        .Include(c => c.Lecturer)
                        .OrderByDescending(c => c.SubmitDate)
                        .Take(5)
                        .ToListAsync();
                    ViewBag.RecentClaims = recentClaims;
                }
            }
            catch (Exception ex)
            {
                // If database connection fails, show error
                ViewBag.DatabaseStatus = $"Error: {ex.Message}";
                ViewBag.UserCount = 0;
                ViewBag.ClaimCount = 0;
                ViewBag.PendingClaims = 0;
                ViewBag.IsLoggedIn = IsLoggedIn();
            }

            return View();
        }

        public IActionResult Analytics()
        {
            // Simple authorization check
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
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
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Helper methods
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserId").HasValue;
        }

        private bool IsInRole(string role)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            return userRole == role;
        }

        private User? GetCurrentUser()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                return _context.Users.FirstOrDefault(u => u.UserId == userId.Value);
            }
            return null;
        }
    }
}