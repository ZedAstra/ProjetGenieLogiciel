using Backend.Models;
using Backend.Models.Management;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Backend.Modules
{
    public class ManagementModule : IModule
    {
        private CultureInfo French = CultureInfo.GetCultureInfo("fr-FR");
        public void Setup(WebApplication app)
        {
            #region Ressources
            app.MapGet("chantier/{chantierId}/management/resources", async (
                AppDbContext db, 
                [Description("L'identifiant du chantier")] int chantierId) =>
            {
                if(!db.Chantiers.Any(c => c.Id == chantierId)) return Results.NotFound("Chantier non trouvé");
                var resources = await db.Ressources.Where(r => r.Chantier.Id == chantierId)
                    .Select(r => r.CompactEntity(db))
                    .ToListAsync();
                return resources.Count > 0 ? Results.Ok(resources) : Results.NotFound("Aucune ressource trouvée pour le chantier spécifié");
            })
                .RequireAuthorization("Authenticated")
                .WithName("GetAllResources")
                .WithTags("Management")
                .WithDescription("Lister toutes les ressources du chantier")
                .Produces<List<Ressource.CompactRessource>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);

            app.MapPost("chantier/{chantierId}/management/resources/create", async (AppDbContext db, 
                [Description("L'identifiant du chantier")] int chantierId, 
                [FromForm] CreateResourceForm form) =>
            {
                if (!db.Chantiers.Any(c => c.Id == chantierId)) return Results.NotFound("Chantier non trouvé");
                var resource = new Ressource
                {
                    Nom = form.nom,
                    Unite = form.unite,
                    ChantierId = chantierId
                };
                var tracked = db.Ressources.Add(resource);
                await db.SaveChangesAsync();
                return Results.CreatedAtRoute($"chantier/{chantierId}/management/resources/{tracked.Entity.Id}", tracked.Entity.CompactEntity(db));
            })
                .RequireAuthorization("Management")
                .WithName("CreateResource")
                .WithTags("Management")
                .WithDescription("Créer une nouvelle ressource")
                .Produces<Ressource.CompactRessource>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError)
                .DisableAntiforgery();

            app.MapGet("chantier/{chantierId}/management/resources/{resourceId}", async (
                AppDbContext db, 
                [Description("L'identifiant du chantier")] int chantierId, 
                [Description("L'identifiant de la ressource")] int resourceId) =>
            {
                if (!db.Chantiers.Any(c => c.Id == chantierId)) return Results.NotFound("Chantier non trouvé");
                var resource = await db.Ressources.FindAsync(chantierId, resourceId);
                return resource is not null ? Results.Ok(resource) : Results.NotFound();
            })
                .RequireAuthorization("Authenticated")
                .WithName("GetResourceById")
                .WithTags("Management")
                .WithDescription("Obtenir une ressource spécifique")
                .Produces<Ressource.CompactRessource>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);

            app.MapGet("chantier/{chantierId}/management/resources/{resourceId}/movements", async (
                AppDbContext db, 
                [Description("L'identifiant du chantier")] int chantierId, 
                [Description("L'identifiant de la ressource")] int resourceId) =>
            {
                if (!db.Chantiers.Any(c => c.Id == chantierId)) return Results.NotFound("Chantier non trouvé");
                var movements = await db.Mouvements.Where(m => m.Ressource.Id == resourceId && m.Ressource.Chantier.Id == chantierId)
                    .Select(m => m.CompactEntity())
                    .ToListAsync();
                return Results.Ok(movements);
            })
                .RequireAuthorization("Authenticated")
                .WithName("GetResourceMovements")
                .WithTags("Management")
                .WithDescription("Lister tous les mouvements d'une ressource spécifique")
                .Produces<List<Mouvement.CompactMouvement>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);

            app.MapPost("chantier/{chantierId}/management/resources/{resourceId}/movements/create", async (
                AppDbContext db, 
                [Description("L'identifiant du chantier")] int chantierId,
                [Description("L'identifiant de la ressource")] int resourceId,
                [FromForm] CreateMovementForm form) =>
            {
                if (!db.Chantiers.Any(c => c.Id == chantierId)) return Results.NotFound("Chantier non trouvé");
                if (!db.Ressources.Any(r => r.Id == resourceId && r.Chantier.Id == chantierId)) return Results.NotFound("Ressource non trouvée");
                var resource = await db.Ressources.FindAsync(chantierId, resourceId);
                if(form.type == Mouvement.TypeMouvement.Sortie && resource!.Quantite(db) < form.quantite) return Results.BadRequest("Quantité insuffisante pour effectuer la sortie");
                var movement = new Mouvement
                {
                    RessourceId = resourceId,
                    Date = form.date,
                    Quantite = form.quantite,
                    Type = form.type
                };
                db.Mouvements.Add(movement);
                var tracked = db.Ressources.Update(resource);
                await db.SaveChangesAsync();
                return Results.CreatedAtRoute($"chantier/{chantierId}/management/resources/{resourceId}/movements/{movement.Id}", tracked.Entity.CompactEntity(db));
            })
                .RequireAuthorization("Management")
                .WithName("CreateMovement")
                .WithTags("Management")
                .WithDescription("Créer un nouveau mouvement pour une ressource spécifique")
                .Produces<Mouvement.CompactMouvement>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError)
                .DisableAntiforgery();

            app.MapGet("chantier/{chantierId}/management/resources/{resourceId}/movements/{movementId}", async (AppDbContext db, [Description("L'identifiant du chantier")] int chantierId, [Description("L'identifiant du mouvement")] int movementId) =>
            {
                if (!db.Chantiers.Any(c => c.Id == chantierId)) return Results.NotFound("Chantier non trouvé");
                if (!db.Ressources.Any(r => r.Id == movementId && r.Chantier.Id == chantierId)) return Results.NotFound("Ressource non trouvée");
                if (!db.Mouvements.Any(m => m.Id == movementId && m.Ressource.Chantier.Id == chantierId)) return Results.NotFound("Mouvement non trouvé");
                var movement = await db.Mouvements.FindAsync(chantierId, movementId);
                return movement is not null ? Results.Ok(movement) : Results.NotFound();
            })
                .RequireAuthorization("Authenticated")
                .WithName("GetMovementById")
                .WithTags("Management")
                .WithDescription("Obtenir un mouvement spécifique d'une ressource spécifiée")
                .Produces<Mouvement.CompactMouvement>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);
            #endregion Ressources

            #region Rapports
            app.MapGet("chantier/{chantierId}/management/report/{annee}/{mois}", async (AppDbContext db, [Description("L'identifiant du chantier")] int chantierId, [Description("Mois")] int mois, [Description("Année")] int annee) =>
            {
                if (!db.Chantiers.Any(c => c.Id == chantierId)) return Results.NotFound("Chantier non trouvé");
                var rapport = await db.Rapports.FindAsync(chantierId, annee, mois);
                if (rapport == null) return Results.NotFound("Aucun rapport trouvé pour le mois et l'année spécifiés");

                return Results.File(rapport.Fichier, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"rapport_{French.DateTimeFormat.GetMonthName(mois)}_{annee}.xlsx", true);
            })
                .RequireAuthorization("Management")
                .WithName("GetReport")
                .WithTags("Management")
                .WithDescription("Obtenir un rapport pour le mois et l'année spécifiés")
                .Produces<byte[]>(StatusCodes.Status200OK, contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);


            app.MapPost("chantier/{chantierId}/management/generate_report", async (AppDbContext db, [Description("L'identifiant du chantier")] int chantierId, [Description("Mois")] int mois, [Description("Année")] int annee) =>
            {
                if (!db.Chantiers.Any(c => c.Id == chantierId)) return Results.NotFound("Chantier non trouvé");
                if (mois < 1 || mois > 12) return Results.BadRequest("Mois invalide");
                if (!db.Mouvements.Any(m => m.Date.Year == annee && m.Date.Month == mois)) return Results.NotFound("Aucun mouvement trouvé pour le mois et l'année spécifiés");
                var doc = GenerateReport(db, mois, annee);

                return Results.CreatedAtRoute($"chantier/{chantierId}/management/report/{annee}/{mois}");
            })
                .RequireAuthorization("Management")
                .WithName("GenerateReport")
                .WithTags("Management")
                .WithDescription("Générer un rapport pour le mois et l'année spécifiés")
                .Produces(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);
            #endregion Rapports

        }

        public byte[] GenerateReport(AppDbContext db, int mois, int année)
        {
            List<Mouvement> mouvements = db.Mouvements
                .Where(m => m.Date.Year == année && m.Date.Month == mois)
                .Include(m => m.Ressource)
                .ToList();
            List<Ressource> ressources = db.Ressources
                .Include(r => r.Chantier)
                .ToList();
            using MemoryStream stream = new MemoryStream();
            using (var workbook = new XLWorkbook())
            {
                var sheet = workbook.AddWorksheet();

                // TODO: Populate the sheet with data

                workbook.Author = "AppName";
                workbook.SaveAs(stream);
            }
            return stream.ToArray();
        }

        // Form data for various endpoints
        public record CreateResourceForm(string nom, string unite);
        public record CreateMovementForm(int resourceId, DateTime date, decimal quantite, Mouvement.TypeMouvement type);
    }
}
