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
                    .Include(c => c.Membres)
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

            app.MapGet("app/projects/{id}", async (AppDbContext db, int id) =>
            {
                var chantier = await db.Chantiers.FindAsync(id);
                if(chantier == null) return Results.NotFound($"Chantier with id {id} not found");
                var membres = await db.Utilisateurs
                    .Include(u => u.Chantiers)
                    .Where(u => u.Chantiers.Any(c => c.Id == chantier.Id))
                    .Select(u => u.CompactEntity())
                    .ToListAsync();
                return Results.Ok(membres);
            })
                .RequireAuthorization()
                .WithName("GetChantierById")
                .WithTags("App")
                .WithDescription("Obtenir un chantier par son ID")
                .Produces<Chantier.CompactChantier>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);

            app.MapPost("app/projects/create", async(
                AppDbContext db,
                [FromForm] CreateChantierForm form) =>
            {
                var membresEntities = await db.Utilisateurs.Where(u => form.membres.Contains(u.Id)).ToListAsync();
                var chantier = new Chantier
                {
                    Nom = form.nom,
                    Details = form.details,
                    DateDebut = form.dateDebut,
                    Status = form.status,
                    Membres = membresEntities,
                };
                db.Chantiers.Add(chantier);
                await db.SaveChangesAsync();
                return Results.Created($"/app/projects/{chantier.Id}", chantier.CompactEntity());
            })
                .RequireAuthorization("HighAuthority")
                .WithName("CreateChantier")
                .WithTags("App")
                .WithDescription("Créer un nouveau chantier")
                .Produces<Chantier.CompactChantier>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError)
                .DisableAntiforgery();

            app.MapPost("app/projects/{id}/add_members", async (
                AppDbContext db,
                int id,
                [FromBody] List<int> membres) =>
            {
                var chantier = await db.Chantiers.FindAsync(id);
                if (chantier == null) return Results.NotFound($"Chantier with id {id} not found");
                var membresEntities = await db.Utilisateurs.Where(u => membres.Contains(u.Id)).ToListAsync();
                chantier.Membres.AddRange(membresEntities);
                db.Chantiers.Update(chantier);
                await db.SaveChangesAsync();
                return Results.Ok(chantier.CompactEntity());
            })
                .RequireAuthorization("HighAuthority")
                .WithName("AddChantierMembers")
                .WithTags("App")
                .WithDescription("Ajouter des membres à un chantier")
                .Produces<Chantier.CompactChantier>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);
            // Remove members from a chantier
            app.MapPost("app/projects/{id}/remove_members", async (
                AppDbContext db,
                int id,
                [FromBody] List<int> membres) =>
            {
                var chantier = await db.Chantiers.Include(c => c.Membres).FirstOrDefaultAsync(c => c.Id == id);
                if (chantier == null) return Results.NotFound($"Chantier with id {id} not found");
                var membresEntities = await db.Utilisateurs.Where(u => membres.Contains(u.Id)).ToListAsync();
                chantier.Membres.RemoveAll(m => membresEntities.Contains(m));
                db.Chantiers.Update(chantier);
                await db.SaveChangesAsync();
                return Results.Ok(chantier.CompactEntity());
            })
                .RequireAuthorization("HighAuthority")
                .WithName("RemoveChantierMembers")
                .WithTags("App")
                .WithDescription("Retirer des membres d'un chantier")
                .Produces<Chantier.CompactChantier>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);

            app.MapPut("app/projects/{id}", async(
                AppDbContext db,
                int id,
                [FromForm] string? nom,
                [FromForm] string? details,
                [FromForm] Chantier.StatusChantier? status) =>
            {
                var chantier = await db.Chantiers.FindAsync(id);
                if (chantier == null) return Results.NotFound($"Chantier with id {id} not found");
                if (nom != null) chantier.Nom = nom;
                if (details != null) chantier.Details = details;
                if(status != null) chantier.Status = status.Value;
                db.Chantiers.Update(chantier);
                await db.SaveChangesAsync();
                return Results.Ok(chantier.CompactEntity());
            })
                .RequireAuthorization("HighAuthority")
                .WithName("UpdateChantier")
                .WithTags("App")
                .WithDescription("Mettre à jour un chantier")
                .Produces<Chantier.CompactChantier>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError)
                .DisableAntiforgery();

            app.MapDelete("app/projects/{id}", async (AppDbContext db, int id) =>
            {
                var chantier = await db.Chantiers.FindAsync(id);
                if (chantier == null) return Results.NotFound($"Chantier with id {id} not found");
                db.Annonces.Where(a => a.ChantierId == chantier.Id).ToList().ForEach(a => db.Annonces.Remove(a));
                db.Taches.Where(t => t.ChantierId == chantier.Id).ToList().ForEach(t => db.Taches.Remove(t));
                db.Rapports.Where(r => r.ChantierId == chantier.Id).ToList().ForEach(r => db.Rapports.Remove(r));
                db.Ressources.Where(r => r.ChantierId == chantier.Id).ToList().ForEach(r => db.Ressources.Remove(r));
                db.Mouvements.Where(m => m.ChantierId == chantier.Id).ToList().ForEach(m => db.Mouvements.Remove(m));
                db.Chantiers.Remove(chantier);
                await db.SaveChangesAsync();
                return Results.NoContent();
            })
                .RequireAuthorization("HighAuthority")
                .WithName("DeleteChantier")
                .WithTags("App")
                .WithDescription("Supprimer un chantier")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);

            app.MapGet("app/everyone", async (AppDbContext db) =>
            {
                return Results.Ok(db.Utilisateurs.Select(u => u.CompactEntity()));
            })
                .RequireAuthorization()
                .WithTags("App")
                .WithName("GetAllUsers")
                .WithDescription("Lister tous les utilisateurs")
                .Produces<List<Utilisateur.SafeUtilisateur>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);

            app.MapGet("app/projects/{id}/members", async (AppDbContext db, int id) =>
            {
                var chantier = await db.Chantiers.FindAsync(id);
                if (chantier == null) return Results.NotFound($"Chantier with id {id} not found");
                var membres = await db.Utilisateurs
                    .Include(u => u.Chantiers)
                    .Where(u => u.Chantiers.Any(c => c.Id == chantier.Id))
                    .Select(u => u.CompactEntity())
                    .ToListAsync();
                return Results.Ok(membres);
            })
                .RequireAuthorization()
                .WithName("GetChantierMembers")
                .WithTags("App")
                .WithDescription("Obtenir les membres d'un chantier")
                .Produces<List<Utilisateur.SafeUtilisateur>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);

        }

        public record CreateChantierForm(
            string nom,
            string details,
            DateOnly dateDebut,
            Chantier.StatusChantier status,
            List<int> membres
        );
    }
}
