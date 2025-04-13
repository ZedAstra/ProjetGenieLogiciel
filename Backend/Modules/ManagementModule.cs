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
            app.MapGet("chantier/{chantierId}/management/resources", async (AppDbContext db, [Description("L'identifiant du chantier")] int chantierId) =>
            {
                var resources = await db.Ressources.Where(r => r.Chantier.Id == chantierId)
                    .Select(r => r.CompactEntity())
                    .ToListAsync();
                return Results.Ok(resources);
            })
                .RequireAuthorization("Authenticated")
                .WithName("GetAllResources")
                .WithTags("Management")
                .WithDescription("Lister toutes les ressources du chantier")
                .Produces<List<Ressource.CompactRessource>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);

            app.MapGet("chantier/{chantierId}/management/resources/{resourceId}", async (AppDbContext db, [Description("L'identifiant du chantier")] int chantierId, [Description("L'identifiant de la ressource")] int resourceId) =>
            {
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

            app.MapGet("chantier/{chantierId}/management/resources/{resourceId}/movements", async (AppDbContext db, [Description("L'identifiant du chantier")] int chantierId, [Description("L'identifiant de la ressource")] int resourceId) =>
            {
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
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);

            app.MapGet("chantier/{chantierId}/management/movement/{movementId}", async (AppDbContext db, [Description("L'identifiant du chantier")] int chantierId, [Description("L'identifiant du mouvement")] int movementId) =>
            {
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

            #region Rapports
            app.MapGet("chantier/{chantierId}/management/report/{annee}/{mois}", async (AppDbContext db, [Description("L'identifiant du chantier")] int chantierId, [Description("Mois")] int mois, [Description("Année")] int annee) =>
            {
                var rapport = await db.Rapports.FindAsync(chantierId, annee, mois);
                if (rapport == null) return Results.NotFound("Aucun rapport trouvé pour le mois et l'année spécifiés");

                return Results.File(rapport.Fichier, "application/vnd.ms-excel", $"rapport_{French.DateTimeFormat.GetMonthName(mois)}_{annee}.xlsx", true);
            })
                .RequireAuthorization("Management")
                .WithName("GetReport")
                .WithTags("Management")
                .WithDescription("Obtenir un rapport pour le mois et l'année spécifiés")
                .Produces<byte[]>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);


            app.MapPost("chantier/{chantierId}/management/generate_report", async (AppDbContext db, [Description("L'identifiant du chantier")] int chantierId, [Description("Mois")] int mois, [Description("Année")] int annee) =>
            {
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
    }
}
