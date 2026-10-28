using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using Mfa.Crc.Simulateur;
using Mfa.Crc.Supervision.Donnees;
using Mfa.Crc.Supervision.TempsReel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mfa.Crc.Tests.Infrastructure;

/// <summary>
/// L'application, démarrée pour un test : base SQLite neuve, simulation arrêtée
/// (les tests poussent eux-mêmes les événements).
/// </summary>
public sealed class ApplicationTest : WebApplicationFactory<Program>
{
    public string FichierBase { get; } = Path.Combine(Path.GetTempPath(), $"crc-test-{Guid.NewGuid():N}.db");
    private readonly Dictionary<string, string?> _reglages;
    private readonly string _environnement;

    public ApplicationTest(Dictionary<string, string?>? reglages = null, string environnement = "Testing")
    {
        _reglages = reglages ?? [];
        _environnement = environnement;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(_environnement);
        builder.UseSetting("ConnectionStrings:Crc", $"Data Source={FichierBase}");
        builder.UseSetting("Simulation:Active", "false");
        foreach (var (cle, valeur) in _reglages) builder.UseSetting(cle, valeur);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SqliteConnection.ClearAllPools();
        try { File.Delete(FichierBase); } catch (IOException) { }
    }
}

/// <summary>Base de tous les tests : une application neuve par test, et des raccourcis.</summary>
public abstract class TestCrc : IDisposable
{
    public const string Login = "cmorel";
    public const string MotDePasse = "Supervision2026!";

    protected ApplicationTest App { get; }
    protected HttpClient Client { get; }
    protected EtatCrc Etat => App.Services.GetRequiredService<EtatCrc>();

    protected TestCrc() : this(null) { }

