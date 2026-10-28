using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Mfa.Crc.Simulateur;
using Mfa.Crc.Tests.Infrastructure;

namespace Mfa.Crc.Tests.Bonus;

/// <summary>
/// BONUS — les missions 🔴, facultatives.
///
///   dotnet test --filter Groupe=bonus
/// </summary>
[Trait("Groupe", "bonus")]
public class BonusTest : TestCrc
{
    [Fact]
    public async Task s1_m5_le_moniteur_1_filtre_par_canal()
    {
        Rejouer(
            Arrive("K0001", "Contrats", Priorite.Normale, H(9, 0), Canal.Chat),
            Arrive("K0002", "Contrats", Priorite.Normale, H(9, 1), Canal.Telephone),
            Arrive("K0003", "Sante", Priorite.Normale, H(9, 2), Canal.Chat));

        var page = await Page("/moniteur/1?canal=Chat");
        Assert.Equal(["K0001", "K0003"], page.QuerySelectorAll("[data-contact]").Select(e => e.GetAttribute("data-contact")));
    }

    [Fact]
    public async Task s2_m5_la_page_des_conseillers_d_une_file()
    {
        var page = await Page("/conseillers?file=Sante");
        var ids = page.QuerySelectorAll("[data-conseiller]").Select(e => e.GetAttribute("data-conseiller")).ToHashSet();
        Assert.Equal(new HashSet<string?> { "C04", "C06", "C07", "C11", "C15", "C17" }, ids);
    }

    [Fact]
    public async Task s3_m5_le_plateau_se_trie_par_duree()
    {
        Rejouer(
            Statut("C01", StatutConseiller.EnAppel, H(9, 0)),
            Statut("C02", StatutConseiller.EnAppel, H(8, 50)),
            Statut("C03", StatutConseiller.Pause, H(9, 5)));

        var page = await Page("/moniteur/2?tri=duree");
        var ordre = page.QuerySelectorAll("[data-conseiller]").Select(e => e.GetAttribute("data-conseiller")).Take(3);
        Assert.Equal(["C02", "C01", "C03"], ordre);
    }

    [Fact]
    public async Task s4_m5_sans_conseiller_indique_on_affecte_au_conseiller_propose()
    {
        Rejouer(
            Statut("C02", StatutConseiller.Disponible, H(8, 40)),
            Statut("C03", StatutConseiller.Disponible, H(8, 30)),
            Arrive("K0001", "Sinistres", Priorite.Normale, H(9, 0)));

        var r = await Post("/api/contacts/K0001/affecter", new { });
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal(StatutConseiller.EnAppel, Etat.Conseiller("C03")!.Statut);
    }

    [Fact]
    public async Task s5_m5_le_lanceur_de_poste_place_les_fenetres_sur_les_ecrans()
    {
        var r = await Get("/poste");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var html = await r.Content.ReadAsStringAsync();
        Assert.Contains("getScreenDetails", html);
        Assert.Contains("window.open", html);
    }

    [Fact]
    public async Task s6_m5_cinq_echecs_bloquent_la_connexion()
    {
        DemarrerApplication();
        for (var i = 0; i < 5; i++) await SeConnecter(Login, "mauvais");
        var r = await SeConnecter();
        var html = await r.Content.ReadAsStringAsync();
        Assert.Contains("Trop de tentatives", html);
        var protegee = await Get("/moniteur/1", connexionAuto: false);
        Assert.NotEqual(HttpStatusCode.OK, protegee.StatusCode);
    }

    [Fact]
    public async Task s7_m5_les_indicateurs_d_une_seule_file()
    {
        Rejouer(
            Statut("C02", StatutConseiller.Disponible, H(8, 40)),
            Arrive("K0001", "Sinistres", Priorite.Normale, H(9, 0)),
            Decroche("K0001", "C02", H(9, 0, 5)),
            Arrive("K0002", "Contrats", Priorite.Normale, H(9, 0)),
            new ContactAbandonne(H(9, 3), "K0002"));

        var json = await Json<JsonElement>("/api/indicateurs?file=Sinistres");
        Assert.Equal(100.0, json.GetProperty("tauxDecroche").GetDouble());
    }
}
