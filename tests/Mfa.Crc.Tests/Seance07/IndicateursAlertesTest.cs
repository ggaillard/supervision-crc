using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Mfa.Crc.Simulateur;
using Mfa.Crc.Supervision.Regles;
using Mfa.Crc.Supervision.TempsReel;
using Mfa.Crc.Tests.Infrastructure;

namespace Mfa.Crc.Tests.Seance07;

/// <summary>
/// SÉANCE 7 — Le moniteur 4 : indicateurs (RG3), échéances des réclamations (RG5),
/// alertes acquittées mais jamais effacées (RG6)
///
///   dotnet test --filter Groupe=seance-07
/// </summary>
[Trait("Groupe", "seance-07")]
public class IndicateursAlertesTest : TestCrc
{
    private static ContactTrace T(string id, int arriveeS, int? decrocheS, int? closS = null, bool abandon = false,
        int? satisfaction = null, bool? resolu = null, string file = "Contrats") =>
        new(id, file, "S002", TimeSpan.FromSeconds(36000 + arriveeS),
            decrocheS is null ? null : TimeSpan.FromSeconds(36000 + decrocheS.Value), decrocheS is null ? null : "C04",
            closS is null ? null : TimeSpan.FromSeconds(36000 + closS.Value), abandon, satisfaction, resolu);

    // ── Mission 7.1 🟢 — les indicateurs de la journée ─────────────

    [Fact]
    public void m1_les_quatre_indicateurs_sur_un_petit_jeu()
    {
        var contacts = new[]
        {
            T("A", 0, 10, 310, satisfaction: 5, resolu: true),
            T("B", 0, 30, 530, satisfaction: 3, resolu: false),
            T("C", 0, null, abandon: true),
            T("D", 0, 5, 405, satisfaction: 4, resolu: true),
            T("E", 0, null)                        // encore en attente : ne compte pas
        };
        Assert.Equal(75.0, Indicateurs.TauxDecroche(contacts));
        Assert.Equal(TimeSpan.FromSeconds(400), Indicateurs.DureeMoyenneTraitement(contacts));
        Assert.Equal(200.0 / 3, Indicateurs.ResolutionPremierContact(contacts), 6);
        Assert.Equal(4.0, Indicateurs.Satisfaction(contacts));
    }

    [Fact]
    public void m1_aucun_contact_ne_fait_pas_planter()
    {
        Assert.Equal(0, Indicateurs.TauxDecroche([]));
        Assert.Equal(TimeSpan.Zero, Indicateurs.DureeMoyenneTraitement([]));
    }

    [Fact]
    public async Task m1_l_api_des_indicateurs_en_fin_de_matinee()
    {
        RejouerScenarioJusqua(H(12, 0));
        var json = await Json<JsonElement>("/api/indicateurs");
        Assert.Equal(95.8, Math.Round(json.GetProperty("tauxDecroche").GetDouble(), 1));
        Assert.Equal(72.7, Math.Round(json.GetProperty("resolutionPremierContact").GetDouble(), 1));
        Assert.Equal(4.00, Math.Round(json.GetProperty("satisfaction").GetDouble(), 2));
        Assert.InRange(json.GetProperty("dureeMoyenneSecondes").GetDouble(), 395, 405);
    }

    // ── Mission 7.2 🟡 — RG3 : le niveau de service ────────────────────

    [Fact]
    public void m2_niveau_de_service_decroches_en_moins_de_20_s()
    {
        var contacts = new[] { T("A", 0, 10), T("B", 60, 85), T("C", 120, null, abandon: true), T("D", 180, 185) };
        var ns = Indicateurs.NiveauService(contacts, H(10, 10), TimeSpan.FromMinutes(30), TimeSpan.FromSeconds(20));
        Assert.Equal(50.0, ns);
    }

    [Fact]
    public void m2_un_contact_arrive_il_y_a_moins_de_20_s_n_est_pas_encore_compte()
    {
        Assert.Null(Indicateurs.NiveauService([T("A", 590, null)], H(10, 10), TimeSpan.FromMinutes(30), TimeSpan.FromSeconds(20)));
    }

    [Fact]
    public void m2_le_filtre_par_file()
    {
        var contacts = new[] { T("A", 0, 10, file: "Sinistres"), T("B", 0, 60, file: "Contrats") };
        Assert.Equal(100.0, Indicateurs.NiveauService(contacts, H(10, 10), TimeSpan.FromMinutes(30), TimeSpan.FromSeconds(20), "Sinistres"));
        Assert.Equal(0.0, Indicateurs.NiveauService(contacts, H(10, 10), TimeSpan.FromMinutes(30), TimeSpan.FromSeconds(20), "Contrats"));
    }

    [Fact]
    public async Task m2_a_11h42_trois_files_sont_en_alerte_rg3()
    {
        RejouerScenarioJusqua(H(11, 42));
        var json = await Json<JsonElement>("/api/indicateurs");
        var ns = json.GetProperty("niveauxService").EnumerateArray()
            .ToDictionary(e => e.GetProperty("file").GetString()!, e => e);
        Assert.Equal(52.2, Math.Round(ns["Sinistres"].GetProperty("valeur").GetDouble(), 1));
        Assert.False(ns["Sinistres"].GetProperty("alerte").GetBoolean());
        Assert.Equal(76.5, Math.Round(ns["Contrats"].GetProperty("valeur").GetDouble(), 1));
        Assert.True(ns["Contrats"].GetProperty("alerte").GetBoolean());
        Assert.True(ns["Sante"].GetProperty("alerte").GetBoolean());
        Assert.True(ns["Accueil"].GetProperty("alerte").GetBoolean());
    }

    // ── Mission 7.3 🟡 — RG5 : les échéances des réclamations ──────────

