using Mfa.Crc.Simulateur;
using Mfa.Crc.Supervision.Regles;
using Mfa.Crc.Supervision.TempsReel;
using Mfa.Crc.Tests.Infrastructure;

namespace Mfa.Crc.Tests.Seance03;

/// <summary>
/// SÉANCE 3 — Le moniteur 2 : le plateau des conseillers, et les alertes de durée (RG4)
///
///   dotnet test --filter Groupe=seance-03
/// </summary>
[Trait("Groupe", "seance-03")]
public class PlateauTest : TestCrc
{
    private static EtatConseiller E(string id, StatutConseiller s, int h, int m) =>
        new(id, id, new HashSet<string> { "Sinistres" }, s, new TimeSpan(h, m, 0));

    private static readonly SeuilsAlerte Dix = new() { AppelLongMinutes = 10, PauseLongueMinutes = 10 };

    // ── Mission 3.1 🟢 — la page du plateau ─────────────────────────

    [Fact]
    public async Task m1_le_moniteur_2_affiche_les_18_conseillers_avec_leur_statut()
    {
        Rejouer(
            Statut("C01", StatutConseiller.EnAppel, H(9, 0)),
            Statut("C06", StatutConseiller.Pause, H(9, 2)));

        var page = await Page("/moniteur/2");
        var cartes = page.QuerySelectorAll("[data-conseiller]");
        Assert.Equal(18, cartes.Length);
        Assert.Equal("EnAppel", page.QuerySelector("[data-conseiller='C01']")!.GetAttribute("data-statut"));
        Assert.Equal("Pause", page.QuerySelector("[data-conseiller='C06']")!.GetAttribute("data-statut"));
        Assert.Contains("Karim", page.QuerySelector("[data-conseiller='C01']")!.TextContent);
    }

    [Fact]
    public async Task m1_chaque_conseiller_connecte_affiche_la_duree_de_son_statut()
    {
        Rejouer(
            Statut("C01", StatutConseiller.EnAppel, H(9, 0, 0)),
            Statut("C02", StatutConseiller.Disponible, H(9, 12, 35)));

        var page = await Page("/moniteur/2");
        Assert.Equal("12:35", page.QuerySelector("[data-conseiller='C01'] [data-duree]")!.TextContent.Trim());
        Assert.Equal("00:00", page.QuerySelector("[data-conseiller='C02'] [data-duree]")!.TextContent.Trim());
    }

    [Fact]
    public async Task m1_l_accueil_mene_au_moniteur_2()
    {
        var page = await Page("/");
        Assert.NotNull(page.QuerySelector("a[href='/moniteur/2']"));
    }

    // ── Mission 3.2 🟢 — RG4 : appels longs et pauses longues ──────────

    [Fact]
    public void m2_appel_et_pause_au_dela_du_seuil_mais_pas_le_post_appel()
    {
        var alertes = ReglesDuree.Alertes(
        [
            E("C01", StatutConseiller.EnAppel, 11, 29),
            E("C11", StatutConseiller.Pause, 11, 28),
            E("C16", StatutConseiller.PostAppel, 11, 0),
            E("C02", StatutConseiller.EnAppel, 11, 38),
            E("C10", StatutConseiller.Chat, 11, 20)
        ], H(11, 42), Dix);

        Assert.Equal(["C10", "C11", "C01"], alertes.Select(a => a.ConseillerId));
        Assert.Equal(H(0, 22), alertes[0].Duree);
    }

    [Fact]
    public void m2_exactement_le_seuil_ne_declenche_pas()
    {
        Assert.Empty(ReglesDuree.Alertes([E("C01", StatutConseiller.EnAppel, 11, 32)], H(11, 42), Dix));
    }

    [Fact]
    public async Task m2_les_seuils_viennent_de_la_configuration()
    {
        using var test = new AvecSeuils(appelLong: 5);
        test.Rejouer(Statut("C01", StatutConseiller.EnAppel, H(9, 0)), Statut("C02", StatutConseiller.Disponible, H(9, 6)));
        var page = await test.Page("/moniteur/2");
        Assert.Equal("true", page.QuerySelector("[data-conseiller='C01']")!.GetAttribute("data-alerte"));
        Assert.Equal("false", page.QuerySelector("[data-conseiller='C02']")!.GetAttribute("data-alerte"));
    }

    [Fact]
    public async Task m2_le_moniteur_2_marque_les_conseillers_en_alerte()
    {
        Rejouer(
            Statut("C01", StatutConseiller.EnAppel, H(9, 0)),
            Statut("C06", StatutConseiller.Pause, H(9, 5)),
            Statut("C02", StatutConseiller.Disponible, H(9, 12)));

        var page = await Page("/moniteur/2");
        Assert.Equal("true", page.QuerySelector("[data-conseiller='C01']")!.GetAttribute("data-alerte"));
        Assert.Equal("false", page.QuerySelector("[data-conseiller='C06']")!.GetAttribute("data-alerte"));
    }

    // ── Mission 3.3 🟡 — compter et filtrer par statut ─────────────────

    [Fact]
    public async Task m3_un_compteur_par_statut()
    {
        Rejouer(
            Statut("C01", StatutConseiller.EnAppel, H(9, 0)),
            Statut("C02", StatutConseiller.EnAppel, H(9, 0)),
            Statut("C03", StatutConseiller.Pause, H(9, 0)),
            Statut("C04", StatutConseiller.Disponible, H(9, 0)));

        var page = await Page("/moniteur/2");
        string? N(string statut) => page.QuerySelector($"[data-statut-compteur='{statut}']")?.GetAttribute("data-nombre");
        Assert.Equal("2", N("EnAppel"));
        Assert.Equal("1", N("Pause"));
        Assert.Equal("1", N("Disponible"));
        Assert.Equal("14", N("HorsLigne"));
    }

    [Fact]
    public async Task m3_le_plateau_se_filtre_par_statut()
    {
        Rejouer(
            Statut("C01", StatutConseiller.EnAppel, H(9, 0)),
            Statut("C03", StatutConseiller.Pause, H(9, 0)),
            Statut("C09", StatutConseiller.Pause, H(9, 1)));

        var page = await Page("/moniteur/2?statut=Pause");
        Assert.Equal(["C03", "C09"], page.QuerySelectorAll("[data-conseiller]").Select(e => e.GetAttribute("data-conseiller")));
    }

    /// <summary>Une application dont le seuil d'appel long est modifié par la configuration.</summary>
    private sealed class AvecSeuils(int appelLong) : TestCrc(new Dictionary<string, string?>
    {
        ["Supervision:Seuils:AppelLongMinutes"] = appelLong.ToString()
    })
    {
        public new void Rejouer(params Evenement[] e) => base.Rejouer(e);
        public new Task<AngleSharp.Dom.IDocument> Page(string url) => base.Page(url);
    }
}
