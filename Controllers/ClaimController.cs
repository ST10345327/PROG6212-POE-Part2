using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMCS.WebApp.Models;
using CMCS.WebApp.Data;

namespace CMCS.WebApp.Controllers
{
    public class ClaimController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClaimController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Display claim submission form
        public IActionResult Create()
        {
            return View();
        }

        // POST: Handle claim submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Claim claim)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Set basic claim properties
                    claim.LecturerId = 1; // Default lecturer for now
                    claim.Status = "Submitted";
                    claim.SubmitDate = DateTime.Now;

                    // Save to database
                    _context.Claims.Add(claim);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Claim submitted successfully!";
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }
            return View(claim);
        }

        // GET: Display claims for approval
        public async Task<IActionResult> Approvals()
        {
            var pendingClaims = await _context.Claims
                .Include(c => c.Lecturer)
                .Where(c => c.Status == "Submitted")
                .OrderBy(c => c.SubmitDate)
                .ToListAsync();

            return View(pendingClaims);
        }

        // POST: Approve a claim
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim != null)
            {
                claim.Status = "Approved";
                claim.ProcessedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Claim approved!";
            }
            return RedirectToAction("Approvals");
        }

        // POST: Reject a claim
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim != null)
            {
                claim.Status = "Rejected";
                claim.ProcessedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Claim rejected!";
            }
            return RedirectToAction("Approvals");
        }

        // GET: User's claim history
        public async Task<IActionResult> Index()
        {
            var claims = await _context.Claims
                .Where(c => c.LecturerId == 1)
                .OrderByDescending(c => c.SubmitDate)
                .ToListAsync();

            return View(claims);
        }
    }
}