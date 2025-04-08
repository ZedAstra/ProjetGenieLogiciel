
using Backend.Models;
using Backend.Modules;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json.Serialization;
using System.IO.Compression;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Backend
{
    public class Program
    {
        public static DirectoryInfo ExeDirectory { get; } = new(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
        public static DirectoryInfo BackupDirectory { get; } = new(Path.Combine(ExeDirectory.FullName, "Backups"));
        public static TokenValidationParameters TokenValidationParameters { get; private set; }
        
        public static void Main(string[] args)
        {
            if(args.Length == 1 && args[0] == "create-admin")
            {
                return;
            }
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
                options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
                options.AddPolicy("Chef", policy => policy.RequireRole("Chef"));
                options.AddPolicy("Partenaire", policy => policy.RequireRole("Partenaire"));
                options.AddPolicy("Ouvrier", policy => policy.RequireRole("Ouvrier"));

                // Aggregates
                options.AddPolicy("Management", policy => policy.RequireRole("Admin", "Chef", "Partenaire"));
            });
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])),
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        ClockSkew = TimeSpan.Zero,
                    };
                });
            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            builder.Services.AddDbContext<AppDbContext>();
            builder.Services.AddSingleton<TokenProvider>();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });
            builder.Services.AddCors();
            builder.Services.AddSignalR();
            

            var app = builder.Build();

            var mgr = CheckForMigrations(app);
            if (mgr.Any())
            {
                Console.WriteLine("Migrations applied: " + string.Join(", ", mgr));
            }
            else
            {
                Console.WriteLine("No migrations applied");
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference(opt =>
                {
                    opt.HideModels = true;
                });
            }
            // Disable CORS
            app.UseCors(options => options
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowAnyOrigin());

            AuthModule.Setup(app);
            AdminModule.Setup(app);
            PlanningModule.Setup(app);
            ManagementModule.Setup(app);
            CommunicationModule.Setup(app);

            app.MapHub<CommunicationModule.ChatHub>("communication/hub");

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
                    Email = "admin",
                    Password = "changeme",
                    UserRole = User.Role.Admin
                });
                db.SaveChanges();
                return [];
            }
            else if (db.Database.GetPendingMigrations().Any())
            {
                var migrations = db.Database.GetPendingMigrations().ToList();
                Backup(migrations);
                db.Database.Migrate();
                return migrations;
            }
            else
            {
                return [];
            }
        }


        public static void Backup(List<string> migrations)
        {
            if(!BackupDirectory.Exists)
            {
                BackupDirectory.Create();
            }
            using(ZipArchive archive = ZipFile.Open(Path.Combine(BackupDirectory.FullName, $"{DateTime.Now:yyyy-MM-ddTHH-mm-ss}.zip"), ZipArchiveMode.Create))
            {
                var db = archive.CreateEntry("app.db");
                using (var stream = db.Open())
                {
                    using (var file = new FileStream(Path.Combine(ExeDirectory.FullName, "app.db"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        file.CopyTo(stream);
                    }
                }
                if (File.Exists(Path.Combine(ExeDirectory.FullName, "app.db-shm")))
                {
                    var shm = archive.CreateEntry("app.db-shm");
                    using (var stream = shm.Open())
                    {
                        using (var file = new FileStream(Path.Combine(ExeDirectory.FullName, "app.db-shm"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            file.CopyTo(stream);
                        }
                    }
                    //archive.CreateEntryFromFile(Path.Combine(ExeDirectory.FullName, "app.db-shm"), "app.db-shm");
                }
                if (File.Exists(Path.Combine(ExeDirectory.FullName, "app.db-wal")))
                {
                    var wal = archive.CreateEntry("app.db-wal");
                    using (var stream = wal.Open())
                    {
                        using (var file = new FileStream(Path.Combine(ExeDirectory.FullName, "app.db-wal"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            file.CopyTo(stream);
                        }
                    }
                    //archive.CreateEntryFromFile(Path.Combine(ExeDirectory.FullName, "app.db-wal"), "app.db-wal");
                }
                archive.CreateEntry("migrations.txt").Open().Write(Encoding.UTF8.GetBytes(string.Join('\n', migrations)));
            }
        }
        internal sealed class BearerSecuritySchemeTransformer(Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
        {
            public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
            {
                var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
                if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
                {
                    var requirements = new Dictionary<string, OpenApiSecurityScheme>
                    {
                        ["Bearer"] = new OpenApiSecurityScheme
                        {
                            Type = SecuritySchemeType.Http,
                            Scheme = "bearer",
                            In = ParameterLocation.Header,
                            BearerFormat = "Json Web Token"
                        }
                    };
                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes = requirements;

                    foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
                    {
                        operation.Value.Security.Add(new OpenApiSecurityRequirement
                        {
                            [new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme } }] = Array.Empty<string>()
                        });
                    }
                }
            }
        }
    }
}
