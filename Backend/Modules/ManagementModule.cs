using Backend.Models;
using Backend.Models.Management;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Backend.Modules
{
    public class ManagementModule
    {
        public static void Setup(WebApplication app)
        {
            app.MapGet("/management/resources", async (HttpContext ctx, AppDbContext db, TokenProvider provider) =>
            {
                var result = db.Ressources.ToList();
                return Results.Ok(result);
            })
                .RequireAuthorization("Management")
                .WithTags("Management")
                .WithName("management.resources")
                .WithDisplayName("Ressources")
                .WithDescription("Lister les ressources disponnibles")
                .Produces<List<Ressource>>(200)
                .Produces(401);

            app.MapPost("/management/resources/add", async (HttpContext ctx, AppDbContext db, TokenProvider provider, [FromForm] string name, [FromForm] decimal quantity, [FromForm] string unit) =>
            {
                Ressource? res = await db.Ressources.FindAsync(name);
                // Already exists
                if (res != null) return Results.Conflict("Resource already exists. Maybe you meant to update it?");
                else
                {
                    var newRes = new Ressource()
                    {
                        Nom = name,
                        Quantité = quantity,
                        Unité = unit
                    };
                    db.Ressources.Add(newRes);
                    await db.SaveChangesAsync();
                    return Results.Ok(newRes);
                }
            })
                .RequireAuthorization("Management")
                .WithTags("Management")
                .WithName("management.new_resource")
                .WithDisplayName("Ajouter une ressource")
                .WithDescription("Ajouter une nouvelle ressource")
                .Produces<Ressource>(200)
                .Produces(409)
                .DisableAntiforgery();
            
            app.MapPost("/management/resources/update", async (HttpContext ctx, AppDbContext db, [FromBody] Ressource resource) =>
            {
                Ressource? existing = db.Ressources.Find(resource.Nom);
                if (existing == null) return Results.NotFound();
                if(resource.Quantité != -1) existing.Quantité = resource.Quantité;
                if (resource.Unité != "?") existing.Unité = resource.Unité;
                db.Update(existing);
                await db.SaveChangesAsync();
                return Results.Ok(existing);
            })
                .RequireAuthorization("Management")
                .WithTags("Management")
                .WithName("management.update_resource")
                .WithDisplayName("Modifier une ressource")
                .WithDescription("Modifier une ressource existante")
                .Produces<Ressource>(200)
                .Produces(404);
            
            app.MapDelete("/management/resources/{name}", async (HttpContext ctx, AppDbContext db, [FromRoute] string name) =>
            {
                Ressource? existing = db.Ressources.Find(name);
                if (existing == null) return Results.NotFound();
                db.Ressources.Remove(existing);
                await db.SaveChangesAsync();
                return Results.Ok();
            })
                .RequireAuthorization("Management")
                .WithTags("Management")
                .WithName("management.delete_resource")
                .WithDisplayName("Supprimer une ressource")
                .WithDescription("Supprimer une ressource existante")
                .Produces(200)
                .Produces(404);
        }
    }
}
