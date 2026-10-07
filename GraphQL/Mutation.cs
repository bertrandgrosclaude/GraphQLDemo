using GraphQLDemo.Data;
using GraphQLDemo.Models;
using HotChocolate.Authorization;

namespace GraphQLDemo.GraphQL;

// Entrées (inputs) pour la mutation, bonne pratique GraphQL plutôt que
// de passer directement l'entité EF en paramètre.
public record AddBookInput(string Title, int PublicationYear, int AuthorId);

// Cette classe définit les "mutations" (écriture) exposées par l'API
public class Mutation
{
    // Seuls les utilisateurs avec le rôle "Admin" peuvent exécuter cette mutation.
    // Un utilisateur authentifié mais sans ce rôle reçoit une erreur
    // "AUTH_NOT_AUTHORIZED" ; un utilisateur non authentifié reçoit
    // "AUTH_NOT_AUTHENTICATED".
    [Authorize(Roles = ["Admin"])]
    public async Task<Book> AddBook(AddBookInput input, AppDbContext context)
    {
        var author = await context.Authors.FindAsync(input.AuthorId)
            ?? throw new GraphQLException($"Auteur {input.AuthorId} introuvable.");

        var book = new Book
        {
            Title = input.Title,
            PublicationYear = input.PublicationYear,
            AuthorId = author.Id
        };

        context.Books.Add(book);
        await context.SaveChangesAsync();

        return book;
    }
}
