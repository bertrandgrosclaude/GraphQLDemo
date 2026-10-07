namespace GraphQLDemo.Auth;

// ⚠️ DÉMO UNIQUEMENT.
// Ici les utilisateurs et mots de passe sont codés en dur, en clair, en mémoire.
// En production, remplacez ceci par :
//   - une vraie base d'utilisateurs (table EF Core dédiée)
//   - des mots de passe hashés avec un algorithme adapté (BCrypt, Argon2, ou
//     PasswordHasher d'ASP.NET Core Identity) — jamais en clair
//   - idéalement, ASP.NET Core Identity ou un fournisseur d'identité externe
//     (Entra ID, Auth0, Keycloak...) plutôt que de gérer soi-même les mots de passe
public record DemoUser(string Username, string Password, string Role);

public static class DemoUsers
{
    public static readonly List<DemoUser> Users =
    [
        new DemoUser("alice", "user123", "User"),
        new DemoUser("bob", "admin123", "Admin")
    ];
}
