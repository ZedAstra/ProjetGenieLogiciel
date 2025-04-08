using Microsoft.AspNetCore.Builder;
using Backend.Models;
using System;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Modules
{
    public class AuthModule
    {
        public static void Setup(WebApplication app)
        {
            app.MapPost("auth/login", async (TokenProvider tokenProvider, AppDbContext db, [FromForm] string login, [FromForm] string password) =>
            {
                var user = db.Users.Where(user => (user.Email == login || user.FirstName + " " + user.LastName == login) && user.Password == password).FirstOrDefault();
                if (user == null)
                {
                    return Results.Unauthorized();
                }
                else return Results.Text(await tokenProvider.Create(user!));
            })
                .WithTags("Auth")
                .WithName("auth.login")
                .WithDisplayName("Connexion")
                .WithDescription("Connexion à application")
                .Produces<string>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .DisableAntiforgery();

            app.MapPost("auth/set_password", async (TokenProvider tokenProvider, AppDbContext db, [FromForm] Guid userId, [FromForm] string newPassword) =>
            {
                var user = db.Users.Find(userId);
                if (user == null)
                {
                    return Results.NotFound();
                }
                else
                {
                    user.Password = newPassword;
                    await db.SaveChangesAsync();
                    return Results.Ok();
                }
            })
                .RequireAuthorization("Admin")
                .WithTags("Auth")
                .WithName("auth.change_password")
                .WithDisplayName("Changer le mot de passe")
                .WithDescription("Changer le mot de passe de l'utilisateur")
                .Produces(StatusCodes.Status200OK)
                .DisableAntiforgery();
        }
    }
}
