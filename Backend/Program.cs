
using Backend.Models;
using Backend.Modules;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Backend
{
    public class Program
    {
        public static DirectoryInfo ExeDirectory { get; } = new(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])),
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        ClockSkew = TimeSpan.Zero,
                    };
                });

            builder.Services.AddDbContext<AppDbContext>();
            builder.Services.AddSingleton<TokenProvider>();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            var mgr = CheckForMigrations(app);
            if (mgr.Any())
            {
                Console.WriteLine("Migrations applied: " + string.Join(", ", mgr));
            }
            else
            {
                Console.WriteLine("No migrations applied");
                return;
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            AuthModule.Setup(app);
            PlanningModule.Setup(app);

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.Run();
        }

        public static List<string> CheckForMigrations(WebApplication app)
        {
            var db = app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
            if (ExeDirectory.GetFiles("app.db").Count() != 1)
            {
                db.Database.Migrate();
                db.Users.Add(new User()
                {
                    Id = Guid.NewGuid(),
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "email@email.com",
                    Password = 
                    Type = User.UserType.Admin,
                    UserRole = User.Role.Chef
                });
            }
            if (db.Database.GetPendingMigrations().Any())
            {
                db.Database.Migrate();
                return db.Database.GetPendingMigrations().ToList();
            }
            else
            {
                return [];
            }
        }
    }
}
