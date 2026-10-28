using Mfa.Crc.Simulateur;
using Mfa.Crc.Supervision.Regles;
using Mfa.Crc.Supervision.TempsReel;
using Mfa.Crc.Tests.Infrastructure;

namespace Mfa.Crc.Tests.Seance01;

/// <summary>
/// SÉANCE 1 — La file d'attente : trier (RG1) et rendre lisible le moniteur 1
///
///   dotnet test --filter Groupe=seance-01
/// </summary>
[Trait("Groupe", "seance-01")]
public class FileAttenteTest : TestCrc
{
    private static ContactEnAttente C(string id, Priorite p, int h, int m) =>
        new(id, "Contrats", Canal.Telephone, p, "S002", "Question", new TimeSpan(h, m, 0));

    // ── Mission 1.2 🟢 — RG1 : trier la file ─────────────────────────

    [Fact]
    public void m2_l_escalade_passe_devant_meme_arrivee_plus_tard()
    {
        var triee = ReglesFile.Trier([C("A", Priorite.Normale, 10, 0), C("B", Priorite.Escalade, 10, 5), C("C", Priorite.Haute, 10, 3)]);
        Assert.Equal(["B", "C", "A"], triee.Select(c => c.Id));
    }

    [Fact]
    public void m2_a_priorite_egale_la_plus_longue_attente_d_abord()
    {
        var triee = ReglesFile.Trier([C("A", Priorite.Normale, 10, 2), C("B", Priorite.Normale, 10, 1), C("C", Priorite.Normale, 10, 3)]);
        Assert.Equal(["B", "A", "C"], triee.Select(c => c.Id));
    }

    [Fact]
    public void m2_une_file_vide_donne_une_liste_vide()
    {
        Assert.Empty(ReglesFile.Trier([]));
    }

    [Fact]
    public async Task m2_le_moniteur_1_affiche_la_file_triee()
    {
        Rejouer(
            Arrive("K0001", "Contrats", Priorite.Normale, H(9, 0)),
            Arrive("K0002", "Sinistres", Priorite.Normale, H(9, 1)),
            Arrive("K0003", "Sinistres", Priorite.Escalade, H(9, 2)),
            Arrive("K0004", "Sante", Priorite.Haute, H(9, 3)));

        var page = await Page("/moniteur/1");
        var ordre = page.QuerySelectorAll("[data-contact]").Select(e => e.GetAttribute("data-contact"));
        Assert.Equal(["K0003", "K0004", "K0001", "K0002"], ordre);
    }

    // ── Mission 1.3 🟡 — l'attente et la priorité se lisent d'un coup d'œil ─

    [Fact]
    public async Task m3_chaque_contact_affiche_son_attente_en_minutes_secondes()
    {
        Rejouer(
            Arrive("K0001", "Contrats", Priorite.Normale, H(9, 0, 0)),
            Arrive("K0002", "Contrats", Priorite.Normale, H(9, 3, 25)),
            Statut("C04", StatutConseiller.Disponible, H(9, 4, 51)));   // l'heure courante devient 09:04:51

        var page = await Page("/moniteur/1");
        var attente = page.QuerySelector("[data-contact='K0001'] [data-attente]");
        Assert.NotNull(attente);
        Assert.Equal("04:51", attente!.TextContent.Trim());
        Assert.Equal("01:26", page.QuerySelector("[data-contact='K0002'] [data-attente]")!.TextContent.Trim());
    }

    [Fact]
    public async Task m3_la_priorite_est_un_badge_colore()
    {
        Rejouer(
            Arrive("K0001", "Sinistres", Priorite.Escalade, H(9, 0)),
            Arrive("K0002", "Contrats", Priorite.Haute, H(9, 0)),
            Arrive("K0003", "Contrats", Priorite.Normale, H(9, 0)));

        var page = await Page("/moniteur/1");
        Assert.NotNull(page.QuerySelector("[data-contact='K0001'] .badge.prio-escalade"));
        Assert.NotNull(page.QuerySelector("[data-contact='K0002'] .badge.prio-haute"));
        Assert.NotNull(page.QuerySelector("[data-contact='K0003'] .badge.prio-normale"));
    }

    [Fact]
    public async Task m3_une_attente_de_plus_de_3_minutes_est_signalee()
    {
        Rejouer(
            Arrive("K0001", "Contrats", Priorite.Normale, H(9, 0)),
            Arrive("K0002", "Contrats", Priorite.Normale, H(9, 2)),
            Statut("C04", StatutConseiller.Disponible, H(9, 3, 30)));

        var page = await Page("/moniteur/1");
        Assert.True(page.QuerySelector("[data-contact='K0001'] [data-attente]")!.ClassList.Contains("alerte"));
        Assert.False(page.QuerySelector("[data-contact='K0002'] [data-attente]")!.ClassList.Contains("alerte"));
    }

    // ── Mission 1.4 🟡 — combien attendent dans chaque file ? ─────────

    [Fact]
    public async Task m4_un_compteur_par_file_meme_vide()
    {
        Rejouer(
            Arrive("K0001", "Sinistres", Priorite.Normale, H(9, 0)),
            Arrive("K0002", "Sinistres", Priorite.Normale, H(9, 1)),
            Arrive("K0003", "Sante", Priorite.Normale, H(9, 2)));

        var page = await Page("/moniteur/1");
        string? Nombre(string file) => page.QuerySelector($"[data-file='{file}']")?.GetAttribute("data-nombre");
        Assert.Equal("2", Nombre("Sinistres"));
        Assert.Equal("1", Nombre("Sante"));
        Assert.Equal("0", Nombre("Contrats"));
        Assert.Equal("0", Nombre("Accueil"));
    }
}
