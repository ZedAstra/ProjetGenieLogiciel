using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Models
{
    public sealed class TokenProvider(IConfiguration config)
    {
        public async Task<string> Create(Utilisateur user)
        {
            string secret = config["Jwt:Secret"];
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity([
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Name, user.Name),
                    new Claim(JwtRegisteredClaimNames.GivenName, user.Prenom),
                    new Claim(JwtRegisteredClaimNames.FamilyName, user.Nom),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim("role", user.RoleUtilisateur.ToString())
                ]),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = credentials,
                Issuer = config["Jwt:Issuer"],
                Audience = config["Jwt:Audience"],
            };

            var tokenHandler = new JsonWebTokenHandler();
            return tokenHandler.CreateToken(tokenDescriptor);
        }

        public async Task<Utilisateur?> GetUser(string token, AppDbContext context)
        {
            var tokenHandler = new JsonWebTokenHandler();
            var tokenValidationResult = await tokenHandler.ValidateTokenAsync(token.StartsWith("Bearer ") ? token.Replace("Bearer ", "") : token, Program.TokenValidationParameters);
            if (tokenValidationResult == null)
            {
                return null;
            }
            else if (tokenValidationResult.IsValid)
            {
                var id = tokenValidationResult.Claims.FirstOrDefault(claim => claim.Key == JwtRegisteredClaimNames.Sub).Value.ToString();
                if (id == null || !id.Any(char.IsDigit))
                {
                    return null;
                }
                return await context.Utilisateurs.FindAsync(int.Parse(id));
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> Validate(string token)
        {
            JsonWebTokenHandler tokenHandler = new JsonWebTokenHandler();
            TokenValidationResult? tokenValidationResult = await tokenHandler.ValidateTokenAsync(token.StartsWith("Bearer ") ? token.Replace("Bearer ", "") : token, Program.TokenValidationParameters);
            return tokenValidationResult.IsValid;
        }

        public async Task<bool> IsAuthorized(HttpRequest request, AppDbContext db, Func<Utilisateur, bool> condition)
        {
            var user = await GetUser(request.Headers.Authorization.FirstOrDefault(x => x != null && x.StartsWith("Bearer")) ?? "", db);
            if (user == null) return false;
            return condition(user);
        }
    }
}
