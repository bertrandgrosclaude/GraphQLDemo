namespace GraphQLDemo.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int PublicationYear { get; set; }

    // Clé étrangère + navigation vers l'auteur
    public int AuthorId { get; set; }
    public Author? Author { get; set; }
}
