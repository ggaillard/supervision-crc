using System.Text.Json;

namespace Mfa.Crc.Simulateur;

/// <summary>
/// Rejoue un scénario de matinée du centre de relation client et déclenche un événement C#
/// pour chaque fait (contact qui arrive, conseiller qui change de statut…).
/// </summary>
/// <remarks>
/// Les événements sont déclenchés depuis un thread de travail. Dans une application WPF,
/// repassez sur le thread de l'interface avant de toucher aux vues
/// (Application.Current.Dispatcher.Invoke).
/// </remarks>
public sealed class SimulateurCrc
{
    private readonly List<Evenement> _evenements;
    private int _prochain;   // index du prochain événement à rejouer

    public event EventHandler<ContactArrive>? ContactArrive;
    public event EventHandler<ContactDecroche>? ContactDecroche;
    public event EventHandler<ContactAbandonne>? ContactAbandonne;
    public event EventHandler<ContactClos>? ContactClos;
    public event EventHandler<StatutConseillerChange>? StatutConseillerChange;
    public event EventHandler<EscaladeSuperviseur>? EscaladeSuperviseur;

    /// <summary>Déclenché à chaque événement, quel que soit son type.</summary>
    public event EventHandler<Evenement>? EvenementRejoue;

    /// <summary>Déclenché à la fin du scénario.</summary>
    public event EventHandler? Termine;

    /// <summary>
    /// L'horloge simulée avance (au moins une fois par seconde réelle pendant <see cref="RejouerAsync"/>,
    /// et à la fin de <see cref="AvancerJusqua"/>) : les durées affichées continuent de courir entre deux événements.
    /// </summary>
    public event EventHandler<TimeSpan>? HorlogeAvancee;

    /// <summary>Nombre de secondes simulées par seconde réelle (60 = une minute par seconde).</summary>
    public double Acceleration { get; set; } = 60;

    /// <summary>Heure simulée du dernier événement rejoué.</summary>
    public TimeSpan HeureSimulee { get; private set; }

    /// <summary>Tous les événements du scénario, dans l'ordre.</summary>
    public IReadOnlyList<Evenement> Evenements => _evenements;

    private SimulateurCrc(List<Evenement> evenements, TimeSpan debut)
    {
        _evenements = evenements;
        HeureSimulee = debut;
    }

    /// <summary>Charge un scénario au format de data/scenario-matinee.json.</summary>
    public static SimulateurCrc Charger(string cheminScenario)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(cheminScenario));
        var racine = doc.RootElement;
        var debut = TimeSpan.Parse(racine.GetProperty("debut").GetString()!);
        var liste = new List<Evenement>();
        foreach (var e in racine.GetProperty("evenements").EnumerateArray())
        {
            liste.Add(Lire(e));
        }
        return new SimulateurCrc(liste, debut);
    }

    /// <summary>
    /// Rejoue le scénario en respectant les écarts de temps (divisés par <see cref="Acceleration"/>).
    /// À appeler sans bloquer l'interface : <c>_ = simulateur.RejouerAsync(jeton);</c>
    /// </summary>
    public async Task RejouerAsync(CancellationToken jeton = default)
    {
        while (_prochain < _evenements.Count)
        {
            var e = _evenements[_prochain];
            // Attendre jusqu'à l'événement, par pas d'au plus une seconde réelle, en faisant avancer l'horloge
            var pas = TimeSpan.FromSeconds(Acceleration > 0 ? Acceleration : 1);
            while (Acceleration > 0 && e.Heure - HeureSimulee > TimeSpan.Zero)
            {
                var avance = e.Heure - HeureSimulee < pas ? e.Heure - HeureSimulee : pas;
                await Task.Delay(TimeSpan.FromTicks((long)(avance.Ticks / Acceleration)), jeton).ConfigureAwait(false);
                HeureSimulee += avance;
                if (HeureSimulee < e.Heure) HorlogeAvancee?.Invoke(this, HeureSimulee);
            }
            _prochain++;
            HeureSimulee = e.Heure;
            Publier(e);
        }
        Termine?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Rejoue d'un coup tous les événements jusqu'à l'heure donnée incluse (utile pour les tests).
    /// Un appel suivant, ou <see cref="RejouerAsync"/>, reprend là où celui-ci s'est arrêté.
    /// </summary>
    public void AvancerJusqua(TimeSpan heure)
    {
        while (_prochain < _evenements.Count && _evenements[_prochain].Heure <= heure)
        {
            var e = _evenements[_prochain++];
            HeureSimulee = e.Heure;
            Publier(e);
        }
        if (heure > HeureSimulee) HeureSimulee = heure;
        HorlogeAvancee?.Invoke(this, HeureSimulee);
    }

    private void Publier(Evenement e)
    {
        switch (e)
        {
            case ContactArrive x: ContactArrive?.Invoke(this, x); break;
            case ContactDecroche x: ContactDecroche?.Invoke(this, x); break;
            case ContactAbandonne x: ContactAbandonne?.Invoke(this, x); break;
            case ContactClos x: ContactClos?.Invoke(this, x); break;
            case StatutConseillerChange x: StatutConseillerChange?.Invoke(this, x); break;
            case EscaladeSuperviseur x: EscaladeSuperviseur?.Invoke(this, x); break;
        }
        EvenementRejoue?.Invoke(this, e);
    }

    private static Evenement Lire(JsonElement e)
    {
        var heure = TimeSpan.FromSeconds(e.GetProperty("t").GetInt32());
        string S(string nom) => e.GetProperty(nom).GetString()!;
        T En<T>(string nom) where T : struct, Enum => Enum.Parse<T>(S(nom));

        return S("type") switch
        {
            "ContactArrive" => new ContactArrive(heure, S("contactId"), S("file"), En<Canal>("canal"),
                En<Priorite>("priorite"), S("societaireId"), S("motif")),
            "ContactDecroche" => new ContactDecroche(heure, S("contactId"), S("conseillerId")),
            "ContactAbandonne" => new ContactAbandonne(heure, S("contactId")),
            "ContactClos" => new ContactClos(heure, S("contactId"), S("conseillerId"),
                e.GetProperty("satisfaction").GetInt32(), e.GetProperty("resoluPremierContact").GetBoolean()),
            "StatutConseillerChange" => new StatutConseillerChange(heure, S("conseillerId"),
                En<StatutConseiller>("statut")),
            "EscaladeSuperviseur" => new EscaladeSuperviseur(heure, S("contactId"), S("conseillerId"),
                S("societaireId"), S("reclamationId")),
            var t => throw new InvalidDataException($"Type d'événement inconnu : {t}")
        };
    }
}
