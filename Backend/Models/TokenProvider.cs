using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Backend.Models
{
    public sealed class TokenProvider(IConfiguration config)
    {
        public async Task<string> Create(User user)
        {
            string secret = config["Jwt:Secret"];
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity([
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Name, user.Name),
                    new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                    new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim("role", user.UserRole.ToString())
                ]),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = credentials,
                Issuer = config["Jwt:Issuer"],
                Audience = config["Jwt:Audience"],
            };

            var tokenHandler = new JsonWebTokenHandler();
            return tokenHandler.CreateToken(tokenDescriptor);
        }

        public async Task<User>? GetUser(string token, AppDbContext context)
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
                if (id == null)
                {
                    return null;
                }
                return context.Users.Find(Guid.Parse(id));
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> Validate(string token)
        {
            var tokenHandler = new JsonWebTokenHandler();
            var tokenValidationResult = await tokenHandler.ValidateTokenAsync(token.StartsWith("Bearer ") ? token.Replace("Bearer ", "") : token, Program.TokenValidationParameters);
            return tokenValidationResult.IsValid;
        }

        public async Task<bool> IsAuthorized(HttpRequest request, AppDbContext db, Func<User, bool> condition)
        {
            var user = await GetUser(request.Headers["Authorization"], db);
            if (user == null) return false;
            return condition(user);
        }
    }
}
