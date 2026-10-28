using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Data.Sqlite;

/// <summary>
/// FOURNI — rien à modifier. Fait fonctionner l'application là où elle tourne : votre Codespace.
///
/// 1. Dans un Codespace, le navigateur ne parle pas directement à l'application : il passe par
///    un relais de GitHub (https://…-5000.app.github.dev), qui transmet la requête à
///    http://localhost:5000. Sans précaution, l'application croit que son adresse est
///    « localhost » et y renvoie le navigateur (redirection vers la connexion, séance 6) :
///    la page ne répond plus. Le relais indique l'adresse publique dans les en-têtes
///    X-Forwarded-Host et X-Forwarded-Proto ; on les fait lire à l'application.
///    On ne les croit QUE dans un Codespace : en production, on n'accepte ces en-têtes que
///    du relais connu (KnownProxies), sinon n'importe qui pourrait se faire passer pour lui.
///
/// 2. La base SQLite (crc.db) est toujours rangée dans le dossier du projet, quel que soit le
///    dossier d'où l'on lance l'application (racine du dépôt, F5, tâche VS Code…).
/// </summary>
public static class Hebergement
{
    public static bool DansUnCodespace =>
        string.Equals(Environment.GetEnvironmentVariable("CODESPACES"), "true", StringComparison.OrdinalIgnoreCase);

    public static void Preparer(WebApplicationBuilder builder)
    {
        if (DansUnCodespace)
        {
            builder.Services.Configure<ForwardedHeadersOptions>(o =>
            {
                o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
                o.KnownIPNetworks.Clear();   // le relais de GitHub n'a pas d'adresse fixe connue
                o.KnownProxies.Clear();
            });
        }

        var chaine = builder.Configuration.GetConnectionString("Crc");
        if (chaine is null) return;
        var cs = new SqliteConnectionStringBuilder(chaine);
        if (cs.DataSource is "" or ":memory:" || Path.IsPathRooted(cs.DataSource)) return;
        cs.DataSource = Path.Combine(builder.Environment.ContentRootPath, cs.DataSource);
        builder.Configuration["ConnectionStrings:Crc"] = cs.ToString();
    }

    public static void Utiliser(WebApplication app)
    {
        if (DansUnCodespace) app.UseForwardedHeaders();
    }
}
