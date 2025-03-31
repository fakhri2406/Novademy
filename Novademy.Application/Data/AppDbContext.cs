using Microsoft.EntityFrameworkCore;
using Novademy.Application.Models;

namespace Novademy.Application.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public AppDbContext() { }
    
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Package> Packages { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<UserQuizAttempt> UserQuizAttempts { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Course>()
            .HasMany(c => c.Packages)
            .WithMany(p => p.Courses)
            .UsingEntity(j => j.ToTable("CoursePackages"));
        
        modelBuilder.Entity<Subscription>()
            .HasOne(s => s.User)
            .WithMany(u => u.Subscriptions)
            .HasForeignKey(s => s.UserId);
        
        modelBuilder.Entity<Subscription>()
            .HasOne(s => s.Package)
            .WithMany(p => p.Subscriptions)
            .HasForeignKey(s => s.PackageId);
        
        modelBuilder.Entity<Quiz>()
            .HasOne(q => q.Lesson)
            .WithMany(l => l.Quizzes)
            .HasForeignKey(q => q.LessonId);
        
        modelBuilder.Entity<Question>()
            .HasOne(q => q.Quiz)
            .WithMany(q => q.Questions)
            .HasForeignKey(q => q.QuizId);
        
        modelBuilder.Entity<Answer>()
            .HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .HasForeignKey(a => a.QuestionId);
        
        modelBuilder.Entity<UserQuizAttempt>()
            .HasOne(uqa => uqa.User)
            .WithMany(u => u.UserQuizAttempts)
            .HasForeignKey(uqa => uqa.UserId);
        
        modelBuilder.Entity<UserQuizAttempt>()
            .HasOne(uqa => uqa.Quiz)
            .WithMany()
            .HasForeignKey(uqa => uqa.QuizId);
    }
}