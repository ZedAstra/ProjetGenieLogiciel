using Backend.Models;
using Backend.Models.Planning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Backend.Modules
{
    public static class PlanningModule
    {
        public static void Setup(WebApplication app)
        {
            app.MapGet("/planning", async (HttpContext ctx, AppDbContext db, TokenProvider provider) =>
            {
                if(!await provider.IsAuthorized(ctx.Request, db, user => user.UserRole != User.Role.None)) return Results.Unauthorized();
                var now = DateTime.Now;
                return Results.Json(db.Taches.Where(p => p.Début >= now && p.Début.Year == now.Year && p.Début.Month == now.Month).Select(p => p.ToSmall()));
            })
                .RequireAuthorization("Authenticated")
                .WithTags("Planning")
                .WithName("planning.list")
                .WithDisplayName("Planning")
                .WithDescription("Lister les taches du mois")
                .Produces(200, typeof(Tache.Small[]))
                .Produces(401);

            app.MapPost("/planning/create", async (HttpContext ctx, AppDbContext db, TokenProvider provider, [FromBody] Tache.Small tache) =>
            {
                if (tache == null) return Results.BadRequest("Tache is null");
                if (tache.Début > tache.Fin) return Results.BadRequest("La date de début ne peut pas être supérieure à la date de fin.");
                var full = new Tache()
                {
                    Nom = tache.Nom,
                    Description = tache.Description,
                    Début = tache.Début,
                    Fin = tache.Fin,
                    Assignés = db.Users.Where(u => tache.Assignés.Contains(u.Id)).ToList(),
                    Etat = tache.Etat
                };
                db.Taches.Add(full);
                await db.SaveChangesAsync();
                return Results.Created($"/planning/{full.Id}", full.ToSmall());
            })
                .RequireAuthorization("Management")
                .WithTags("Planning")
                .WithName("planning.create")
                .WithDisplayName("Planning")
                .WithDescription("Créer une nouvelle tache")
                .Produces(201, typeof(Tache.Small))
                .Produces(401)
                .Produces(400);

            app.MapGet("/planning/{id}", async (HttpContext ctx, AppDbContext db, TokenProvider provider, Guid id) =>
            {
                if (!await provider.IsAuthorized(ctx.Request, db, user => user.UserRole != User.Role.None)) return Results.Unauthorized();
                var tache = db.Taches.Find(id);
                if (tache == null) return Results.NotFound();
                return Results.Json(tache.ToSmall());
            })
                .RequireAuthorization("Authenticated")
                .WithTags("Planning")
                .WithName("planning.get")
                .WithDisplayName("Planning")
                .WithDescription("Obtenir une tache spécifique")
                .Produces(200, typeof(Tache.Small))
                .Produces(401)
                .Produces(404);
            // Get tasks for the given month
            app.MapGet("/planning/of", async (HttpContext ctx, AppDbContext db, TokenProvider provider, [FromHeader] DateTime time) =>
            {
                if (!await provider.IsAuthorized(ctx.Request, db, user => user.UserRole != User.Role.None)) return Results.Unauthorized();
                return Results.Json(db.Taches.Where(p => p.Début.Year == time.Year && p.Début.Month == time.Month).Select(selector => selector.ToSmall()));
            })
                .RequireAuthorization("Authenticated")
                .WithTags("Planning")
                .WithName("planning.of")
                .WithDisplayName("Planning")
                .WithDescription("Lister les taches du mois")
                .Produces(200, typeof(Tache.Small[]))
                .Produces(401);
        }
    }
}
