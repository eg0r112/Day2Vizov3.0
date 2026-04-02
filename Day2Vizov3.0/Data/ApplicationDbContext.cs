using Microsoft.EntityFrameworkCore;
using Day2Vizov3._0.Models;

namespace Day2Vizov3._0.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<AppUser> Users { get; set; }
    public DbSet<Document> Documents { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Username)
            .IsUnique();
        
        modelBuilder.Entity<Document>()
            .HasIndex(d => d.OwnerId);
        
        modelBuilder.Entity<Document>()
            .HasIndex(d => new { d.IsDeleted, d.IsPublic });

        modelBuilder.Entity<AppUser>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        modelBuilder.Entity<Document>()
            .Property(d => d.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}