# GraphQLDemo — API GraphQL en C# avec Entity Framework Core

Exemple minimal illustrant :
- Un serveur GraphQL avec **HotChocolate**
- Une persistance avec **Entity Framework Core** (SQLite)
- Une relation 1-N entre `Author` et `Book`
- Requêtes (`Query`) avec filtrage/tri/projection automatiques
- Une mutation (`Mutation`) pour ajouter un livre

## Prérequis

- .NET 9 SDK installé (https://dotnet.microsoft.com/download)

## Lancer le projet

```bash
cd GraphQLDemo
dotnet restore
dotnet run
```

La base SQLite `graphqldemo.db` est créée automatiquement au démarrage, avec
quelques données de test (2 auteurs, 3 livres).

Le endpoint GraphQL est exposé sur :

```
https://localhost:<port>/graphql
```

En ouvrant cette URL dans un navigateur, HotChocolate affiche **Nitro**
(l'IDE web intégré, anciennement nommé Banana Cake Pop), qui permet de tester
des requêtes/mutations directement.

## Exemples de requêtes à tester

### Lister tous les auteurs avec leurs livres

```graphql
query {
  authors {
    id
    name
    books {
      title
      publicationYear
    }
  }
}
```

### Filtrer les livres publiés après 1940, triés par année

```graphql
query {
  books(where: { publicationYear: { gt: 1940 } }, order: { publicationYear: ASC }) {
    title
    publicationYear
    author {
      name
    }
  }
}
```

### Ajouter un nouveau livre (mutation)

```graphql
mutation {
  addBook(input: { title: "La Peste", publicationYear: 1947, authorId: 2 }) {
    id
    title
  }
}
```

## Sécurité : authentification et autorisation

L'API est protégée par JWT :
- **Toutes les requêtes** (`Query`) nécessitent d'être authentifié.
- **La mutation `addBook`** nécessite en plus le rôle `Admin`.

### 1. Se connecter pour obtenir un jeton

```bash
curl -X POST https://localhost:<port>/login \
  -H "Content-Type: application/json" \
  -d '{"username":"bob","password":"admin123"}'
```

Utilisateurs de démonstration (voir `Auth/DemoUsers.cs`) :

| Utilisateur | Mot de passe | Rôle  |
|-------------|---------------|-------|
| alice       | user123       | User  |
| bob         | admin123      | Admin |

La réponse contient `{ "token": "eyJ..." }`.

### 2. Appeler l'API GraphQL avec le jeton

Dans Nitro (l'IDE intégré sur `/graphql`), ajoutez un en-tête HTTP :

```
Authorization: Bearer <token>
```

- Avec le jeton d'`alice` (rôle `User`) : les requêtes passent, la mutation `addBook` échoue avec `AUTH_NOT_AUTHORIZED`.
- Avec le jeton de `bob` (rôle `Admin`) : tout fonctionne.
- Sans jeton : toute requête échoue avec `AUTH_NOT_AUTHENTICATED`.

## Autres bonnes pratiques de sécurité pour une API GraphQL

Ce projet illustre l'authentification/autorisation, plus deux garde-fous propres à GraphQL déjà activés dans `Program.cs` :
- **`AddMaxExecutionDepthRule(15)`** : rejette les requêtes trop imbriquées (une requête GraphQL peut, contrairement à un endpoint REST, être arbitrairement coûteuse en s'enchaînant sur des relations).
- **`AllowIntrospection(...)`** : désactive l'exploration du schéma (`__schema`, `__type`) hors environnement de développement.

À considérer en plus pour une mise en production :

- **HTTPS obligatoire** partout (jamais de jeton JWT en clair sur HTTP).
- **Cost analysis** (`.AddCostAnalyzer()` côté HotChocolate) pour limiter le *nombre de nœuds* qu'une requête peut réclamer, pas seulement sa profondeur — utile contre les requêtes qui multiplient les listes paginées entre elles.
- **Rate limiting** (middleware ASP.NET Core `Microsoft.AspNetCore.RateLimiting`) pour limiter le nombre de requêtes par IP/utilisateur.
- **CORS restrictif** : n'autoriser que les origines (domaines front-end) réellement nécessaires.
- **Masquage des erreurs** : ne jamais renvoyer de stack trace ou de détails d'exception en production (HotChocolate masque déjà les exceptions non gérées par défaut hors dev, à vérifier/personnaliser via un `IErrorFilter`).
- **DataLoader** pour éviter les problèmes de performance N+1 sur les relations (ex. charger les auteurs de plusieurs livres en une seule requête SQL).
- **Refresh tokens** + expiration courte des access tokens (ici fixée à 1h) pour limiter l'impact d'un jeton volé.
- **Remplacer les utilisateurs en mémoire** par ASP.NET Core Identity, une vraie table avec mots de passe hashés, ou un fournisseur d'identité externe (Entra ID, Auth0, Keycloak...).
- **Requêtes persistées / trusted documents** (`.AddTrustedDocuments()`) pour n'autoriser en production que des requêtes GraphQL pré-approuvées plutôt que du texte libre arbitraire.



## Structure du projet

```
GraphQLDemo/
├── GraphQLDemo.csproj
├── Program.cs                    # configuration EF Core + JWT + HotChocolate
├── appsettings.Development.json  # clé JWT de démo (ne jamais committer en vrai)
├── Auth/
│   ├── DemoUsers.cs               # utilisateurs de démo (à remplacer en prod)
│   └── TokenService.cs            # génération des jetons JWT
├── Models/
│   ├── Author.cs
│   └── Book.cs
├── Data/
│   └── AppDbContext.cs            # DbContext EF Core + données de test
└── GraphQL/
    ├── Query.cs                    # requêtes en lecture — [Authorize]
    └── Mutation.cs                 # mutations en écriture — [Authorize(Roles = ["Admin"])]
```

## Points clés à retenir

- **HotChocolate** génère automatiquement le schéma GraphQL à partir des classes
  `Query`/`Mutation` en C# (approche "code-first").
- `IQueryable<T>` renvoyé par un resolver + `[UseFiltering]`/`[UseSorting]`/`[UseProjection]`
  permet à HotChocolate de traduire les arguments GraphQL (`where`, `order`) directement
  en requête SQL via EF Core, sans charger toutes les données en mémoire.
- Le DbContext est injecté directement en paramètre du resolver (`AppDbContext context`) :
  HotChocolate gère l'injection de dépendances comme ASP.NET Core.
