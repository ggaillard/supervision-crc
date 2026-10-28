using Mfa.Crc.Simulateur;

// Démo console : rejoue la matinée et affiche chaque événement.
// Usage : dotnet run --project src/Simulateur.Demo [acceleration]
//   acceleration = secondes simulées par seconde réelle (défaut 600 : 4 h en 24 s)

var acceleration = args.Length > 0 && double.TryParse(args[0], out var a) ? a : 600;
var scenario = TrouverFichier(Path.Combine("data", "scenario-matinee.json"));

var simulateur = SimulateurCrc.Charger(scenario);
simulateur.Acceleration = acceleration;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine($"Scénario : {simulateur.Evenements.Count} événements, accélération x{acceleration}");
Console.WriteLine();

simulateur.ContactArrive += (_, e) =>
    Ecrire(e.Heure, ConsoleColor.Cyan, $"Arrivée   {e.ContactId} [{e.File}/{e.Canal}] {e.Priorite} — {e.Motif}");
simulateur.ContactDecroche += (_, e) =>
    Ecrire(e.Heure, ConsoleColor.Green, $"Décroché  {e.ContactId} par {e.ConseillerId}");
simulateur.ContactAbandonne += (_, e) =>
    Ecrire(e.Heure, ConsoleColor.Red, $"Abandon   {e.ContactId}");
simulateur.ContactClos += (_, e) =>
    Ecrire(e.Heure, ConsoleColor.Gray, $"Clos      {e.ContactId} par {e.ConseillerId}, satisfaction {e.Satisfaction}/5");
simulateur.StatutConseillerChange += (_, e) =>
    Ecrire(e.Heure, ConsoleColor.DarkGray, $"Statut    {e.ConseillerId} → {e.Statut}");
simulateur.EscaladeSuperviseur += (_, e) =>
    Ecrire(e.Heure, ConsoleColor.Yellow, $"ESCALADE  {e.ContactId} par {e.ConseillerId} — réclamation {e.ReclamationId}");

using var arret = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; arret.Cancel(); };

try
{
    await simulateur.RejouerAsync(arret.Token);
    Console.WriteLine("\nFin du scénario.");
}
catch (TaskCanceledException)
{
    Console.WriteLine($"\nArrêté à {simulateur.HeureSimulee:hh\\:mm\\:ss}.");
}

static void Ecrire(TimeSpan heure, ConsoleColor couleur, string texte)
{
    lock (Console.Out)
    {
        Console.ForegroundColor = couleur;
        Console.WriteLine($"{heure:hh\\:mm\\:ss}  {texte}");
        Console.ResetColor();
    }
}

static string TrouverFichier(string relatif)
{
    for (var dossier = new DirectoryInfo(AppContext.BaseDirectory); dossier != null; dossier = dossier.Parent)
    {
        var chemin = Path.Combine(dossier.FullName, relatif);
        if (File.Exists(chemin)) return chemin;
    }
    throw new FileNotFoundException($"Introuvable : {relatif}. Lancez la démo depuis le dépôt.");
}
