using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace GraphQLDemo.Auth;

public static class TokenService
{
    // Génère un JWT signé, contenant le nom d'utilisateur et son rôle.
    // C'est ce rôle que HotChocolate lira ensuite via [Authorize(Roles = ...)].
    public static string GenerateToken(string username, string role, string key, string issuer)
    {
        List<Claim> claims =
        [
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        ];

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1), // durée de vie courte : bonne pratique
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
