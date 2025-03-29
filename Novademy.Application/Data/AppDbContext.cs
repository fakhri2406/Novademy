using Microsoft.EntityFrameworkCore;
using Novademy.Application.Models;

namespace Novademy.Application.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Package> Packages { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=Novademy;Username=postgres;Password=Jdnnx68e");
    }
    
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
    }
}