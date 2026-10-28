using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Mfa.Crc.Simulateur;
using Mfa.Crc.Tests.Infrastructure;
using Microsoft.AspNetCore.SignalR.Client;

namespace Mfa.Crc.Tests.Seance05;

/// <summary>
/// SÉANCE 5 — Le temps réel et les quatre moniteurs : SignalR, configuration du poste,
/// sélection partagée entre moniteurs
///
///   dotnet test --filter Groupe=seance-05
/// </summary>
[Trait("Groupe", "seance-05")]
public class TempsReelTest : TestCrc
{
    private static readonly TimeSpan Delai = TimeSpan.FromSeconds(5);

    // ── Mission 5.1 🟢 — le hub diffuse chaque changement ───────────

    [Fact]
    public async Task m1_le_hub_diffuse_un_changement_quand_un_contact_arrive()
    {
        var recu = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var hub = await HubDemarre(h => h.On<JsonElement>("changement", e => recu.TrySetResult(e)));

        Rejouer(Arrive("K0001", "Sinistres", Priorite.Normale, H(9, 0)));

        var message = await recu.Task.WaitAsync(Delai);
        Assert.Equal("ContactArrive", message.GetProperty("type").GetString());
        Assert.Equal("09:00:00", message.GetProperty("heure").GetString());
    }

    [Fact]
    public async Task m1_un_changement_de_statut_est_diffuse_aussi()
    {
        var types = new List<string>();
        var deux = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var hub = await HubDemarre(h => h.On<JsonElement>("changement", e =>
        {
            lock (types) { types.Add(e.GetProperty("type").GetString()!); if (types.Count == 2) deux.TrySetResult(); }
        }));

        Rejouer(Statut("C01", StatutConseiller.Disponible, H(8, 0)), Statut("C01", StatutConseiller.EnAppel, H(8, 5)));

        await deux.Task.WaitAsync(Delai);
        Assert.All(types, t => Assert.Equal("StatutConseillerChange", t));
    }

    // ── Mission 5.2 🟢 — les moniteurs écoutent au lieu de se recharger ─

    [Theory]
    [InlineData("/moniteur/1")]
    [InlineData("/moniteur/2")]
    public async Task m2_les_moniteurs_ne_se_rechargent_plus_toutes_les_5_secondes(string url)
    {
        var page = await Page(url);
        Assert.Null(page.QuerySelector("meta[http-equiv='refresh']"));
        var scripts = string.Join(" ", page.QuerySelectorAll("script").Select(s => s.GetAttribute("src") ?? s.TextContent));
        Assert.Contains("signalr", scripts, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/js/moniteur.js", scripts);
    }

    // ── Mission 5.3 🟡 — la configuration du poste ──────────────────

    [Fact]
    public async Task m3_l_api_du_poste_lit_config_poste_json()
    {
        var json = await Json<JsonElement>("/api/poste");
        Assert.Equal("SUP-CRC-01", json.GetProperty("poste").GetString());
        var moniteurs = json.GetProperty("moniteurs").EnumerateArray().ToList();
        Assert.Equal(4, moniteurs.Count);
        Assert.Equal(["/moniteur/1", "/moniteur/2", "/moniteur/3", "/moniteur/4"],
            moniteurs.Select(m => m.GetProperty("url").GetString()));
        Assert.Equal([1, 2, 3, 4], moniteurs.Select(m => m.GetProperty("ecran").GetInt32()));
        Assert.True(json.GetProperty("repliEnFenetres").GetBoolean());
    }

    // ── Mission 5.4 🟡 — une sélection partagée par les moniteurs ─────

    [Fact]
    public async Task m4_selectionner_un_conseiller_est_memorise()
    {
        var r = await Post("/api/selection", new { conseillerId = "C01" });
        Assert.Equal(HttpStatusCode.NoContent, r.StatusCode);

        var json = await Json<JsonElement>("/api/selection");
        Assert.Equal("C01", json.GetProperty("conseillerId").GetString());
    }

    [Fact]
    public async Task m4_la_selection_est_diffusee_aux_autres_moniteurs()
    {
        var recu = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var hub = await HubDemarre(h => h.On<JsonElement>("selection", e => recu.TrySetResult(e)));

        await Post("/api/selection", new { conseillerId = "C14" });

        var message = await recu.Task.WaitAsync(Delai);
        Assert.Equal("C14", message.GetProperty("conseillerId").GetString());
    }

    [Fact]
    public async Task m4_un_conseiller_inconnu_est_refuse()
    {
        var r = await Post("/api/selection", new { conseillerId = "C99" });
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
        Assert.Equal("application/problem+json", r.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task m4_sur_le_moniteur_2_cliquer_un_conseiller_le_selectionne()
    {
        var page = await Page("/moniteur/2");
        Assert.NotNull(page.QuerySelector("button[data-selectionner='C01']"));
    }
}
