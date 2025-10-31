using Microsoft.EntityFrameworkCore;
using CMCS.WebApp.Models;

namespace CMCS.WebApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<SupportingDocument> SupportingDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure primary keys
            modelBuilder.Entity<User>().HasKey(u => u.UserId);
            modelBuilder.Entity<Claim>().HasKey(c => c.ClaimId);
            modelBuilder.Entity<SupportingDocument>().HasKey(sd => sd.DocumentId);

            // Configure relationships
            modelBuilder.Entity<Claim>()
                .HasOne(c => c.Lecturer)
                .WithMany(u => u.Claims)
                .HasForeignKey(c => c.LecturerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupportingDocument>()
                .HasOne(sd => sd.Claim)
                .WithMany(c => c.SupportingDocuments)
                .HasForeignKey(sd => sd.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed initial data
            modelBuilder.Entity<User>().HasData(
                new User { 
                    UserId = 1, 
                    Name = "John Lecturer", 
                    Email = "lecturer@cmcs.com", 
                    Password = "password", 
                    Role = "Lecturer" 
                },
                new User { 
                    UserId = 2, 
                    Name = "Sarah Coordinator", 
                    Email = "coordinator@cmcs.com", 
                    Password = "password", 
                    Role = "Coordinator" 
                },
                new User { 
                    UserId = 3, 
                    Name = "Mike Manager", 
                    Email = "manager@cmcs.com", 
                    Password = "password", 
                    Role = "Manager" 
                }
            );

            // Seed sample claims
            modelBuilder.Entity<Claim>().HasData(
                new Claim {
                    ClaimId = 1,
                    LecturerId = 1,
                    Period = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                    HoursWorked = 40,
                    Rate = 550,
                    Status = "Submitted",
                    SubmitDate = DateTime.Now.AddDays(-2)
                },
                new Claim {
                    ClaimId = 2,
                    LecturerId = 1,
                    Period = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, 1),
                    HoursWorked = 35,
                    Rate = 550,
                    Status = "Approved",
                    SubmitDate = DateTime.Now.AddDays(-30),
                    ProcessedDate = DateTime.Now.AddDays(-25),
                    ProcessedBy = "Sarah Coordinator"
                }
            );
        }
    }
}