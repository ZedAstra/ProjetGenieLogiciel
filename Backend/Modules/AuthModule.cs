using Microsoft.AspNetCore.Builder;
using Backend.Models;
using System;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Backend.Modules
{
    public class AuthModule
    {
        public static void Setup(WebApplication app)
        {
            app.MapPost("auth/login", (TokenProvider tokenProvider) =>
            {
                return tokenProvider.Create(new User()
                {
                    Id = Guid.NewGuid(),
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "email@email.com",
                    Type = User.UserType.Admin,
                });
            })
                .WithName("auth.login")
                .WithDisplayName("Login")
                .WithDescription("Login to the application")
                .Produces<string>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized);

            app.MapGet("auth/idk", (HttpContext ctx, AppDbContext db) =>
            {
                /*db.Add(new User()
                {
                    Id = Guid.NewGuid(),
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "",
                    Type = User.UserType.Admin,
                });*/
                return Results.Json(db.Users.ToList());
            });
        }
    }
}
