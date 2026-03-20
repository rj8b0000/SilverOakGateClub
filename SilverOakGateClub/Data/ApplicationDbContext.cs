using Microsoft.EntityFrameworkCore;
using SilverOakGateClub.Models;

namespace SilverOakGateClub.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<MockTest> MockTests => Set<MockTest>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<TestResult> TestResults => Set<TestResult>();
    public DbSet<Lecture> Lectures => Set<Lecture>();
    public DbSet<Notes> Notes => Set<Notes>();
    public DbSet<Announcement> Announcements => Set<Announcement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasOne(u => u.Branch)
                  .WithMany(b => b.Users)
                  .HasForeignKey(u => u.BranchId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // MockTest
        modelBuilder.Entity<MockTest>(entity =>
        {
            entity.HasOne(m => m.Branch)
                  .WithMany(b => b.MockTests)
                  .HasForeignKey(m => m.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Question
        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasOne(q => q.MockTest)
                  .WithMany(m => m.Questions)
                  .HasForeignKey(q => q.MockTestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // TestResult
        modelBuilder.Entity<TestResult>(entity =>
        {
            entity.HasOne(tr => tr.User)
                  .WithMany(u => u.TestResults)
                  .HasForeignKey(tr => tr.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(tr => tr.MockTest)
                  .WithMany(m => m.TestResults)
                  .HasForeignKey(tr => tr.MockTestId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Lecture
        modelBuilder.Entity<Lecture>(entity =>
        {
            entity.HasOne(l => l.Branch)
                  .WithMany(b => b.Lectures)
                  .HasForeignKey(l => l.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Notes
        modelBuilder.Entity<Notes>(entity =>
        {
            entity.HasOne(n => n.Branch)
                  .WithMany(b => b.Notes)
                  .HasForeignKey(n => n.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(n => n.UploadedBy)
                  .WithMany()
                  .HasForeignKey(n => n.UploadedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Announcement
        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.HasOne(a => a.Branch)
                  .WithMany()
                  .HasForeignKey(a => a.BranchId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(a => a.CreatedBy)
                  .WithMany()
                  .HasForeignKey(a => a.CreatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed Branches
        modelBuilder.Entity<Branch>().HasData(
            new Branch { Id = 1, Name = "Computer Science & IT", Code = "CS" },
            new Branch { Id = 2, Name = "Electronics & Communication", Code = "EC" },
            new Branch { Id = 3, Name = "Electrical Engineering", Code = "EE" },
            new Branch { Id = 4, Name = "Mechanical Engineering", Code = "ME" },
            new Branch { Id = 5, Name = "Civil Engineering", Code = "CE" },
            new Branch { Id = 6, Name = "Instrumentation Engineering", Code = "IN" }
        );
    }
}
