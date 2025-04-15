using Backend.Models;
using Backend.Models.Planning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Backend.Modules
{
    public class PlanningModule : IModule
    {
        public void Setup(WebApplication app)
        {
            app.MapGet("chantier/{chantierId}/planning/tasks/", async (
                AppDbContext db,
                [Description("L'identifiant du chantier")] int chantierId) =>
            {
                var taches = await db.Taches
                    .Where(t => t.ChantierId == chantierId)
                    .AsNoTracking()
                    .ToListAsync();
                return Results.Ok(taches);
            })
                .RequireAuthorization()
                .WithName("GetChantierTasks")
                .WithTags("Planning")
                .WithDescription("Récupérer les tâches d'un chantier")
                .Produces<List<Tache>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

            app.MapPost("chantier/{chantierId}/planning/tasks/create", async (
                AppDbContext db,
                [Description("L'identifiant du chantier")] int chantierId,
                [FromForm] string titre,
                [FromForm] string description,
                [FromForm] DateTime dateDebut,
                [FromForm] DateTime dateFin) =>
            {
                if (await db.Chantiers.FindAsync(chantierId) is null) return Results.NotFound("Chantier introuvable");
                var tache = new Tache
                {
                    Titre = titre,
                    Description = description,
                    DateDebut = dateDebut,
                    DateFin = dateFin,
                    ChantierId = chantierId
                };
                await db.Taches.AddAsync(tache);
                await db.SaveChangesAsync();
                return Results.Created($"/chantier/{chantierId}/planning/tasks/{tache.Id}", tache);
            })
                .RequireAuthorization("Management")
                .WithName("CreateTask")
                .WithTags("Planning")
                .WithDescription("Créer une tâche pour un chantier")
                .Produces<Tache>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError)
                .DisableAntiforgery();

            app.MapPut("chantier/{chantierId}/planning/tasks/{taskId}", async (
                AppDbContext db,
                [Description("L'identifiant du chantier")] int chantierId,
                [Description("L'identifiant de la tâche")] int taskId,
                [FromForm] string? titre,
                [FromForm] string? description,
                [FromForm] Tache.EtatTache status
                ) => 
            {
                var tache = await db.Taches.FindAsync(chantierId, taskId);
                if (tache == null) return Results.NotFound("Tâche introuvable");
                if (titre != null) tache.Titre = titre;
                if (description != null) tache.Description = description;
                tache.Status = status;
                db.Taches.Update(tache);
                await db.SaveChangesAsync();
                return Results.Ok(tache);
            })
                .RequireAuthorization("Management")
                .WithName("UpdateTask")
                .WithTags("Planning")
                .WithDescription("Mettre à jour une tâche d'un chantier")
                .Produces<Tache>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError)
                .DisableAntiforgery();

            app.MapDelete("chantier/{chantierId}/planning/tasks/{taskId}", async (
                AppDbContext db,
                [Description("L'identifiant du chantier")] int chantierId,
                [Description("L'identifiant de la tâche")] int taskId) =>
            {
                var tache = await db.Taches.FindAsync(chantierId, taskId);
                if (tache == null) return Results.NotFound("Tâche introuvable");
                db.Taches.Remove(tache);
                await db.SaveChangesAsync();
                return Results.NoContent();
            })
                .RequireAuthorization("Management")
                .WithName("DeleteTask")
                .WithTags("Planning")
                .WithDescription("Supprimer une tâche d'un chantier")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

            app.MapGet("chantier/{chantierId}/planning/tasks/planned", async (
                AppDbContext db,
                [Description("L'identifiant du chantier")] int chantierId) =>
            {
                var taches = await db.Taches
                    .Where(t => t.ChantierId == chantierId && t.Status == Tache.EtatTache.NonCommencee)
                    .AsNoTracking()
                    .ToListAsync();
            })
                .RequireAuthorization()
                .WithName("GetPlannedTasks")
                .WithTags("Planning")
                .WithDescription("Récupérer les tâches planifiées d'un chantier")
                .Produces<List<Tache>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

            app.MapGet("chantier/{chantierId}/planning/tasks/ongoing", async (
                AppDbContext db,
                [Description("L'identifiant du chantier")] int chantierId) =>
            {
                var taches = await db.Taches
                    .Where(t => t.ChantierId == chantierId && t.Status == Tache.EtatTache.EnCours)
                    .AsNoTracking()
                    .ToListAsync();
                return Results.Ok(taches);
            })
                .RequireAuthorization()
                .WithName("GetOngoingTasks")
                .WithTags("Planning")
                .WithDescription("Récupérer les tâches en cours d'un chantier")
                .Produces<List<Tache>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

            app.MapGet("chantier/{chantierId}/planning/tasks/completed", async (
                AppDbContext db,
                [Description("L'identifiant du chantier")] int chantierId) =>
            {
                var taches = await db.Taches
                    .Where(t => t.ChantierId == chantierId && t.Status == Tache.EtatTache.Terminee)
                    .AsNoTracking()
                    .ToListAsync();
                return Results.Ok(taches);
            })
                .RequireAuthorization()
                .WithName("GetCompletedTasks")
                .WithTags("Planning")
                .WithDescription("Récupérer les tâches terminées d'un chantier")
                .Produces<List<Tache>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

            app.MapGet("chantier/{chantierId}/planning/tasks/cancelled", async (
                AppDbContext db,
                [Description("L'identifiant du chantier")] int chantierId) =>
            {
                var taches = await db.Taches
                    .Where(t => t.ChantierId == chantierId && t.Status == Tache.EtatTache.Annulee)
                    .AsNoTracking()
                    .ToListAsync();
                return Results.Ok(taches);
            })
                .RequireAuthorization()
                .WithName("GetCancelledTasks")
                .WithTags("Planning")
                .WithDescription("Récupérer les tâches annulées d'un chantier")
                .Produces<List<Tache>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);
        }
    }
}
