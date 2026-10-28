using System.Net;
using System.Net.Http.Json;
using Mfa.Crc.Simulateur;
using Mfa.Crc.Tests.Infrastructure;

namespace Mfa.Crc.Tests.Seance06;

/// <summary>
/// SÉANCE 6 — Le moniteur 3 et la sécurité : connexion du superviseur, pages protégées,
/// dossier du sociétaire, notes et journal des actions
///
///   dotnet test --filter Groupe=seance-06
/// </summary>
[Trait("Groupe", "seance-06")]
public class DossierSecuriteTest : TestCrc
{
    // ── Mission 6.1 🟢 — les superviseurs et la connexion ───────────

    [Fact]
    public void m1_le_mot_de_passe_n_est_jamais_stocke_en_clair()
    {
        DemarrerApplication();
        Assert.True(TableExiste("Superviseurs"), "La table Superviseurs n'existe pas.");
        var ligne = Sql("SELECT * FROM Superviseurs WHERE Login = 'cmorel'").Single();
        var valeurs = ligne.Values.Select(v => v?.ToString() ?? "").ToList();
        Assert.DoesNotContain(MotDePasse, valeurs);
        Assert.Contains(valeurs, v => v.StartsWith("AQAAAA"));   // format de PasswordHasher
    }

    [Fact]
    public async Task m1_la_connexion_reussie_redirige_vers_l_accueil()
    {
        DemarrerApplication();
        var r = await SeConnecter();
        Assert.Equal(HttpStatusCode.Redirect, r.StatusCode);
        Assert.Equal("/", r.Headers.Location!.ToString());
    }

    [Theory]
    [InlineData(Login, "mauvais-mot-de-passe")]
    [InlineData("inconnu", "Supervision2026!")]
    public async Task m1_l_echec_donne_le_meme_message_que_le_login_existe_ou_non(string login, string mdp)
    {
        DemarrerApplication();
        var r = await SeConnecter(login, mdp);
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Contains("Identifiant ou mot de passe incorrect.", await r.Content.ReadAsStringAsync());
    }

    // ── Mission 6.2 🟢 — protéger les moniteurs et l'API ───────────────

    [Theory]
    [InlineData("/")]
    [InlineData("/moniteur/1")]
    [InlineData("/moniteur/2")]
    [InlineData("/societaires")]
    public async Task m2_une_page_sans_connexion_renvoie_vers_le_formulaire(string url)
    {
        var r = await Get(url, connexionAuto: false);
        Assert.Equal(HttpStatusCode.Redirect, r.StatusCode);
        Assert.Contains("/connexion", r.Headers.Location!.OriginalString, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task m2_l_api_sans_connexion_repond_401()
    {
        Rejouer(Arrive("K0001", "Sinistres", Priorite.Normale, H(9, 0)));
        var r = await Post("/api/contacts/K0001/affecter", new { conseillerId = "C03" }, connexionAuto: false);
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
        var s = await Get("/api/selection", connexionAuto: false);
        Assert.Equal(HttpStatusCode.Unauthorized, s.StatusCode);
    }

    [Fact]
    public async Task m2_la_page_de_connexion_et_la_feuille_de_style_restent_accessibles()
    {
        Assert.Equal(HttpStatusCode.OK, (await Get("/connexion", connexionAuto: false)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await Get("/css/site.css", connexionAuto: false)).StatusCode);
    }

    // ── Mission 6.3 🟡 — le moniteur 3 : le dossier en cours ───────────

    [Fact]
    public async Task m3_le_moniteur_3_affiche_le_dossier_du_conseiller_selectionne()
    {
        Rejouer(
            Statut("C01", StatutConseiller.Disponible, H(11, 0)),
            Arrive("K9001", "Sinistres", Priorite.Escalade, H(11, 29), societaire: "S001"),
            Decroche("K9001", "C01", H(11, 29, 22)));
        await Post("/api/selection", new { conseillerId = "C01" });

        var page = await Page("/moniteur/3");
        var texte = page.Body!.TextContent;
        Assert.Contains("ROCHE", texte);
        Assert.Contains("Hélène", texte);
        Assert.NotNull(page.QuerySelector("[data-contrat='AUTO-FICT-0001']"));
        Assert.NotNull(page.QuerySelector("[data-sinistre='SIN-FICT-0187']"));
        Assert.NotNull(page.QuerySelector("[data-reclamation='REC-FICT-0042']"));
    }

    [Fact]
    public async Task m3_on_peut_ouvrir_un_dossier_directement()
    {
        var page = await Page("/moniteur/3?societaire=S009");
        Assert.Contains("VERNET", page.Body!.TextContent);
        Assert.NotNull(page.QuerySelector("[data-reclamation='REC-FICT-0039']"));
    }

    [Fact]
    public async Task m3_sans_selection_le_moniteur_3_l_indique()
    {
        var page = await Page("/moniteur/3");
        Assert.Contains("Aucun dossier", page.Body!.TextContent);
    }

    // ── Mission 6.4 🟡 — notes et journal des actions ──────────────────

    [Fact]
    public async Task m4_une_note_est_enregistree_avec_son_auteur_et_journalisee()
    {
        var r = await Post("/api/notes", new { societaireId = "S001", texte = "Rappel sous 48 h promis." });
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);

        var note = Sql("SELECT n.Texte, s.Login FROM Notes n JOIN Superviseurs s ON s.Id = n.SuperviseurId").Single();
        Assert.Equal("Rappel sous 48 h promis.", note["Texte"]);
        Assert.Equal("cmorel", note["Login"]);

        var journal = Sql("SELECT Action, Cible FROM JournalActions").Single();
        Assert.Equal("Note", journal["Action"]);
        Assert.Equal("S001", journal["Cible"]);
    }

    [Fact]
    public async Task m4_une_note_vide_est_refusee()
    {
        var r = await Post("/api/notes", new { societaireId = "S001", texte = "  " });
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task m4_les_notes_s_affichent_sur_le_dossier()
    {
        await Post("/api/notes", new { societaireId = "S009", texte = "Courrier de relance envoyé." });
        var page = await Page("/moniteur/3?societaire=S009");
        Assert.Contains("Courrier de relance envoyé.", page.Body!.TextContent);
    }

    [Fact]
    public async Task m4_une_affectation_est_journalisee_aussi()
    {
        Rejouer(
            Statut("C03", StatutConseiller.Disponible, H(8, 30)),
            Arrive("K0001", "Sinistres", Priorite.Normale, H(9, 0)));
        await Post("/api/contacts/K0001/affecter", new { conseillerId = "C03" });

        var journal = Sql("SELECT Action, Cible FROM JournalActions").Single();
        Assert.Equal("Affectation", journal["Action"]);
        Assert.Equal("K0001", journal["Cible"]);
    }
}
