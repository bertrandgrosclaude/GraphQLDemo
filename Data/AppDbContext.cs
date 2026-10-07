using GraphQLDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace GraphQLDemo.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Book> Books => Set<Book>();

    // Données de démarrage, pour pouvoir tester tout de suite sans mutation
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>().HasData(
            new Author { Id = 1, Name = "Antoine de Saint-Exupéry" },
            new Author { Id = 2, Name = "Albert Camus" }
        );

        modelBuilder.Entity<Book>().HasData(
            new Book { Id = 1, Title = "Le Petit Prince", PublicationYear = 1943, AuthorId = 1 },
            new Book { Id = 2, Title = "Vol de nuit", PublicationYear = 1931, AuthorId = 1 },
            new Book { Id = 3, Title = "L'Étranger", PublicationYear = 1942, AuthorId = 2 }
        );
    }
}
