using Backend.Models;
using Backend.Models.Communication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Backend.Modules
{
    public class CommunicationModule
    {

        private static List<WebSocket> _sockets = new List<WebSocket>();

        public static void Setup(WebApplication app)
        {
            app.MapGet("/communication/channels", async (HttpContext ctx, AppDbContext db, TokenProvider provider) =>
            {
                //if (!await provider.IsAuthorized(ctx.Request, db, user => user.UserRole != User.Role.None)) return Results.Unauthorized();
                var user = await provider.GetUser(ctx.Request.Headers["Authorization"], db);
                return Results.Json(db.Channels.Where(c => c.Participants.Contains(user)).Select(cannal => cannal.ToSmall()));
            })
                .RequireAuthorization("Authenticated")
                .WithTags("Communication")
                .WithName("communication.channels")
                .WithDisplayName("Communication")
                .WithDescription("Lister les canaux de communication disponnibles pour l'utilisateur")
                .Produces<List<Cannal.Small>>(200)
                .Produces(401);

            app.MapPost("/communication/channels/create", async (HttpContext ctx, AppDbContext db, TokenProvider provider, [FromForm] string nom, [FromForm] string description) =>
            {
                if (!await provider.IsAuthorized(ctx.Request, db, user => user.UserRole != User.Role.None)) return Results.Unauthorized();
                var user = await provider.GetUser(ctx.Request.Headers["Authorization"], db);
                if (user == null) return Results.InternalServerError();
                var channel = new Cannal()
                {
                    Nom = nom,
                    Description = description,
                    Admins = [user],
                    Participants = [user]
                };
                db.Channels.Add(channel);
                await db.SaveChangesAsync();
                return Results.Created($"/communication/channels/{channel.Id}", channel.ToSmall());
            })
                .RequireAuthorization("Authenticated")
                .WithTags("Communication")
                .WithName("communication.channels.create")
                .WithDisplayName("Communication")
                .WithDescription("Créer un nouveau canal de communication")
                .Produces<Cannal>(201)
                .Produces(401)
                .Produces(500)
                .DisableAntiforgery();

            app.MapGet("/communication/channels/{channelId}/all", async (HttpContext ctx, AppDbContext db, TokenProvider provider, Guid channelId) =>
            {
                if (!await provider.IsAuthorized(ctx.Request, db, user => user.UserRole != User.Role.None)) return Results.Unauthorized();
                var user = await provider.GetUser(ctx.Request.Headers["Authorization"], db);
                var channel = db.Channels.Find(channelId);
                if (channel == null) return Results.NotFound();
                if (!channel.Participants.Contains(user)) return Results.Unauthorized();
                var messages = db.Messages.Where(m => m.Cannal.Id == channelId);
                return Results.Json(messages);
            })
                .RequireAuthorization("Authenticated")
                .WithTags("Communication")
                .WithName("communication.channels.all")
                .WithDisplayName("Communication")
                .WithDescription("Lister tous les messages d'un canal de communication")
                .Produces<List<Message>>(200)
                .Produces(401)
                .Produces(404);

            app.MapGet("/communication/news", async (AppDbContext db) =>
            {
                return Results.Ok(db.Annonces.ToList());
            })
                .RequireAuthorization("Authenticated")
                .WithTags("Communication")
                .WithName("communication.news")
                .WithDisplayName("Annonces")
                .WithDescription("Lister les annonces")
                .Produces(200)
                .Produces(401);

            app.MapGet("/communication/news/{id}", async (AppDbContext db, Guid id) =>
            {
                var annonce = db.Annonces.Find(id);
                if (annonce == null) return Results.NotFound();
                return Results.Ok(annonce);
            })
                .RequireAuthorization("Authenticated")
                .WithTags("Communication")
                .WithName("communication.news.read")
                .WithDisplayName("Annonces")
                .WithDescription("Lire une annonce")
                .Produces(200)
                .Produces(401)
                .Produces(404);

            app.MapDelete("/communication/news/{id}", async (AppDbContext db, Guid id) =>
            {
                var existing = db.Annonces.Find(id);
                if (existing == null) return Results.NotFound();
                db.Annonces.Remove(existing);
                await db.SaveChangesAsync();
                return Results.Accepted();
            })
                .RequireAuthorization("Management")
                .WithTags("Communication")
                .WithName("communication.news.delete")
                .WithDisplayName("Annonces")
                .WithDescription("Supprimer une annonce")
                .Produces(202)
                .Produces(401)
                .Produces(404);

            app.MapPost("/communication/news/create", async (AppDbContext db, [FromBody] NewsFormData data) =>
            {
                var annonce = new Annonce()
                {
                    Titre = data.title,
                    Publication = DateTime.Now,
                    Contenu = data.content
                };
                db.Annonces.Add(annonce);
                await db.SaveChangesAsync();
                return Results.Created($"/communication/news/{annonce.Id}", annonce);
            })
                .RequireAuthorization("Management")
                .WithTags("Communication")
                .WithName("communication.news.create")
                .WithDisplayName("Annonces")
                .WithDescription("Créer une annonce")
                .Produces(201)
                .Produces(401)
                .Produces(500);

            app.MapGet("/communication/complaints", async (AppDbContext db) =>
            {
                return Results.Ok(db.Plaintes.ToList());
            })
                .RequireAuthorization("Authenticated")
                .WithTags("Communication")
                .WithName("communication.complaints")
                .WithDisplayName("Plaintes")
                .WithDescription("Lister les plaintes")
                .Produces<List<Plainte.Small>>(200)
                .Produces(401);

            app.MapGet("/communication/complaints/{id}", async (AppDbContext db, Guid id) =>
            {
                var plainte = db.Plaintes.Find(id);
                if (plainte == null) return Results.NotFound();
                return Results.Ok(plainte);
            })
                .RequireAuthorization("Authenticated")
                .WithTags("Communication")
                .WithName("communication.complaints.read")
                .WithDisplayName("Plaintes")
                .WithDescription("Lire une plainte")
                .Produces(200)
                .Produces(401)
                .Produces(404);

            app.MapPost("/communication/complaints/create", async (AppDbContext db, [FromBody] ComplaintFormData plainte) =>
            {
                var author = await db.Users.FindAsync(plainte.author);
                if(author == null) return Results.NotFound();
                var newPlainte = new Plainte()
                {
                    Titre = plainte.title,
                    Auteur = author,
                    Contenu = plainte.content,
                };
                db.Plaintes.Add(newPlainte);
                await db.SaveChangesAsync();
                return Results.Created($"/communication/complaints/{newPlainte.Id}", newPlainte.ToSmall());
            })
                .RequireAuthorization("Authenticated")
                .WithTags("Communication")
                .WithName("communication.complaints.create")
                .WithDisplayName("Plaintes")
                .WithDescription("Créer une plainte")
                .Produces<Plainte.Small>(201)
                .Produces(401)
                .Produces(500);
        }

        public record NewsFormData(string title, string content);
        public record ComplaintFormData(string title, Guid author, string content);

        public class ChatHub : Hub
        {
            public override async Task OnConnectedAsync()
            {
                Console.WriteLine("Client connected");
            }
            public override async Task OnDisconnectedAsync(Exception? exception)
            {
                Console.WriteLine("Client disconnected");
            }
            public async Task SendMessage(Guid userId, Guid cannalId, string message, AppDbContext db)
            {
                User? user = db.Users.Find(userId);
                if (user == null) return;
                Cannal? cannal = db.Channels.Find(cannalId);
                if (cannal == null) return;
                db.Messages.Add(new Message()
                {
                    Auteur = user,
                    Cannal = cannal,
                    Contenu = message
                });
                await db.SaveChangesAsync();

                //await Clients.Group(cannal.Id.ToString()).SendAsync("ReceiveMessage", userId, message);
            }
            
        }
    }
}
