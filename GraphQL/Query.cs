using GraphQLDemo.Data;
using GraphQLDemo.Models;
using HotChocolate.Authorization;

namespace GraphQLDemo.GraphQL;

// [Authorize] au niveau de la classe : toutes les requêtes de ce type nécessitent
// un utilisateur authentifié (peu importe son rôle). Un appel sans jeton JWT valide
// reçoit une erreur GraphQL "AUTH_NOT_AUTHENTICATED".
[Authorize]
public class Query
{
    // [UseDbContext] injecte le DbContext EF Core dans le resolver
    // [UseProjection], [UseFiltering], [UseSorting] : HotChocolate traduit
    // automatiquement les arguments GraphQL (where, order, etc.) en requête SQL EF Core.
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Author> GetAuthors(AppDbContext context)
        => context.Authors;

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Book> GetBooks(AppDbContext context)
        => context.Books;

    // Exemple de requête avec un paramètre simple
    public async Task<Author?> GetAuthorById(int id, AppDbContext context)
        => await context.Authors.FindAsync(id);
}
