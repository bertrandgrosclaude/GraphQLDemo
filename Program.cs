using System.Text;
using GraphQLDemo.Auth;
using GraphQLDemo.Data;
using GraphQLDemo.GraphQL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuration d'Entity Framework Core avec SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=graphqldemo.db"));

// 2. Authentification : validation des jetons JWT envoyés dans l'en-tête
//    "Authorization: Bearer <token>"
//    ⚠️ La clé de signature ne doit JAMAIS être codée en dur en production :
//    utilisez `dotnet user-secrets` en dev, et une variable d'environnement /
//    un coffre-fort de secrets (Azure Key Vault, AWS Secrets Manager...) en prod.
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("La clé 'Jwt:Key' est manquante dans la configuration.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "GraphQLDemo";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtIssuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

// 3. Autorisation ASP.NET Core (nécessaire même si les règles vivent dans le schéma GraphQL)
builder.Services.AddAuthorization();

// 4. Configuration du serveur GraphQL (HotChocolate)
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddAuthorization()               // active [Authorize] / [AllowAnonymous] sur les resolvers
    .AddProjections()                 // ne sélectionne en SQL que les champs demandés
    .AddFiltering()                   // active l'argument "where"
    .AddSorting()                     // active l'argument "order"
    .AddMaxExecutionDepthRule(15);     // anti-DoS : rejette les requêtes trop imbriquées
  // .AllowIntrospection(builder.Environment.IsDevelopment()); // schéma non explorable en prod

var app = builder.Build();

// 5. Création automatique de la base + application des données de départ (HasData)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseAuthentication();
app.UseAuthorization();

// 6. Endpoint REST minimal pour obtenir un jeton JWT (login).
//    Dans une vraie application, on vérifierait un hash de mot de passe stocké
//    en base, pas une comparaison en clair sur une liste en mémoire.
app.MapPost("/login", (LoginRequest request) =>
{
    var user = DemoUsers.Users.FirstOrDefault(u =>
        u.Username == request.Username && u.Password == request.Password);

    if (user is null)
    {
        return Results.Unauthorized();
    }

    var token = TokenService.GenerateToken(user.Username, user.Role, jwtKey, jwtIssuer);
    return Results.Ok(new { token });
});

// 7. Exposition du endpoint GraphQL (avec Nitro, l'IDE web intégré)
app.MapGraphQL(); // disponible sur /graphql

app.Run();

record LoginRequest(string Username, string Password);
