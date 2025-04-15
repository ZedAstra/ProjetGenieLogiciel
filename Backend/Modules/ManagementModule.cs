using Backend.Models;
using Backend.Models.Management;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Diagrams;
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
            #region Ressources
            app.MapGet("chantier/{chantierId}/management/resources", async (
                AppDbContext db, 
                [Description("L'identifiant du chantier")] int chantierId) =>
            {
                if(!db.Chantiers.Any(c => c.Id == chantierId)) return Results.NotFound("Chantier non trouvé");
                var resources = await db.Ressources.Where(r => r.ChantierId == chantierId)
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
                [FromForm] string nom,
                [FromForm] string unite) =>
            {
                var chantier = await db.Chantiers.FindAsync(chantierId);
                if (chantier == null) return Results.NotFound("Chantier non trouvé");
                var resource = new Ressource
                {
                    Nom = nom,
                    Unite = unite,
                    ChantierId = chantierId
                };
                var tracked = db.Ressources.Add(resource);
                await db.SaveChangesAsync();
                return Results.Created($"chantier/{chantierId}/management/resources/{tracked.Entity.Id}", tracked.Entity.CompactEntity(db));
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

            app.MapDelete("chantier/{chantierId}/management/resources/{resourceId}", async (
                AppDbContext db,
                [Description("L'identifiant du chantier")] int chantierId,
                [Description("L'identifiant de la ressource")] int resourceId) =>
            {
                var resource = await db.Ressources.FirstOrDefaultAsync(r => r.Id == resourceId && r.ChantierId == chantierId);
                if (resource == null) return Results.NotFound("Ressource non trouvée");
                db.Ressources.Remove(resource);
                await db.SaveChangesAsync();
                return Results.NoContent();
            })
                .RequireAuthorization("Management")
                .WithName("DeleteResource")
                .WithTags("Management")
                .WithDescription("Supprimer une ressource spécifique")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);

            app.MapGet("chantier/{chantierId}/management/resources/{resourceId}", async (
                AppDbContext db, 
                [Description("L'identifiant du chantier")] int chantierId, 
                [Description("L'identifiant de la ressource")] int resourceId) =>
            {
                if (!db.Chantiers.Any(c => c.Id == chantierId)) return Results.NotFound("Chantier non trouvé");
                var resource = await db.Ressources.FirstOrDefaultAsync(r => r.Id == resourceId && r.ChantierId == chantierId);
                return resource is not null ? Results.Ok(resource.CompactEntity(db)) : Results.NotFound();
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
                var movements = await db.Mouvements.Where(m => m.Ressource.Id == resourceId && m.Ressource.ChantierId == chantierId)
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
                [FromForm] string description,
                [FromForm] DateTime date,
                [FromForm] decimal quantite,
                [FromForm] Mouvement.TypeMouvement type) =>
            {
                var chantier = await db.Chantiers.FindAsync(chantierId);
                if (chantier == null) return Results.NotFound("Chantier non trouvé");
                var resource = await db.Ressources.FirstOrDefaultAsync(r => r.Id == resourceId && r.ChantierId == chantierId);
                if (resource == null) return Results.NotFound("Ressource non trouvée");
                
                if(type == Mouvement.TypeMouvement.Sortie && resource!.Quantite(db) < quantite) return Results.BadRequest("Quantité insuffisante pour effectuer la sortie");
                var movement = new Mouvement
                {
                    ChantierId = chantier.Id,
                    RessourceId = resourceId,
                    Description = description,
                    Date = date,
                    Quantite = quantite,
                    Type = type
                };
                db.Mouvements.Add(movement);
                var tracked = db.Ressources.Update(resource);
                await db.SaveChangesAsync();
                return Results.Created($"chantier/{chantierId}/management/resources/{resourceId}/movements/{movement.Id}", tracked.Entity.CompactEntity(db));
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

            app.MapDelete("chantier/{chantierId}/management/resources/{resourceId}/movements/{movementId}", async (
                AppDbContext db,
                [Description("L'identifiant du chantier")] int chantierId,
                [Description("L'identifiant de la ressource")] int resourceId,
                [Description("L'identifiant du mouvement")] int movementId) =>
            {
                var mouvement = await db.Mouvements.FirstOrDefaultAsync(m => m.ChantierId == chantierId && m.RessourceId == resourceId && m.Id == movementId);
                if (mouvement == null) return Results.NotFound("Mouvement non trouvé");
                db.Mouvements.Remove(mouvement);
                await db.SaveChangesAsync();
                return Results.NoContent();
            })
                .RequireAuthorization("Management")
                .WithName("DeleteMovement")
                .WithTags("Management")
                .WithDescription("Supprimer un mouvement spécifique d'une ressource")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);

            app.MapGet("chantier/{chantierId}/management/resources/{resourceId}/movements/{movementId}", async (
                AppDbContext db, 
                [Description("L'identifiant du chantier")] int chantierId, 
                [Description("L'identifiant du mouvement")] int movementId) =>
            {
                if (!db.Chantiers.Any(c => c.Id == chantierId)) return Results.NotFound("Chantier non trouvé");
                if (!db.Ressources.Any(r => r.Id == movementId && r.ChantierId == chantierId)) return Results.NotFound("Ressource non trouvée");
                if (!db.Mouvements.Any(m => m.Id == movementId && m.Ressource.ChantierId == chantierId)) return Results.NotFound("Mouvement non trouvé");
                var movement = await db.Mouvements.FirstOrDefaultAsync(m => m.Id == movementId && m.RessourceId == movementId && m.ChantierId == chantierId);
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
                var rapport = await db.Rapports.FirstOrDefaultAsync(r => r.ChantierId == chantierId && r.Annee == annee && r.Mois == mois);
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
                var doc = GenerateReport(db, chantierId, mois, annee);
                var existing = await db.Rapports.FirstOrDefaultAsync(r => r.ChantierId == chantierId && r.Annee == annee && r.Mois == mois);
                if(existing != null)
                {
                    existing.Fichier = doc;
                    db.Rapports.Update(existing);
                    await db.SaveChangesAsync();
                }
                else
                {
                    existing = new Rapport()
                    {
                        Annee = annee,
                        Mois = mois,
                        ChantierId = chantierId,
                        Fichier = doc,
                    };
                    db.Rapports.Add(existing);
                    await db.SaveChangesAsync();
                }
                return Results.Created($"chantier/{chantierId}/management/report/{annee}/{mois}", "");
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

            #region Rh
            app.MapPut("chantier/{chantierId}/management/rh/addMember", async (
                AppDbContext db,
                [Description("L'identifiant du chantier")] int chantierId,
                [FromForm] int userId) =>
            {
                var chantier = await db.Chantiers.FindAsync(chantierId);
                if (chantier == null) return Results.NotFound("Chantier non trouvé");
                var utilisateur = await db.Utilisateurs.FindAsync(userId);
                if (utilisateur == null) return Results.NotFound("Utilisateur non trouvé");
                if (chantier.Membres.Contains(utilisateur)) return Results.BadRequest("L'utilisateur est déjà membre du chantier");
                chantier.Membres.Add(utilisateur);
                db.Chantiers.Update(chantier);
                await db.SaveChangesAsync();
                return Results.Ok();
            })
                .RequireAuthorization("HighAuthority")
                .WithName("AddMember")
                .WithTags("Management")
                .WithDescription("Ajouter un membre à un chantier")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .DisableAntiforgery();
            #endregion Rh
        }

        public byte[] GenerateReport(AppDbContext db, int chantierId, int mois, int année)
        {
            List<Mouvement> mouvements = db.Mouvements
                .Where(m => m.ChantierId == chantierId && m.Date.Year == année && m.Date.Month == mois)
                .Include(m => m.Ressource)
                .ToList();
            List<Ressource> ressources = db.Ressources
                .Where(r => r.ChantierId == chantierId)
                .ToList();
            using MemoryStream stream = new MemoryStream();
            using (var workbook = new XLWorkbook())
            {
                var sheet = workbook.AddWorksheet();

                // TODO: Populate the sheet with data
                sheet.Column("A").Width = 12;
                sheet.Cell("A1").Value = "Ressource";
                BorderAll(sheet.Cell("A1"));
                sheet.Cell("A2").Value = "Quantité";
                BorderAll(sheet.Cell("A2"));
                sheet.Cell("A3").Value = "Unité";
                BorderAll(sheet.Cell("A3"));
                for (int i = 0; i < ressources.Count; i++)
                {
                    
                    sheet.Cell(1, 2 + i).Value = ressources[i].Nom;
                    sheet.Cell(2, 2 + i).Value = ressources[i].Quantite(db);
                    sheet.Cell(3, 2 + i).Value = ressources[i].Unite;
                }
                sheet.Cell("A5").Value = "Mouvements";

                sheet.Cell("A6").Value = "Ressource";
                BorderAll(sheet.Cell("A6"));
                sheet.Cell("A7").Value = "Type";
                BorderAll(sheet.Cell("A7"));
                sheet.Cell("A8").Value = "Quantité";
                BorderAll(sheet.Cell("A8"));
                sheet.Cell("A9").Value = "Date";
                BorderAll(sheet.Cell("A9"));

                for (int i = 0; i < mouvements.Count; i++)
                {
                    sheet.Column(2 + i).Width = 20;
                    sheet.Cell(6, i + 2).Value = mouvements[i].Ressource.Nom;
                    sheet.Cell(7, i + 2).Value = mouvements[i].Type.ToString();
                    sheet.Cell(8, i + 2).Value = mouvements[i].Quantite;
                    sheet.Cell(9, i + 2).Value = mouvements[i].Date.ToString();
                    
                }

                workbook.Author = "AppName";
                workbook.SaveAs(stream);
            }
            return stream.ToArray();
        }

        private void BorderAll(IXLCell cell)
        {
            cell.Style.Border.TopBorder = XLBorderStyleValues.Thick;
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thick;
            cell.Style.Border.LeftBorder = XLBorderStyleValues.Thick;
            cell.Style.Border.RightBorder = XLBorderStyleValues.Thick;

        }

        // Form data for various endpoints
        public record CreateResourceFor(string nom, string unite);
        public record CreateMovementFor(int resourceId, DateTime date, decimal quantite, Mouvement.TypeMouvement type);
    }
}
