using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Mfa.Crc.Simulateur;
using Mfa.Crc.Supervision.Regles;
using Mfa.Crc.Supervision.TempsReel;
using Mfa.Crc.Tests.Infrastructure;

namespace Mfa.Crc.Tests.Seance04;

/// <summary>
/// SÉANCE 4 — Affecter un contact à un conseiller : la règle RG2 et une API
///
///   dotnet test --filter Groupe=seance-04
/// </summary>
[Trait("Groupe", "seance-04")]
public class AffectationTest : TestCrc
{
    private static readonly ContactEnAttente Sinistre =
        new("K0001", "Sinistres", Canal.Telephone, Priorite.Normale, "S002", "Bris de glace", new TimeSpan(9, 0, 0));

    private static EtatConseiller E(string id, StatutConseiller s, TimeSpan depuis, params string[] files) =>
        new(id, id, files.ToHashSet(), s, depuis);

    // ── Mission 4.1 🟢 — RG2 : qui peut prendre ce contact ? ──────────

    [Fact]
    public void m1_seul_un_conseiller_disponible_et_competent_est_propose()
    {
        var plateau = new[]
        {
            E("C04", StatutConseiller.Disponible, H(8, 0), "Contrats", "Sante"),
            E("C01", StatutConseiller.Pause, H(8, 30), "Sinistres"),
            E("C03", StatutConseiller.Disponible, H(8, 55), "Sinistres"),
        };
        Assert.Equal("C03", ServiceAffectation.ConseillerPropose(Sinistre, plateau)!.Id);
    }

    [Fact]
    public void m1_le_plus_ancien_disponible_passe_devant()
    {
        var plateau = new[]
        {
            E("C03", StatutConseiller.Disponible, H(8, 55), "Sinistres"),
            E("C08", StatutConseiller.Disponible, H(8, 50), "Sinistres", "Contrats"),
        };
        Assert.Equal("C08", ServiceAffectation.ConseillerPropose(Sinistre, plateau)!.Id);
    }

    [Fact]
    public void m1_personne_ne_convient()
    {
        var plateau = new[] { E("C04", StatutConseiller.Disponible, H(8, 0), "Contrats") };
        Assert.Null(ServiceAffectation.ConseillerPropose(Sinistre, plateau));
    }

    [Fact]
    public async Task m1_l_api_donne_le_conseiller_propose()
    {
        Rejouer(
            Statut("C03", StatutConseiller.Disponible, H(8, 30)),
            Statut("C04", StatutConseiller.Disponible, H(8, 0)),
            Arrive("K0001", "Sinistres", Priorite.Normale, H(9, 0)));

        var json = await Json<JsonElement>("/api/contacts/K0001/proposition");
        Assert.Equal("C03", json.GetProperty("conseillerId").GetString());
    }

    // ── Mission 4.2 🟢 — affecter ─────────────────────────────────────

    [Fact]
    public async Task m2_affecter_retire_le_contact_de_la_file_et_met_le_conseiller_en_appel()
    {
        Rejouer(
            Statut("C03", StatutConseiller.Disponible, H(8, 30)),
            Arrive("K0001", "Sinistres", Priorite.Normale, H(9, 0)),
            Statut("C04", StatutConseiller.Disponible, H(9, 1)));

        var r = await Post("/api/contacts/K0001/affecter", new { conseillerId = "C03" });

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var json = await r.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("K0001", json.GetProperty("contactId").GetString());
        Assert.Equal("C03", json.GetProperty("conseillerId").GetString());

        Assert.DoesNotContain(Etat.FileAttente, c => c.Id == "K0001");
        Assert.Equal(StatutConseiller.EnAppel, Etat.Conseiller("C03")!.Statut);
        Assert.Equal(H(9, 1), Etat.Conseiller("C03")!.Depuis);
        var trace = Etat.Contacts.Single(c => c.Id == "K0001");
        Assert.Equal("C03", trace.ConseillerId);
        Assert.Equal(H(9, 1), trace.Decroche);
    }

