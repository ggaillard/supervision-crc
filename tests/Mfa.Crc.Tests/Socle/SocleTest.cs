using System.Net;
using System.Text.Json;
using Mfa.Crc.Simulateur;
using Mfa.Crc.Tests.Infrastructure;

namespace Mfa.Crc.Tests.Socle;

/// <summary>
/// SOCLE — ce qui fonctionne déjà le premier jour. Ces tests doivent TOUJOURS rester verts :
/// s'ils passent au rouge, une modification a cassé l'existant.
///
///   dotnet test --filter Groupe=socle
/// </summary>
[Trait("Groupe", "socle")]
public class SocleTest : TestCrc
{
    [Fact]
    public async Task l_accueil_repond()
    {
        var page = await Page("/");
        Assert.Contains("supervision", page.Body!.TextContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task la_base_contient_les_conseillers_et_les_societaires_du_dossier_data()
    {
        DemarrerApplication();
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(DossierData, "conseillers.json")));
        Assert.Equal(json.RootElement.GetProperty("conseillers").GetArrayLength(),
            Convert.ToInt32(Sql("SELECT COUNT(*) AS n FROM Conseillers")[0]["n"]));
        Assert.Equal(24, Convert.ToInt32(Sql("SELECT COUNT(*) AS n FROM Societaires")[0]["n"]));
        Assert.Equal(4, Convert.ToInt32(Sql("SELECT COUNT(*) AS n FROM Files")[0]["n"]));
    }

    [Fact]
    public void le_plateau_contient_les_18_conseillers_hors_ligne_au_demarrage()
    {
        Assert.Equal(18, Etat.Plateau.Count);
        Assert.All(Etat.Plateau, c => Assert.Equal(StatutConseiller.HorsLigne, c.Statut));
        Assert.Contains("Sinistres", Etat.Conseiller("C01")!.Competences);
    }

    [Fact]
    public async Task le_moniteur_1_affiche_les_contacts_en_attente()
    {
        Rejouer(
            Arrive("K0001", "Contrats", Priorite.Normale, H(9, 0)),
            Arrive("K0002", "Sinistres", Priorite.Haute, H(9, 1)));

        var page = await Page("/moniteur/1");
        var lignes = page.QuerySelectorAll("[data-contact]").Select(e => e.GetAttribute("data-contact")).ToList();
        Assert.Equal(2, lignes.Count);
        Assert.Contains("K0001", lignes);
        Assert.Contains("K0002", lignes);
    }

    [Fact]
    public async Task un_contact_decroche_quitte_la_file()
    {
        Rejouer(
            Statut("C02", StatutConseiller.Disponible, H(8, 30)),
            Arrive("K0001", "Sinistres", Priorite.Normale, H(9, 0)),
            Decroche("K0001", "C02", H(9, 0, 10)));

        Assert.Empty(Etat.FileAttente);
        var page = await Page("/moniteur/1");
        Assert.Empty(page.QuerySelectorAll("[data-contact]"));
    }

    [Fact]
    public async Task la_recherche_de_societaires_trouve_par_nom()
    {
        var page = await Page("/societaires?nom=roche");
        var trouves = page.QuerySelectorAll("[data-societaire]").Select(e => e.GetAttribute("data-societaire")).ToList();
        Assert.Equal(["S001"], trouves);
    }

    [Fact]
    public void le_scenario_de_la_matinee_se_rejoue_sans_erreur()
    {
        RejouerScenarioJusqua(H(12, 0));
        Assert.Equal(313, Etat.Contacts.Count);
        Assert.Equal(H(12, 0), Etat.Heure);
    }

    [Fact]
    public async Task une_page_inconnue_renvoie_404()
    {
        var r = await Get("/page-qui-n-existe-pas");
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
    }
}
