using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CourtCaseTrackingSystem.Models;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CourtCaseTrackingSystem.Data
{
    public class CourtDbContext : IdentityDbContext<ApplicationUser>
    {
        public CourtDbContext(DbContextOptions<CourtDbContext> options)
            : base(options)
        {
        }

        public DbSet<Case> Cases { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Case entity
            modelBuilder.Entity<Case>(entity =>
            {
                // Configure relationship with judge
                entity.HasOne(c => c.AssignedJudge)
                    .WithMany()
                    .HasForeignKey(c => c.AssignedJudgeId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Configure CaseID
                entity.Property(c => c.CaseID)
                    .ValueGeneratedOnAdd()
                    .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
            });
        }
    }
}