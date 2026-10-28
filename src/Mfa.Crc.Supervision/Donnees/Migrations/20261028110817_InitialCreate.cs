using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mfa.Crc.Supervision.Donnees.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Conseillers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Prenom = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Teletravail = table.Column<bool>(type: "INTEGER", nullable: false),
                    Niveau = table.Column<int>(type: "INTEGER", nullable: false),
                    Competences = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conseillers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Libelle = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Societaires",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 4, nullable: false),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Prenom = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Ville = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    SocietaireDepuis = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Societaires", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contrats",
                columns: table => new
                {
                    Numero = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SocietaireId = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Formule = table.Column<string>(type: "TEXT", nullable: false),
                    Depuis = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contrats", x => x.Numero);
                    table.ForeignKey(
                        name: "FK_Contrats_Societaires_SocietaireId",
                        column: x => x.SocietaireId,
                        principalTable: "Societaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reclamations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SocietaireId = table.Column<string>(type: "TEXT", nullable: false),
                    SinistreId = table.Column<string>(type: "TEXT", nullable: true),
                    Niveau = table.Column<int>(type: "INTEGER", nullable: false),
                    Objet = table.Column<string>(type: "TEXT", nullable: false),
                    RecueLe = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    AccuseReceptionLe = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    ReponseLe = table.Column<DateOnly>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reclamations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reclamations_Societaires_SocietaireId",
                        column: x => x.SocietaireId,
                        principalTable: "Societaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sinistres",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ContratNumero = table.Column<string>(type: "TEXT", nullable: false),
                    Nature = table.Column<string>(type: "TEXT", nullable: false),
                    DeclareLe = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Statut = table.Column<string>(type: "TEXT", nullable: false),
                    ResponsabiliteRetenue = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sinistres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sinistres_Contrats_ContratNumero",
                        column: x => x.ContratNumero,
                        principalTable: "Contrats",
                        principalColumn: "Numero",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contrats_SocietaireId",
                table: "Contrats",
                column: "SocietaireId");

            migrationBuilder.CreateIndex(
                name: "IX_Reclamations_SocietaireId",
                table: "Reclamations",
                column: "SocietaireId");

            migrationBuilder.CreateIndex(
                name: "IX_Sinistres_ContratNumero",
                table: "Sinistres",
                column: "ContratNumero");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Conseillers");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "Reclamations");

            migrationBuilder.DropTable(
                name: "Sinistres");

            migrationBuilder.DropTable(
                name: "Contrats");

            migrationBuilder.DropTable(
                name: "Societaires");
        }
    }
}
