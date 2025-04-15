using Backend.Models;
using Backend.Models.Communication;
using DocumentFormat.OpenXml.Drawing.Diagrams;
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
    public class CommunicationModule : IModule
    {
        public void Setup(WebApplication app)
        {
            app.MapGet("chantier/{chantierId}/communication/news", async (
                AppDbContext db,
                [Description("L'identifiant du chantier")] int chantierId) =>
            {
                var news = await db.Annonces.Where(a => a.ChantierId == chantierId)
                    .ToListAsync();
                return Results.Ok(news);
            })
                .RequireAuthorization()
                .WithName("GetChantierNews")
                .WithTags("Communication")
                .WithDescription("Récupérer les annonces d'un chantier")
                .Produces<List<Annonce>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

            app.MapGet("chantier/{chantierId}/communication/news/{newsId}", async(
                AppDbContext db,
                [Description("L'identifiant du chantier")] int chantierId,
                [Description("L'identifiant de l'annonce")] int newsId) =>
            {
                var news = await db.Annonces.FirstOrDefaultAsync(a => a.ChantierId == chantierId && a.Id == newsId);
                if (news == null) return Results.NotFound("Annonce introuvable");
                return Results.Ok(news);
            })
                .RequireAuthorization()
                .WithName("GetChantierNewsById")
                .WithTags("Communication")
                .WithDescription("Récupérer une annonce d'un chantier par son identifiant")
                .Produces<Annonce>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

            app.MapPost("chantier/{chantierId}/communication/news/create", async(
                AppDbContext db,
                [Description("L'identifiant du chantier")] int chantierId,
                [FromForm] string titre,
                [FromForm] string description,
                [FromForm] DateTime date,
                [FromForm] string content) =>
            {
                if (await db.Chantiers.FindAsync(chantierId) is null) return Results.NotFound("Chantier introuvable");
                var annonce = new Annonce()
                {
                    ChantierId = chantierId,
                    Titre = titre,
                    Description = description,
                    Date = date,
                    Contenu = content,
                };
                var tracked = db.Annonces.Add(annonce);
                await db.SaveChangesAsync();
                return Results.Created($"/chantier/{chantierId}/communication/news/{tracked.Entity.Id}", tracked.Entity);
            })
                .RequireAuthorization("Management")
                .WithName("CreateChantierNews")
                .WithTags("Communication")
                .WithDescription("Créer une annonce pour un chantier")
                .Produces<Annonce>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError)
                .DisableAntiforgery();
        }
    }
}