    protected TestCrc(Dictionary<string, string?>? reglages, string environnement = "Testing")
    {
        App = new ApplicationTest(reglages, environnement);
        Client = App.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });
    }

    public void Dispose()
    {
        Client.Dispose();
        App.Dispose();
        GC.SuppressFinalize(this);
    }

    // ---------- HTTP ----------

    /// <summary>
    /// Envoie une requête. Si l'application demande une connexion (à partir de la séance 6),
    /// le test se connecte avec le compte de démonstration puis renvoie la requête.
    /// </summary>
    protected async Task<HttpResponseMessage> Envoyer(HttpMethod methode, string url, object? json = null, bool connexionAuto = true)
    {
        var reponse = await Client.SendAsync(Requete(methode, url, json));
        if (connexionAuto && DemandeConnexion(reponse))
        {
            await SeConnecter();
            reponse = await Client.SendAsync(Requete(methode, url, json));
        }
        return reponse;
    }

    protected Task<HttpResponseMessage> Get(string url, bool connexionAuto = true) => Envoyer(HttpMethod.Get, url, null, connexionAuto);
    protected Task<HttpResponseMessage> Post(string url, object? json = null, bool connexionAuto = true) => Envoyer(HttpMethod.Post, url, json, connexionAuto);

    /// <summary>GET d'une page HTML qui doit répondre 200 ; renvoie le document.</summary>
    protected async Task<IDocument> Page(string url)
    {
        var reponse = await Get(url);
        var html = await reponse.Content.ReadAsStringAsync();
        Assert.True(reponse.StatusCode == HttpStatusCode.OK, $"GET {url} : {(int)reponse.StatusCode} attendu 200\n{Extrait(html)}");
        return await BrowsingContext.New(Configuration.Default).OpenAsync(r => r.Content(html));
    }

    protected async Task<T> Json<T>(string url)
    {
        var reponse = await Get(url);
        Assert.True(reponse.StatusCode == HttpStatusCode.OK, $"GET {url} : {(int)reponse.StatusCode} attendu 200");
        return (await reponse.Content.ReadFromJsonAsync<T>())!;
    }

    private static HttpRequestMessage Requete(HttpMethod methode, string url, object? json)
    {
        var r = new HttpRequestMessage(methode, url);
        if (json is not null) r.Content = JsonContent.Create(json);
        return r;
    }

    private static bool DemandeConnexion(HttpResponseMessage r) =>
        r.StatusCode == HttpStatusCode.Unauthorized
        || (r.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.Found
            && r.Headers.Location?.ToString().Contains("/connexion", StringComparison.OrdinalIgnoreCase) == true);

    /// <summary>Se connecte d'avance si l'application l'exige (avant des requêtes en parallèle).</summary>
    protected async Task AssurerConnexion()
    {
        var r = await Client.GetAsync("/moniteur/1");
        if (DemandeConnexion(r)) await SeConnecter();
    }

    /// <summary>Se connecte par le formulaire /connexion (séance 6).</summary>
    protected async Task<HttpResponseMessage> SeConnecter(string login = Login, string motDePasse = MotDePasse)
    {
        var formulaire = await Client.GetAsync("/connexion");
        var html = await formulaire.Content.ReadAsStringAsync();
        var jeton = Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;
        return await Client.PostAsync("/connexion", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Login"] = login, ["MotDePasse"] = motDePasse, ["__RequestVerificationToken"] = jeton
        }));
    }

    /// <summary>Une connexion SignalR au hub /hubs/crc, avec les cookies du client (séance 5).</summary>
    protected HubConnection Hub()
    {
        return new HubConnectionBuilder()
            .WithUrl(new Uri(App.Server.BaseAddress, "/hubs/crc"), o =>
            {
                o.HttpMessageHandlerFactory = _ => App.Server.CreateHandler();
                o.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
                if (_cookie is not null) o.Headers["Cookie"] = _cookie;
            })
            .Build();
    }

    private string? _cookie;

    /// <summary>Un hub démarré ; se connecte d'abord à l'application si le hub l'exige (séance 6).</summary>
    protected async Task<HubConnection> HubDemarre(Action<HubConnection> abonnements)
    {
        var hub = Hub();
        abonnements(hub);
        try
        {
            await hub.StartAsync();
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.Unauthorized)
        {
            await hub.DisposeAsync();
            await PreparerCookieHub();
            hub = Hub();
            abonnements(hub);
            await hub.StartAsync();
        }
        return hub;
    }

    /// <summary>Récupère le cookie de session pour le hub, après connexion (séance 6 et suivantes).</summary>
    protected async Task PreparerCookieHub()
    {
        var r = await SeConnecter();
        if (r.Headers.TryGetValues("Set-Cookie", out var valeurs))
            _cookie = string.Join("; ", valeurs.Select(v => v.Split(';')[0]));
    }

    protected static string Extrait(string texte) => texte.Length > 600 ? texte[..600] + "…" : texte;

    // ---------- Événements ----------

    protected static TimeSpan H(int h, int m, int s = 0) => new(h, m, s);

    protected void Rejouer(params Evenement[] evenements)
    {
        foreach (var e in evenements) Etat.Appliquer(e);
    }

    protected static ContactArrive Arrive(string id, string file, Priorite priorite, TimeSpan heure,
        Canal canal = Canal.Telephone, string societaire = "S002", string motif = "Question") =>
        new(heure, id, file, canal, priorite, societaire, motif);

    protected static StatutConseillerChange Statut(string conseiller, StatutConseiller statut, TimeSpan heure) =>
        new(heure, conseiller, statut);

    protected static ContactDecroche Decroche(string contact, string conseiller, TimeSpan heure) =>
        new(heure, contact, conseiller);

    /// <summary>Rejoue le vrai scénario de la matinée jusqu'à l'heure donnée.</summary>
    protected void RejouerScenarioJusqua(TimeSpan heure)
    {
        var sim = SimulateurCrc.Charger(Path.Combine(DossierData, "scenario-matinee.json"));
        sim.EvenementRejoue += (_, e) => Etat.Appliquer(e);
        sim.HorlogeAvancee += (_, h) => Etat.AvancerHorloge(h);
        sim.AvancerJusqua(heure);
    }

    protected static string DossierData => ChargeurDonnees.TrouverDossierData(AppContext.BaseDirectory);

    // ---------- Base de données ----------

    /// <summary>Exécute un SELECT sur la base de l'application et renvoie les lignes.</summary>
    protected List<Dictionary<string, object?>> Sql(string requete)
    {
        using var scope = App.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CrcContext>();
        var cnx = db.Database.GetDbConnection();
        cnx.Open();
        using var cmd = cnx.CreateCommand();
        cmd.CommandText = requete;
        using var lecteur = cmd.ExecuteReader();
        var lignes = new List<Dictionary<string, object?>>();
        while (lecteur.Read())
        {
            var ligne = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < lecteur.FieldCount; i++)
                ligne[lecteur.GetName(i)] = lecteur.IsDBNull(i) ? null : lecteur.GetValue(i);
            lignes.Add(ligne);
        }
        return lignes;
    }

    protected bool TableExiste(string table) =>
        Sql($"SELECT name FROM sqlite_master WHERE type='table' AND name='{table}'").Count == 1;

    protected HashSet<string> Colonnes(string table) =>
        Sql($"PRAGMA table_info('{table}')").Select(l => (string)l["name"]!).ToHashSet(StringComparer.OrdinalIgnoreCase);

    protected void DemarrerApplication() => _ = App.Services;
}
