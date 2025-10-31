using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace CMCS.WebApp.Models
{
    public class Claim
    {
        [Key]
        public int ClaimId { get; set; }

        [Required]
        [Display(Name = "Claim Period")]
        public DateTime Period { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        [Required]
        [Range(0.1, 1000)]
        [Display(Name = "Hours Worked")]
        public decimal HoursWorked { get; set; }

        [Required]
        [Range(1, 1000)]
        [Display(Name = "Hourly Rate")]
        public decimal Rate { get; set; }

        [Display(Name = "Total Amount")]
        public decimal TotalAmount => HoursWorked * Rate;

        public string Status { get; set; } = "Submitted";

        [Display(Name = "Submitted Date")]
        public DateTime SubmitDate { get; set; } = DateTime.Now;

        [Display(Name = "Processed Date")]
        public DateTime? ProcessedDate { get; set; }

        [Display(Name = "Processed By")]
        public string? ProcessedBy { get; set; }

        [Display(Name = "Lecturer")]
        public int LecturerId { get; set; }

        // File upload property (NOT stored in database)
        [NotMapped]
        [Display(Name = "Supporting Documents")]
        public IFormFile[]? SupportingFiles { get; set; }

        // Navigation properties
        [ForeignKey("LecturerId")]
        public virtual User? Lecturer { get; set; }

        public virtual ICollection<SupportingDocument> SupportingDocuments { get; set; } = new List<SupportingDocument>();

        public Claim()
        {
            SubmitDate = DateTime.Now;
            Status = "Submitted";
        }
    }
}