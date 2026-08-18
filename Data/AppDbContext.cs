using Microsoft.EntityFrameworkCore;
using EbookCoinWallet.Api.Models;
namespace EbookCoinWallet.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Book>()
            .HasMany(b => b.Authors)
            .WithMany();
            
        modelBuilder.Entity<Book>()
            .HasMany(b => b.Categories)
            .WithMany();
    }
}
