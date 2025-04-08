using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class CommsChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Périodes_TacheId",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Périodes",
                table: "Périodes");

            migrationBuilder.RenameTable(
                name: "Périodes",
                newName: "Taches");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Taches",
                table: "Taches",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Annonces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Titre = table.Column<string>(type: "TEXT", nullable: false),
                    Publication = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Contenu = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Annonces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Plaintes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Titre = table.Column<string>(type: "TEXT", nullable: false),
                    AuteurId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Contenu = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plaintes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Plaintes_Users_AuteurId",
                        column: x => x.AuteurId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Plaintes_AuteurId",
                table: "Plaintes",
                column: "AuteurId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Taches_TacheId",
                table: "Users",
                column: "TacheId",
                principalTable: "Taches",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Taches_TacheId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Annonces");

            migrationBuilder.DropTable(
                name: "Plaintes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Taches",
                table: "Taches");

            migrationBuilder.RenameTable(
                name: "Taches",
                newName: "Périodes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Périodes",
                table: "Périodes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Périodes_TacheId",
                table: "Users",
                column: "TacheId",
                principalTable: "Périodes",
                principalColumn: "Id");
        }
    }
}
