using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMCS.WebApp.Data;
using CMCS.WebApp.Models;

namespace CMCS.WebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            // If already logged in, redirect to home
            if (IsLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
            
            if (user != null)
            {
                // Store user info in session
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("UserRole", user.Role);
                
                TempData["SuccessMessage"] = $"Welcome back, {user.Name}!";
                return RedirectToAction("Index", "Home");
            }
            
            TempData["ErrorMessage"] = "Invalid email or password";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out successfully";
            return RedirectToAction("Login");
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