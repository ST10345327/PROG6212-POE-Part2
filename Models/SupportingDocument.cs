using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMCS.WebApp.Models
{
    public class SupportingDocument
    {
        [Key]
        public int DocumentId { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "File Name")]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        [Display(Name = "Stored File Name")]
        public string StoredFileName { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Content Type")]
        public string ContentType { get; set; } = string.Empty;

        [Display(Name = "File Size (bytes)")]
        public long FileSize { get; set; }

        [Display(Name = "Upload Date")]
        [DataType(DataType.DateTime)]
        public DateTime UploadDate { get; set; } = DateTime.Now;

        [Display(Name = "Description")]
        [StringLength(200)]
        public string? Description { get; set; }

        public int ClaimId { get; set; }

        [ForeignKey("ClaimId")]
        public virtual Claim? Claim { get; set; }
    }
}