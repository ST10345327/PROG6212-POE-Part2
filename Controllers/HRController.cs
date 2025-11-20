using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMCS.WebApp.Data;
using CMCS.WebApp.Models;
using ClosedXML.Excel;
using System.Linq;

namespace CMCS.WebApp.Controllers
{
    public class HRController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HRController> _logger;

        public HRController(ApplicationDbContext context, ILogger<HRController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: HR Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAuthorized())
            {
                TempData["ErrorMessage"] = "Access denied. HR or Manager role required.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // Get dashboard statistics
                var stats = await GetDashboardStatistics();
                ViewBag.DashboardStats = stats;

                // Get chart data
                ViewBag.MonthlyTrendData = await GetMonthlyTrendData();
                ViewBag.DepartmentData = await GetDepartmentData();
                ViewBag.LecturerPerformanceData = await GetLecturerPerformanceData();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading HR dashboard");
                TempData["ErrorMessage"] = "Error loading dashboard data: " + ex.Message;
                return View();
            }
        }

        // GET: Generate Report
        public async Task<IActionResult> GenerateReport(string reportType = "monthly")
        {
            if (!IsAuthorized())
            {
                TempData["ErrorMessage"] = "Access denied. HR or Manager role required.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var reportData = await GenerateReportData(reportType);
                ViewBag.ReportData = reportData;
                ViewBag.ReportType = reportType;
                ViewBag.GeneratedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

                TempData["SuccessMessage"] = $"{reportType.ToUpper()} report generated successfully!";
                return View("Report");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                TempData["ErrorMessage"] = "Error generating report: " + ex.Message;
                return RedirectToAction("Dashboard");
            }
        }

        // POST: Export to Excel
        public async Task<IActionResult> ExportToExcel(string reportType = "monthly")
        {
            if (!IsAuthorized())
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var reportData = await GenerateReportData(reportType);
                var fileName = $"CMCS_Report_{reportType}_{DateTime.Now:yyyyMMdd}.xlsx";

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Report");

                    // Add title
                    worksheet.Cell(1, 1).Value = $"CMCS {reportType.ToUpper()} Report";
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                    worksheet.Range(1, 1, 1, 5).Merge();

                    // Add headers
                    worksheet.Cell(3, 1).Value = "Period";
                    worksheet.Cell(3, 2).Value = "Lecturer";
                    worksheet.Cell(3, 3).Value = "Department";
                    worksheet.Cell(3, 4).Value = "Hours";
                    worksheet.Cell(3, 5).Value = "Amount";
                    worksheet.Cell(3, 6).Value = "Status";

                    var headerRange = worksheet.Range(3, 1, 3, 6);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

                    // Add data
                    int row = 4;
                    foreach (var claim in reportData.Claims)
                    {
                        worksheet.Cell(row, 1).Value = claim.Period.ToString("MMMM yyyy");
                        worksheet.Cell(row, 2).Value = claim.LecturerName;
                        worksheet.Cell(row, 3).Value = claim.Department;
                        worksheet.Cell(row, 4).Value = (double)claim.HoursWorked;
                        worksheet.Cell(row, 5).Value = (double)claim.TotalAmount;
                        worksheet.Cell(row, 6).Value = claim.Status;
                        row++;
                    }

                    // Auto-fit columns
                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting to Excel");
                TempData["ErrorMessage"] = "Error exporting report to Excel: " + ex.Message;
                return RedirectToAction("Dashboard");
            }
        }

        // POST: Process Payments (Simulation)
        [HttpPost]
        public async Task<IActionResult> ProcessPayments()
        {
            if (!IsAuthorized())
            {
                TempData["ErrorMessage"] = "Access denied. HR or Manager role required.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // Simulate payment processing
                var approvedClaims = await _context.Claims
                    .Where(c => c.Status == "Approved" && c.ProcessedDate == null)
                    .ToListAsync();

                foreach (var claim in approvedClaims)
                {
                    claim.ProcessedDate = DateTime.Now;
                    claim.ProcessedBy = GetCurrentUser()?.Name ?? "HR System";
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Successfully processed payments for {approvedClaims.Count} claims!";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payments");
                TempData["ErrorMessage"] = "Error processing payments: " + ex.Message;
                return RedirectToAction("Dashboard");
            }
        }

        // Private helper methods
        private bool IsAuthorized()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            return userRole == "Manager" || userRole == "HR";
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

        private async Task<DashboardStatistics> GetDashboardStatistics()
        {
            try
            {
                var totalLecturers = await _context.Users.CountAsync(u => u.Role == "Lecturer");
                var activeClaims = await _context.Claims.CountAsync(c => c.Status == "Submitted");
                
                // FIXED FOR SQLITE: Convert to double for aggregation, then back to decimal
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                var monthlyPayout = (await _context.Claims
                    .Where(c => c.Status == "Approved" && c.Period.Month == currentMonth && c.Period.Year == currentYear)
                    .Select(c => new { Amount = (double)(c.HoursWorked * c.Rate) })
                    .ToListAsync())
                    .Sum(x => x.Amount);

                var totalClaims = await _context.Claims.CountAsync();
                var approvedClaims = await _context.Claims.CountAsync(c => c.Status == "Approved");
                var approvalRate = totalClaims > 0 ? (approvedClaims * 100.0 / totalClaims) : 0;

                return new DashboardStatistics
                {
                    TotalLecturers = totalLecturers,
                    ActiveClaims = activeClaims,
                    MonthlyPayout = (decimal)monthlyPayout,
                    ApprovalRate = Math.Round(approvalRate, 1)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard statistics");
                // Return default values if there's an error
                return new DashboardStatistics
                {
                    TotalLecturers = 0,
                    ActiveClaims = 0,
                    MonthlyPayout = 0,
                    ApprovalRate = 0
                };
            }
        }

        private async Task<MonthlyTrendData> GetMonthlyTrendData()
        {
            try
            {
                var sixMonthsAgo = DateTime.Now.AddMonths(-5);
                
                // FIXED FOR SQLITE: Do grouping and aggregation in memory
                var claims = await _context.Claims
                    .Where(c => c.Status == "Approved" && c.Period >= sixMonthsAgo)
                    .Select(c => new { c.Period, Amount = (double)(c.HoursWorked * c.Rate) })
                    .ToListAsync();

                var trends = claims
                    .GroupBy(c => new { c.Period.Year, c.Period.Month })
                    .Select(g => new
                    {
                        Period = new DateTime(g.Key.Year, g.Key.Month, 1),
                        TotalAmount = g.Sum(c => c.Amount)
                    })
                    .OrderBy(x => x.Period)
                    .ToList();

                return new MonthlyTrendData
                {
                    Labels = trends.Select(t => t.Period.ToString("MMM yyyy")).ToArray(),
                    Data = trends.Select(t => t.TotalAmount).ToArray()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly trend data");
                // Return empty data if there's an error
                return new MonthlyTrendData
                {
                    Labels = new string[0],
                    Data = new double[0]
                };
            }
        }

        private async Task<DepartmentData> GetDepartmentData()
        {
            // This uses simulated data, so it should always work
            return await Task.FromResult(new DepartmentData
            {
                Labels = new[] { "IT", "Business", "Engineering", "Health", "Arts" },
                Data = new[] { 35, 25, 20, 15, 5 }
            });
        }

        private async Task<LecturerPerformanceData> GetLecturerPerformanceData()
        {
            try
            {
                // FIXED FOR SQLITE: Do aggregation in memory
                var lecturers = await _context.Users
                    .Where(u => u.Role == "Lecturer")
                    .Include(u => u.Claims)
                    .ToListAsync();

                var performance = lecturers
                    .Select(u => new
                    {
                        u.Name,
                        TotalAmount = u.Claims
                            .Where(c => c.Status == "Approved")
                            .Sum(c => (double)(c.HoursWorked * c.Rate))
                    })
                    .Where(x => x.TotalAmount > 0)
                    .OrderByDescending(x => x.TotalAmount)
                    .Take(8)
                    .ToList();

                return new LecturerPerformanceData
                {
                    Labels = performance.Select(p => p.Name).ToArray(),
                    Data = performance.Select(p => p.TotalAmount).ToArray()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lecturer performance data");
                // Return empty data if there's an error
                return new LecturerPerformanceData
                {
                    Labels = new string[0],
                    Data = new double[0]
                };
            }
        }

        private async Task<ReportData> GenerateReportData(string reportType)
        {
            try
            {
                IQueryable<Claim> query = _context.Claims.Include(c => c.Lecturer);

                if (reportType == "monthly")
                {
                    var currentMonth = DateTime.Now.Month;
                    var currentYear = DateTime.Now.Year;
                    query = query.Where(c => c.Period.Month == currentMonth && c.Period.Year == currentYear);
                }
                else if (reportType == "pending")
                {
                    query = query.Where(c => c.Status == "Submitted");
                }

                var claims = await query
                    .OrderByDescending(c => c.SubmitDate)
                    .Select(c => new ReportClaim
                    {
                        Period = c.Period,
                        LecturerName = c.Lecturer != null ? c.Lecturer.Name : "Unknown",
                        Department = "IT",
                        HoursWorked = c.HoursWorked,
                        TotalAmount = c.HoursWorked * c.Rate,
                        Status = c.Status ?? "Unknown"
                    })
                    .ToListAsync();

                // FIXED FOR SQLITE: Do summation in memory
                var totalAmount = claims.Where(c => c.Status == "Approved").Sum(c => (double)c.TotalAmount);

                return new ReportData
                {
                    ReportType = reportType,
                    GeneratedDate = DateTime.Now,
                    TotalAmount = (decimal)totalAmount,
                    TotalClaims = claims.Count,
                    Claims = claims
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report data");
                // Return empty report if there's an error
                return new ReportData
                {
                    ReportType = reportType,
                    GeneratedDate = DateTime.Now,
                    TotalAmount = 0,
                    TotalClaims = 0,
                    Claims = new List<ReportClaim>()
                };
            }
        }
    }

    // Data transfer objects
    public class DashboardStatistics
    {
        public int TotalLecturers { get; set; }
        public int ActiveClaims { get; set; }
        public decimal MonthlyPayout { get; set; }
        public double ApprovalRate { get; set; }
    }

    public class MonthlyTrendData
    {
        public string[] Labels { get; set; } = Array.Empty<string>();
        public double[] Data { get; set; } = Array.Empty<double>();
    }

    public class DepartmentData
    {
        public string[] Labels { get; set; } = Array.Empty<string>();
        public int[] Data { get; set; } = Array.Empty<int>();
    }

    public class LecturerPerformanceData
    {
        public string[] Labels { get; set; } = Array.Empty<string>();
        public double[] Data { get; set; } = Array.Empty<double>();
    }

    public class ReportData
    {
        public string ReportType { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalClaims { get; set; }
        public List<ReportClaim> Claims { get; set; } = new List<ReportClaim>();
    }

    public class ReportClaim
    {
        public DateTime Period { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public decimal HoursWorked { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}