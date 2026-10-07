namespace GraphQLDemo.Models;

public class Author
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Relation 1-N : un auteur a plusieurs livres
    public List<Book> Books { get; set; } = new();
}
