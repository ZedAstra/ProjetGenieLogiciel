using Backend.Models;
using Backend.Models.Communication;
using Backend.Models.Management;
using Backend.Models.Planning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Backend.Modules
{
    public class AdminModule
    {
        public static void Setup(WebApplication app)
        {
            BuildCRUD(app, typeof(User));
            BuildCRUD(app, typeof(Tache));
            BuildCRUD(app, typeof(Cannal));
            BuildCRUD(app, typeof(Plainte));
            BuildCRUD(app, typeof(Message));
            BuildCRUD(app, typeof(Annonce));
            BuildCRUD(app, typeof(Document));
            BuildCRUD(app, typeof(Ressource));
        }

        public static void BuildCRUD(WebApplication app, Type type)
        {
            Type typeArray = type.MakeArrayType();
            // All
            Delegate listHandler = Delegate.CreateDelegate(typeof(ListHandler<>).MakeGenericType(type), typeof(AdminModule).GetMethod("List", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(type));
            app.MapGet($"admin/{type.Name.ToLower()}", listHandler)
                .RequireAuthorization("Admin")
                .WithTags($"Admin/{type.Name}")
                .WithName($"admin.{type.Name.ToLower()}.list")
                .WithDisplayName($"Liste des objets de type '{type.Name}'")
                .WithDescription($"Lister les objets de type '{type.Name}'")
                .Produces(200, typeArray)
                .Produces(401);

            // Create
            Delegate createHandler = Delegate.CreateDelegate(typeof(CreateHandler<>).MakeGenericType(type), typeof(AdminModule).GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(type));
            app.MapPost($"/admin/{type.Name.ToLower()}/create", createHandler)
                .RequireAuthorization("Admin")
                .WithTags($"Admin/{type.Name}")
                .WithName($"admin.{type.Name.ToLower()}.create")
                .WithDisplayName($"Créer {type.Name}")
                .WithDescription($"Créer un nouvel objet de type '{type.Name}'")
                .Produces(200, type)
                .Produces(401);

            // Read
            Delegate readHandler = Delegate.CreateDelegate(typeof(ReadHandler<>).MakeGenericType(type), typeof(AdminModule).GetMethod("Read", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(type));
            app.MapGet($"/admin/{type.Name.ToLower()}/{{id}}", readHandler)
                .RequireAuthorization("Admin")
                .WithTags($"Admin/{type.Name}")
                .WithName($"admin.{type.Name.ToLower()}.read")
                .WithDisplayName($"Récuperer {type.Name}")
                .WithDescription($"Récuperer l'objet de type '{type.Name}' spécifié")
                .Produces(200, type)
                .Produces(401);

            // Update
            Delegate updateHandler = Delegate.CreateDelegate(typeof(UpdateHandler<>).MakeGenericType(type), typeof(AdminModule).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(type));
            app.MapPut($"/admin/{type.Name.ToLower()}/{{id}}", updateHandler)
                .RequireAuthorization("Admin")
                .WithTags($"Admin/{type.Name}")
                .WithName($"admin.{type.Name.ToLower()}.update")
                .WithDisplayName($"Modifier {type.Name}")
                .WithDescription($"Modifier l'objet de type '{type.Name}' spécifié")
                .Produces(200, type)
                .Produces(401);

            // Delete
            Delegate deleteHandler = Delegate.CreateDelegate(typeof(DeleteHandler<>).MakeGenericType(type), typeof(AdminModule).GetMethod("Delete", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(type));
            app.MapDelete($"/admin/{type.Name.ToLower()}/{{id}}", deleteHandler)
                .RequireAuthorization("Admin")
                .WithTags($"Admin/{type.Name}")
                .WithName($"admin.{type.Name.ToLower()}.delete")
                .WithDisplayName($"Supprimer {type.Name}")
                .WithDescription($"Supprimer l'objet de type '{type.Name}' spécifié")
                .Produces(200)
                .Produces(401);
        }

        private delegate Task<object> ListHandler<TResult>(HttpContext ctx, TokenProvider tokenProvider, AppDbContext db);
        private static async Task<object> List<TResult>(HttpContext ctx, TokenProvider tokenProvider, AppDbContext db)
        {
            return Results.Json(db.GetType().GetMethods().Where(m => m.Name == "Set" && m.IsGenericMethod).First().MakeGenericMethod(typeof(TResult)).Invoke(db, null));
        }


        private delegate Task<object> CreateHandler<TResult>(HttpContext ctx, TokenProvider tokenProvider, AppDbContext db, [FromBody] TResult obj);
        private static async Task<object> Create<TResult>(HttpContext ctx, TokenProvider tokenProvider, AppDbContext db, [FromBody] TResult obj)
        {
            db.Add(obj);
            await db.SaveChangesAsync();
            return Results.Json(obj);
        }

        private delegate Task<object> ReadHandler<TResult>(HttpContext ctx, TokenProvider tokenProvider, AppDbContext db, [FromRoute] Guid id);
        private static async Task<object> Read<TResult>(HttpContext ctx, TokenProvider tokenProvider, AppDbContext db, Guid id)
        {
            return Results.Json(db.Find(typeof(TResult), id));
        }

        private delegate Task<object> UpdateHandler<TResult>(HttpContext ctx, TokenProvider tokenProvider, AppDbContext db, [FromRoute] Guid id, [FromBody] TResult obj);
        private static async Task<object> Update<TResult>(HttpContext ctx, TokenProvider tokenProvider, AppDbContext db, Guid id, TResult obj)
        {
            var existing = db.Find(typeof(TResult), id);
            if (existing == null) return Results.NotFound();
            obj.GetType().GetProperty("Id").SetValue(obj, existing.GetType().GetProperty("Id").GetValue(existing));
            db.Update(existing).CurrentValues.SetValues(obj);
            await db.SaveChangesAsync();
            return Results.Json(obj);
        }

        private delegate Task<object> DeleteHandler<TResult>(HttpContext ctx, TokenProvider tokenProvider, AppDbContext db, [FromRoute] Guid id);
        private static async Task<object> Delete<TResult>(HttpContext ctx, TokenProvider tokenProvider, AppDbContext db, Guid id)
        {
            db.Remove(db.Find(typeof(TResult), id));
            await db.SaveChangesAsync();
            return Results.Ok();
        }
    }
}
