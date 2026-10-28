using Mfa.Crc.Simulateur;

namespace Mfa.Crc.Supervision.TempsReel;

/// <summary>Options de la section « Simulation » d'appsettings.json.</summary>
public sealed class OptionsSimulation
{
    /// <summary>false dans les tests : on y pousse les événements à la main.</summary>
    public bool Active { get; set; } = true;

    /// <summary>Secondes simulées par seconde réelle.</summary>
    public double Acceleration { get; set; } = 20;

    /// <summary>Recommencer la matinée quand elle est finie.</summary>
    public bool Boucle { get; set; } = true;

    /// <summary>
    /// Mode recette : rejoue d'un coup la matinée jusqu'à cette heure, puis s'arrête
    /// (« 11:42 » fige les moniteurs à 11 h 42). Vide : la simulation se déroule normalement.
    /// </summary>
    public TimeSpan? ArreterA { get; set; }
}

/// <summary>Rejoue le scénario de la matinée en tâche de fond et alimente <see cref="EtatCrc"/>.</summary>
public sealed class ServiceSimulation(EtatCrc etat, OptionsSimulation options, CheminsDonnees chemins,
    ILogger<ServiceSimulation> journal) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken arret)
    {
        if (!options.Active) return;
        var plateauInitial = etat.Plateau.Select(c => (c.Id, c.NomComplet, c.Competences)).ToList();
        do
        {
            etat.Initialiser(plateauInitial);
            var simulateur = SimulateurCrc.Charger(chemins.Scenario);
            simulateur.Acceleration = options.Acceleration;
            simulateur.EvenementRejoue += (_, e) => etat.Appliquer(e);
            simulateur.HorlogeAvancee += (_, h) => etat.AvancerHorloge(h);
            if (options.ArreterA is { } heureRecette)
            {
                simulateur.AvancerJusqua(heureRecette);
                journal.LogInformation("Mode recette : simulation figée à {Heure}", heureRecette);
                return;
            }
            journal.LogInformation("Simulation de la matinée lancée (x{Acceleration})", options.Acceleration);
            try
            {
                await simulateur.RejouerAsync(arret);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        } while (options.Boucle && !arret.IsCancellationRequested);
    }
}

/// <summary>Où se trouvent les fichiers du dossier data/.</summary>
public sealed record CheminsDonnees(string Dossier)
{
    public string Scenario => Path.Combine(Dossier, "scenario-matinee.json");
}
