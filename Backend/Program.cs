
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
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

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
                options.AddPolicy("HighAuthority", policy => policy.RequireRole("Admin", "Chef"));
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
            builder.Services.AddOpenApi("v1", options => { 
                //options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
                //options.AddSchemaTransformer<SchemaTransformer>();
            });
            builder.Services.AddCors();
            builder.Services.AddSignalR();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SupportNonNullableReferenceTypes();

                options.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme()
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearerAuth" }
                        },
                        []
                    }
                });
                options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "V1 API",
                        Version = "v1",
                        Description = "Description",
                        //TermsOfService = new Uri("https://scalar-swagger-example.com"),
                    }
                );
            });
            

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
            //if (app.Environment.IsDevelopment())
            //{
                app.UseSwagger(setup =>
                {
                    setup.RouteTemplate = "{documentName}/schema.json";
                });
                app.MapOpenApi().CacheOutput();
                app.MapScalarApiReference(options =>
                {
                    options.ForceThemeMode = ThemeMode.Light;
                    options.WithOpenApiRoutePattern("v1/schema.json");
                });
            //}
            // Disable CORS
            app.UseCors(options => options
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowAnyOrigin());

            List<IModule> modules = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(IModule).IsAssignableFrom(type) && !type.IsAbstract)
                .Select(type => (IModule)Activator.CreateInstance(type)!)
                .ToList();

            foreach (var module in modules) module.Setup(app);

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
                db.Utilisateurs.Add(new Utilisateur()
                {
                    Prenom = "John",
                    Nom = "Doe",
                    Email = "admin",
                    MotDePasse = "changeme",
                    RoleUtilisateur = Utilisateur.Role.Admin
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