    [Fact]
    public async Task m2_le_moniteur_1_propose_un_bouton_affecter_par_contact()
    {
        Rejouer(Arrive("K0001", "Sinistres", Priorite.Normale, H(9, 0)));
        var page = await Page("/moniteur/1");
        Assert.NotNull(page.QuerySelector("[data-contact='K0001'] button[data-affecter='K0001']"));
    }

    // ── Mission 4.3 🟡 — les refus ─────────────────────────────────────

    [Fact]
    public async Task m3_contact_inconnu_ou_deja_pris_404()
    {
        Rejouer(Statut("C03", StatutConseiller.Disponible, H(8, 30)));
        var r = await Post("/api/contacts/K9999/affecter", new { conseillerId = "C03" });
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
        Assert.Equal("application/problem+json", r.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task m3_conseiller_sans_la_competence_409_avec_un_message()
    {
        Rejouer(
            Statut("C04", StatutConseiller.Disponible, H(8, 30)),
            Arrive("K0001", "Sinistres", Priorite.Normale, H(9, 0)));

        var r = await Post("/api/contacts/K0001/affecter", new { conseillerId = "C04" });

        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
        var json = await r.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("detail").GetString()));
        Assert.Contains(Etat.FileAttente, c => c.Id == "K0001");
        Assert.Equal(StatutConseiller.Disponible, Etat.Conseiller("C04")!.Statut);
    }

    [Fact]
    public async Task m3_conseiller_non_disponible_409()
    {
        Rejouer(
            Statut("C03", StatutConseiller.Pause, H(8, 30)),
            Arrive("K0001", "Sinistres", Priorite.Normale, H(9, 0)));

        var r = await Post("/api/contacts/K0001/affecter", new { conseillerId = "C03" });
        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
        Assert.Contains(Etat.FileAttente, c => c.Id == "K0001");
    }

    [Fact]
    public async Task m3_conseiller_inconnu_404()
    {
        Rejouer(Arrive("K0001", "Sinistres", Priorite.Normale, H(9, 0)));
        var r = await Post("/api/contacts/K0001/affecter", new { conseillerId = "C99" });
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
        Assert.Equal("application/problem+json", r.Content.Headers.ContentType?.MediaType);
    }

    // ── Mission 4.4 🟡 — deux superviseurs, un seul contact ──────────

    [Fact]
    public async Task m4_deux_affectations_simultanees_du_meme_contact_une_seule_reussit()
    {
        await AssurerConnexion();
        var conseillers = new[] { "C01", "C02", "C03", "C08", "C12", "C14" };
        Rejouer([.. conseillers.Select(c => Statut(c, StatutConseiller.Disponible, H(8, 30))),
                 Arrive("K0001", "Sinistres", Priorite.Normale, H(9, 0))]);

        var reponses = await Task.WhenAll(conseillers.Select(c => Post("/api/contacts/K0001/affecter", new { conseillerId = c })));

        Assert.Single(reponses, r => r.StatusCode == HttpStatusCode.OK);
        Assert.All(reponses.Where(r => r.StatusCode != HttpStatusCode.OK), r => Assert.True(
            r.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Conflict, $"{(int)r.StatusCode}"));
        Assert.Single(Etat.Plateau, c => c.Statut == StatutConseiller.EnAppel);
    }

    [Fact]
    public async Task m4_un_conseiller_ne_peut_pas_recevoir_deux_contacts_en_meme_temps()
    {
        await AssurerConnexion();
        Rejouer(
            Statut("C03", StatutConseiller.Disponible, H(8, 30)),
            Arrive("K0001", "Sinistres", Priorite.Normale, H(9, 0)),
            Arrive("K0002", "Sinistres", Priorite.Normale, H(9, 0)));

        var reponses = await Task.WhenAll(
            Post("/api/contacts/K0001/affecter", new { conseillerId = "C03" }),
            Post("/api/contacts/K0002/affecter", new { conseillerId = "C03" }));

        Assert.Single(reponses, r => r.StatusCode == HttpStatusCode.OK);
        Assert.Single(Etat.FileAttente);
    }
}
