using Microsoft.EntityFrameworkCore;
using System;

namespace Back_pixlpark.Models
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<VerificationCode> VerificationCodes { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Database.SetCommandTimeout(120); // Adjust timeout as necessary
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VerificationCode>()
                .ToTable("VerificationCodes"); // Ensure correct table name

            modelBuilder.Entity<VerificationCode>()
                .HasKey(vc => vc.Id); // Ensure correct primary key

            base.OnModelCreating(modelBuilder);
        }

    }
}
