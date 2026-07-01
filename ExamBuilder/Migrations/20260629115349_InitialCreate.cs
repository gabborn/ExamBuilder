using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExamBuilder.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lehrveranstaltungen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titel = table.Column<string>(type: "TEXT", nullable: false),
                    Dozentenname = table.Column<string>(type: "TEXT", nullable: true),
                    Abschlussart = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lehrveranstaltungen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kapitel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titel = table.Column<string>(type: "TEXT", nullable: false),
                    Kapitelnummer = table.Column<int>(type: "INTEGER", nullable: false),
                    Vorlesungsfolien = table.Column<string>(type: "TEXT", nullable: true),
                    LehrveranstaltungId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kapitel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kapitel_Lehrveranstaltungen_LehrveranstaltungId",
                        column: x => x.LehrveranstaltungId,
                        principalTable: "Lehrveranstaltungen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pruefungen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titel = table.Column<string>(type: "TEXT", nullable: false),
                    Datum = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Beschreibung = table.Column<string>(type: "TEXT", nullable: true),
                    LehrveranstaltungId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pruefungen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pruefungen_Lehrveranstaltungen_LehrveranstaltungId",
                        column: x => x.LehrveranstaltungId,
                        principalTable: "Lehrveranstaltungen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "McFragen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Fragentext = table.Column<string>(type: "TEXT", nullable: false),
                    KapitelId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McFragen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McFragen_Kapitel_KapitelId",
                        column: x => x.KapitelId,
                        principalTable: "Kapitel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "McAntworten",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Antworttext = table.Column<string>(type: "TEXT", nullable: false),
                    Korrekt = table.Column<bool>(type: "INTEGER", nullable: false),
                    McFrageId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McAntworten", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McAntworten_McFragen_McFrageId",
                        column: x => x.McFrageId,
                        principalTable: "McFragen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "McFragePruefung",
                columns: table => new
                {
                    McFragenId = table.Column<int>(type: "INTEGER", nullable: false),
                    PruefungenId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McFragePruefung", x => new { x.McFragenId, x.PruefungenId });
                    table.ForeignKey(
                        name: "FK_McFragePruefung_McFragen_McFragenId",
                        column: x => x.McFragenId,
                        principalTable: "McFragen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_McFragePruefung_Pruefungen_PruefungenId",
                        column: x => x.PruefungenId,
                        principalTable: "Pruefungen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Kapitel_LehrveranstaltungId",
                table: "Kapitel",
                column: "LehrveranstaltungId");

            migrationBuilder.CreateIndex(
                name: "IX_McAntworten_McFrageId",
                table: "McAntworten",
                column: "McFrageId");

            migrationBuilder.CreateIndex(
                name: "IX_McFragen_KapitelId",
                table: "McFragen",
                column: "KapitelId");

            migrationBuilder.CreateIndex(
                name: "IX_McFragePruefung_PruefungenId",
                table: "McFragePruefung",
                column: "PruefungenId");

            migrationBuilder.CreateIndex(
                name: "IX_Pruefungen_LehrveranstaltungId",
                table: "Pruefungen",
                column: "LehrveranstaltungId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "McAntworten");

            migrationBuilder.DropTable(
                name: "McFragePruefung");

            migrationBuilder.DropTable(
                name: "McFragen");

            migrationBuilder.DropTable(
                name: "Pruefungen");

            migrationBuilder.DropTable(
                name: "Kapitel");

            migrationBuilder.DropTable(
                name: "Lehrveranstaltungen");
        }
    }
}