    [Fact]
    public void m3_dix_jours_ouvrables_pour_l_accuse_deux_mois_pour_la_reponse()
    {
        var e = Echeances.Calculer(new DateOnly(2026, 10, 20));
        Assert.Equal(new DateOnly(2026, 11, 3), e.AccuseReceptionAvant);
        Assert.Equal(new DateOnly(2026, 12, 20), e.ReponseAvant);
    }

    [Fact]
    public void m3_le_11_novembre_ne_compte_pas()
    {
        Assert.Equal(new DateOnly(2026, 11, 20), Echeances.Calculer(new DateOnly(2026, 11, 5)).AccuseReceptionAvant);
    }

    [Fact]
    public void m3_alerte_a_j_moins_5()
    {
        var recue = new DateOnly(2026, 9, 2);       // réponse due le 2 novembre
        Assert.True(Echeances.EnAlerte(recue, new DateOnly(2026, 9, 8), null, new DateOnly(2026, 10, 28), 5));
        Assert.False(Echeances.EnAlerte(recue, new DateOnly(2026, 9, 8), null, new DateOnly(2026, 10, 27), 5));
        Assert.False(Echeances.EnAlerte(recue, new DateOnly(2026, 9, 8), new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 30), 5));
    }

    [Fact]
    public async Task m3_l_api_des_echeances_au_28_octobre()
    {
        var json = await Json<JsonElement>("/api/reclamations/echeances?date=2026-10-28");
        var parId = json.EnumerateArray().ToDictionary(e => e.GetProperty("reclamationId").GetString()!, e => e);
        Assert.Equal("2026-11-03", parId["REC-FICT-0042"].GetProperty("accuseReceptionAvant").GetString());
        Assert.False(parId["REC-FICT-0042"].GetProperty("alerte").GetBoolean());
        Assert.True(parId["REC-FICT-0039"].GetProperty("alerte").GetBoolean());
    }

    // ── Mission 7.4 🟢 — RG6 : des alertes qu'on acquitte sans les effacer ─

    [Fact]
    public async Task m4_generer_cree_les_alertes_du_moment_sans_doublon()
    {
        Rejouer(Statut("C01", StatutConseiller.EnAppel, H(11, 29)), Statut("C02", StatutConseiller.Disponible, H(11, 42)));

        var r = await Post("/api/alertes/generer");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var alertes = await Json<JsonElement>("/api/alertes");
        Assert.Contains(alertes.EnumerateArray(), a => a.GetProperty("type").GetString() == "AppelLong" && a.GetProperty("cible").GetString() == "C01");
        Assert.Contains(alertes.EnumerateArray(), a => a.GetProperty("type").GetString() == "EcheanceReclamation" && a.GetProperty("cible").GetString() == "REC-FICT-0039");
        var avant = alertes.GetArrayLength();

        await Post("/api/alertes/generer");
        Assert.Equal(avant, (await Json<JsonElement>("/api/alertes")).GetArrayLength());
    }

    [Fact]
    public async Task m4_acquitter_garde_l_alerte_avec_l_heure_et_l_auteur()
    {
        Rejouer(Statut("C01", StatutConseiller.EnAppel, H(11, 29)), Statut("C02", StatutConseiller.Disponible, H(11, 42)));
        await Post("/api/alertes/generer");
        var id = (await Json<JsonElement>("/api/alertes")).EnumerateArray()
            .First(a => a.GetProperty("cible").GetString() == "C01").GetProperty("id").GetInt32();

        var r = await Post($"/api/alertes/{id}/acquitter");
        Assert.Equal(HttpStatusCode.NoContent, r.StatusCode);

        var ligne = Sql($"SELECT a.AcquitteeA, s.Login FROM Alertes a JOIN Superviseurs s ON s.Id = a.AcquitteeParId WHERE a.Id = {id}").Single();
        Assert.NotNull(ligne["AcquitteeA"]);
        Assert.Equal("cmorel", ligne["Login"]);
        Assert.Contains(Sql("SELECT Action, Cible FROM JournalActions"), l => (string)l["Action"]! == "Acquittement" && (string)l["Cible"]! == id.ToString());

        Assert.Equal(HttpStatusCode.Conflict, (await Post($"/api/alertes/{id}/acquitter")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Post("/api/alertes/99999/acquitter")).StatusCode);
    }

    [Fact]
    public async Task m4_une_alerte_ne_se_supprime_pas()
    {
        Rejouer(Statut("C01", StatutConseiller.EnAppel, H(11, 29)), Statut("C02", StatutConseiller.Disponible, H(11, 42)));
        await Post("/api/alertes/generer");
        var n = Sql("SELECT COUNT(*) AS n FROM Alertes")[0]["n"];
        var id = Sql("SELECT Id FROM Alertes LIMIT 1")[0]["Id"];

        var r = await Envoyer(HttpMethod.Delete, $"/api/alertes/{id}");
        Assert.False(r.IsSuccessStatusCode);
        Assert.Equal(n, Sql("SELECT COUNT(*) AS n FROM Alertes")[0]["n"]);
    }

    [Fact]
    public async Task m4_le_moniteur_4_affiche_indicateurs_et_alertes()
    {
        Rejouer(Statut("C01", StatutConseiller.EnAppel, H(11, 29)), Statut("C02", StatutConseiller.Disponible, H(11, 42)));
        await Post("/api/alertes/generer");

        var page = await Page("/moniteur/4");
        Assert.NotNull(page.QuerySelector("[data-indicateur='tauxDecroche']"));
        Assert.NotNull(page.QuerySelector("[data-indicateur='satisfaction']"));
        Assert.NotEmpty(page.QuerySelectorAll("[data-alerte-id]"));
        Assert.NotNull(page.QuerySelector("button[data-acquitter]"));
    }
}
