using Backend.Models;
using Backend.Models.Management;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Backend.Modules
{
    public class AdminModule : IModule
    {
        public void Setup(WebApplication app)
        {
            BuildCRUD(app, typeof(Chantier));
            BuildCRUD(app, typeof(Utilisateur));
            BuildCRUD(app, typeof(Ressource));
            BuildCRUD(app, typeof(Mouvement));
        }

        public static void BuildCRUD(WebApplication app, Type type)
        {
            //Type typeArray = type.MakeArrayType();
            Type typeArray = typeof(List<>).MakeGenericType(type);
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

        private delegate Task<object> ReadHandler<TResult>(HttpContext ctx, TokenProvider tokenProvider, AppDbContext db, [FromRoute] int id);
        private static async Task<object> Read<TResult>(HttpContext ctx, TokenProvider tokenProvider, AppDbContext db, int id)
        {
            return Results.Json(db.Find(typeof(TResult), id));
        }

        private delegate Task<object> UpdateHandler<TResult>(HttpContext ctx, TokenProvider tokenProvider, AppDbContext db, [FromRoute] int id, [FromBody] TResult obj);
        private static async Task<object> Update<TResult>(HttpContext ctx, TokenProvider tokenProvider, AppDbContext db, int id, TResult obj)
        {
            var existing = db.Find(typeof(TResult), id);
            if (existing == null) return Results.NotFound();
            obj.GetType().GetProperty("Id").SetValue(obj, existing.GetType().GetProperty("Id").GetValue(existing));
            db.Update(existing).CurrentValues.SetValues(obj);
            await db.SaveChangesAsync();
            return Results.Json(obj);
        }

        private delegate Task<object> DeleteHandler<TResult>(HttpContext ctx, TokenProvider tokenProvider, AppDbContext db, [FromRoute] int id);
        private static async Task<object> Delete<TResult>(HttpContext ctx, TokenProvider tokenProvider, AppDbContext db, int id)
        {
            db.Remove(db.Find(typeof(TResult), id));
            await db.SaveChangesAsync();
            return Results.Ok();
        }
    }
}
