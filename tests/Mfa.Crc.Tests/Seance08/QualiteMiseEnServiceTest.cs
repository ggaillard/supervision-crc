using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using Mfa.Crc.Tests.Infrastructure;

namespace Mfa.Crc.Tests.Seance08;

/// <summary>
/// SÉANCE 8 — Avant la mise en service : la faille, vos propres tests, l'exploitation
///
///   dotnet test --filter Groupe=seance-08
/// </summary>
[Trait("Groupe", "seance-08")]
public class QualiteMiseEnServiceTest : TestCrc
{
    // ── Mission 8.1 🟡 — l'audit ─────────────────────────────────────
    // (Le test ne dit pas où est la faille : à vous de la trouver avec la grille d'audit.)

    [Theory]
    [InlineData("' OR 1=1 --")]
    [InlineData("' OR 'a'='a' --")]
    [InlineData("%' UNION SELECT Id, Login, Login, Login, 0 FROM Superviseurs --")]
    public async Task m1_une_saisie_malveillante_ne_renvoie_aucun_sociétaire(string saisie)
    {
        var r = await Get("/societaires?nom=" + Uri.EscapeDataString(saisie));
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var html = await r.Content.ReadAsStringAsync();
        Assert.Contains("0 résultat(s)", html);
    }

    [Fact]
    public async Task m1_un_nom_avec_apostrophe_ne_fait_pas_planter_et_la_recherche_normale_marche()
    {
        var r = await Get("/societaires?nom=" + Uri.EscapeDataString("d'Arc"));
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);

        var page = await Page("/societaires?nom=ver");
        Assert.Contains("S009", page.QuerySelectorAll("[data-societaire]").Select(e => e.GetAttribute("data-societaire")));
    }

    // ── Mission 8.2 🟢 — vos propres tests ────────────────────────────

    [Fact]
    public void m2_au_moins_deux_tests_personnels_dans_le_dossier_Etudiant()
    {
        var tests = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.Namespace == "Mfa.Crc.Tests.Etudiant")
            .SelectMany(t => t.GetMethods())
            .Where(m => m.GetCustomAttributes().Any(a => a is FactAttribute))
            .ToList();
        Assert.True(tests.Count >= 2, $"{tests.Count} test(s) trouvé(s) dans tests/Mfa.Crc.Tests/Etudiant : 2 attendus.");
    }

    // ── Mission 8.4 🟡 — prêt pour l'exploitation ─────────────────────

    [Fact]
    public async Task m4_une_page_de_sante_pour_l_exploitation()
    {
        var r = await Get("/sante", connexionAuto: false);
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var json = await r.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("ok", json.GetProperty("statut").GetString());
        Assert.Equal("ok", json.GetProperty("base").GetString());
        Assert.True(json.TryGetProperty("heureSimulee", out _));
        Assert.True(json.TryGetProperty("version", out _));
    }

    [Fact]
    public async Task m4_en_production_une_erreur_repond_en_problem_json_sans_detail_technique()
    {
        using var prod = new EnProduction();
        var (code, type, corps) = await prod.RequeteInvalide();
        Assert.Equal(HttpStatusCode.BadRequest, code);
        Assert.Equal("application/problem+json", type);
        Assert.DoesNotContain("Exception", corps);
        Assert.DoesNotContain("   at ", corps);
    }

    private sealed class EnProduction() : TestCrc(null, "Production")
    {
        public async Task<(HttpStatusCode, string?, string)> RequeteInvalide()
        {
            // Une date invalide sur l'API des échéances : la requête est mal formée
            var r = await Get("/api/reclamations/echeances?date=pas-une-date");
            return (r.StatusCode, r.Content.Headers.ContentType?.MediaType, await r.Content.ReadAsStringAsync());
        }
    }
}
