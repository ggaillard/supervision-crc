using Mfa.Crc.Supervision.Donnees;
using Mfa.Crc.Supervision.TempsReel;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
Hebergement.Preparer(builder);       // fourni : Codespace et emplacement de la base

// ---------- Services ----------

builder.Services.AddRazorPages();
builder.Services.AddDbContext<CrcContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Crc")));

var chemins = new CheminsDonnees(ChargeurDonnees.TrouverDossierData(builder.Environment.ContentRootPath));
builder.Services.AddSingleton(chemins);
builder.Services.AddSingleton(builder.Configuration.GetSection("Simulation").Get<OptionsSimulation>() ?? new OptionsSimulation());
builder.Services.AddSingleton<EtatCrc>();
builder.Services.AddHostedService<ServiceSimulation>();

var app = builder.Build();
Hebergement.Utiliser(app);           // fourni : à garder en premier

// ---------- Base de données et plateau ----------

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CrcContext>();
    db.Database.Migrate();
    ChargeurDonnees.Charger(db, chemins.Dossier);

    var etat = app.Services.GetRequiredService<EtatCrc>();
    etat.Initialiser(db.Conseillers.AsNoTracking().OrderBy(c => c.Id).AsEnumerable().Select(c =>
        (c.Id, c.NomComplet, (IReadOnlySet<string>)c.Competences.Split(';', StringSplitOptions.RemoveEmptyEntries).ToHashSet())));
}

// ---------- Pipeline HTTP ----------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Erreur");
}
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

app.Run();

/// <summary>Rendu visible pour les tests (WebApplicationFactory).</summary>
public partial class Program;
