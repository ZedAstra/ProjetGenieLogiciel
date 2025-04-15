using Backend.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Backend.Modules
{
    public class BaseModule : IModule
    {
        public void Setup(WebApplication app)
        {
            app.MapGet("app/projects", async (AppDbContext db) =>
            {
                return await db.Chantiers
                    .AsNoTracking()
                    .Select(ch => ch.CompactEntity())
                    .ToListAsync();
            })
                .RequireAuthorization()
                .WithName("GetAllChantiers")
                .WithTags("App")
                .WithDescription("Lister tous les chantiers")
                .Produces<List<Chantier.CompactChantier>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);

            app.MapPost("app/projects/create", async (AppDbContext db, [FromForm] CreateChantierForm form) =>
            {
                var chantier = new Chantier
                {
                    Nom = form.Nom,
                    Details = form.Details,
                    DateDebut = form.DateDebut,
                    Status = form.Status,
                    Membres = await db.Utilisateurs.Where(u => form.Membres.Contains(u.Id)).ToListAsync()
                };
                db.Chantiers.Add(chantier);
                await db.SaveChangesAsync();
                return Results.Created($"/app/projects/{chantier.Id}", chantier.CompactEntity());
            })
                .RequireAuthorization("Admin", "Chef")
                .WithName("CreateChantier")
                .WithTags("App")
                .WithDescription("Créer un nouveau chantier")
                .Produces<Chantier.CompactChantier>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);
        }

        public record CreateChantierForm(
            string Nom,
            string Details,
            DateOnly DateDebut,
            Chantier.StatusChantier Status,
            List<int> Membres
        );
    }
}
