using System.Text.Json;
using Mfa.Crc.Simulateur;
using Mfa.Crc.Supervision.Donnees;
using Mfa.Crc.Tests.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Mfa.Crc.Tests.Seance02;

/// <summary>
/// SÉANCE 2 — Le modèle de données : les compétences deviennent une association,
/// les statuts deviennent un historique
///
///   dotnet test --filter Groupe=seance-02
/// </summary>
[Trait("Groupe", "seance-02")]
public class CompetencesTest : TestCrc
{
    // ── Mission 2.1 🟢 — la table Competences ─────────────────────────

    [Fact]
    public void m1_la_table_competences_relie_conseiller_et_file_et_porte_un_niveau()
    {
        DemarrerApplication();
        Assert.True(TableExiste("Competences"), "La table Competences n'existe pas.");
        Assert.Superset(new HashSet<string> { "ConseillerId", "FileCode", "Niveau" }, Colonnes("Competences"));
    }

    [Fact]
    public void m1_la_cle_primaire_est_le_couple_conseiller_file()
    {
        DemarrerApplication();
        var cle = Sql("PRAGMA table_info('Competences')").Where(l => Convert.ToInt32(l["pk"]) > 0)
            .Select(l => (string)l["name"]!).ToHashSet();
        Assert.Equal(new HashSet<string> { "ConseillerId", "FileCode" }, cle);
    }

    [Fact]
    public void m1_deux_cles_etrangeres_vers_conseillers_et_files()
    {
        DemarrerApplication();
        var cibles = Sql("PRAGMA foreign_key_list('Competences')").Select(l => (string)l["table"]!).ToHashSet();
        Assert.Equal(new HashSet<string> { "Conseillers", "Files" }, cibles);
    }

    // ── Mission 2.2 🟢 — reprise des données existantes ────────────────

    [Fact]
    public void m2_la_migration_transfere_les_competences_deja_saisies_puis_supprime_l_ancienne_colonne()
    {
        var fichier = Path.Combine(Path.GetTempPath(), $"crc-reprise-{Guid.NewGuid():N}.db");
        try
        {
            using (var db = new CrcContext(new DbContextOptionsBuilder<CrcContext>().UseSqlite($"Data Source={fichier}").Options))
            {
                var migrateur = db.GetService<IMigrator>();
                migrateur.Migrate("InitialCreate");          // la base telle qu'elle est en production
                db.Database.ExecuteSqlRaw("""
                    INSERT INTO Files (Code, Libelle) VALUES ('Sinistres','Sinistres'), ('Contrats','Contrats'), ('Sante','Santé'), ('Accueil','Accueil');
                    INSERT INTO Conseillers (Id, Nom, Prenom, Teletravail, Niveau, Competences) VALUES
                      ('C01','A','A',0,2,'Sinistres'),
                      ('C02','B','B',0,1,'Sinistres;Contrats'),
                      ('C03','C','C',0,1,'Sante;Accueil');
                    """);
                migrateur.Migrate();                          // vos migrations
            }

            using var cnx = new SqliteConnection($"Data Source={fichier}");
            cnx.Open();
            var cmd = cnx.CreateCommand();
            cmd.CommandText = "SELECT ConseillerId || ':' || FileCode || ':' || Niveau FROM Competences ORDER BY 1";
            var lignes = new List<string>();
            using (var l = cmd.ExecuteReader()) while (l.Read()) lignes.Add(l.GetString(0));
            Assert.Equal(["C01:Sinistres:2", "C02:Contrats:1", "C02:Sinistres:1", "C03:Accueil:1", "C03:Sante:1"], lignes);

            cmd.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Conseillers') WHERE name = 'Competences'";
            Assert.Equal(0L, (long)cmd.ExecuteScalar()!);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(fichier);
        }
    }

    [Fact]
    public void m2_une_base_neuve_est_chargee_avec_les_competences_du_fichier()
    {
        DemarrerApplication();
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(DossierData, "conseillers.json")));
        var attendu = json.RootElement.GetProperty("conseillers").EnumerateArray()
            .Sum(c => c.GetProperty("competences").GetArrayLength());
        Assert.Equal(attendu, Convert.ToInt32(Sql("SELECT COUNT(*) AS n FROM Competences")[0]["n"]));

        var c08 = Sql("SELECT FileCode FROM Competences WHERE ConseillerId = 'C08' ORDER BY FileCode").Select(l => (string)l["FileCode"]!);
        Assert.Equal(["Contrats", "Sinistres"], c08);
    }

    // ── Mission 2.3 🟡 — l'historique des statuts ──────────────────────

    [Fact]
    public void m3_la_table_statuts_conseillers_existe()
    {
        DemarrerApplication();
        Assert.True(TableExiste("StatutsConseillers"), "La table StatutsConseillers n'existe pas.");
        Assert.Superset(new HashSet<string> { "Id", "ConseillerId", "Statut", "Debut" }, Colonnes("StatutsConseillers"));
    }

    [Fact]
    public void m3_chaque_changement_de_statut_est_enregistre()
    {
        Rejouer(
            Statut("C01", StatutConseiller.Disponible, H(8, 2)),
            Statut("C01", StatutConseiller.EnAppel, H(8, 10)),
            Statut("C01", StatutConseiller.Pause, H(9, 30)),
            Statut("C02", StatutConseiller.Disponible, H(8, 5)));

        var c01 = Sql("SELECT Statut FROM StatutsConseillers WHERE ConseillerId = 'C01' ORDER BY Id").Select(l => l["Statut"]?.ToString());
        Assert.Equal(["Disponible", "EnAppel", "Pause"], c01);
        Assert.Single(Sql("SELECT Id FROM StatutsConseillers WHERE ConseillerId = 'C02'"));
    }
}
