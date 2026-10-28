using Mfa.Crc.Simulateur;

namespace Mfa.Crc.Supervision.TempsReel;

/// <summary>Un contact qui attend d'être pris en charge.</summary>
public sealed record ContactEnAttente(string Id, string File, Canal Canal, Priorite Priorite,
    string SocietaireId, string Motif, TimeSpan Arrivee);

/// <summary>L'état d'un conseiller sur le plateau.</summary>
public sealed record EtatConseiller(string Id, string NomComplet, IReadOnlySet<string> Competences,
    StatutConseiller Statut, TimeSpan Depuis);

/// <summary>La trace d'un contact (pour les indicateurs).</summary>
public sealed record ContactTrace(string Id, string File, string SocietaireId, TimeSpan Arrivee,
    TimeSpan? Decroche, string? ConseillerId, TimeSpan? Clos, bool Abandonne,
    int? Satisfaction, bool? ResoluPremierContact);

/// <summary>
/// L'état du centre de relation client, tenu en mémoire et mis à jour par les événements
/// du simulateur (demain : de la téléphonie). Une seule instance pour toute l'application.
/// </summary>
public sealed class EtatCrc
{
    private readonly object _verrou = new();
    private readonly List<ContactEnAttente> _attente = [];
    private readonly Dictionary<string, ContactTrace> _contacts = [];
    private readonly Dictionary<string, EtatConseiller> _plateau = [];

    /// <summary>Heure simulée du dernier événement reçu.</summary>
    public TimeSpan Heure { get; private set; } = new(8, 0, 0);

    /// <summary>Déclenché après chaque changement d'état.</summary>
    public event Action<Evenement>? Change;

    /// <summary>Contacts en attente, dans l'ordre où ils sont arrivés.</summary>
    public IReadOnlyList<ContactEnAttente> FileAttente
    {
        get { lock (_verrou) return _attente.ToList(); }
    }

    public IReadOnlyList<EtatConseiller> Plateau
    {
        get { lock (_verrou) return _plateau.Values.OrderBy(c => c.Id).ToList(); }
    }

    public IReadOnlyList<ContactTrace> Contacts
    {
        get { lock (_verrou) return _contacts.Values.ToList(); }
    }

    public EtatConseiller? Conseiller(string id)
    {
        lock (_verrou) return _plateau.GetValueOrDefault(id);
    }

    /// <summary>Place les conseillers sur le plateau, hors ligne, avant le début de la journée.</summary>
    public void Initialiser(IEnumerable<(string Id, string NomComplet, IReadOnlySet<string> Competences)> conseillers)
    {
        lock (_verrou)
        {
            _attente.Clear();
            _contacts.Clear();
            _plateau.Clear();
            foreach (var c in conseillers)
                _plateau[c.Id] = new EtatConseiller(c.Id, c.NomComplet, c.Competences, StatutConseiller.HorsLigne, TimeSpan.Zero);
            Heure = new TimeSpan(8, 0, 0);
        }
    }

    /// <summary>Fait avancer l'heure simulée entre deux événements (les durées continuent de courir).</summary>
    public void AvancerHorloge(TimeSpan heure)
    {
        lock (_verrou)
        {
            if (heure > Heure) Heure = heure;
        }
    }

    public void Appliquer(Evenement e)
    {
        lock (_verrou)
        {
            if (e.Heure > Heure) Heure = e.Heure;
            switch (e)
            {
                case ContactArrive a:
                    _attente.Add(new ContactEnAttente(a.ContactId, a.File, a.Canal, a.Priorite, a.SocietaireId, a.Motif, a.Heure));
                    _contacts[a.ContactId] = new ContactTrace(a.ContactId, a.File, a.SocietaireId, a.Heure,
                        null, null, null, false, null, null);
                    break;
                case ContactDecroche d:
                    _attente.RemoveAll(c => c.Id == d.ContactId);
                    if (_contacts.TryGetValue(d.ContactId, out var t))
                        _contacts[d.ContactId] = t with { Decroche = d.Heure, ConseillerId = d.ConseillerId };
                    break;
                case ContactAbandonne ab:
                    _attente.RemoveAll(c => c.Id == ab.ContactId);
                    if (_contacts.TryGetValue(ab.ContactId, out var ta))
                        _contacts[ab.ContactId] = ta with { Abandonne = true };
                    break;
                case ContactClos cl:
                    if (_contacts.TryGetValue(cl.ContactId, out var tc))
                        _contacts[cl.ContactId] = tc with
                        {
                            Clos = cl.Heure,
                            Satisfaction = cl.Satisfaction,
                            ResoluPremierContact = cl.ResoluPremierContact
                        };
                    break;
                case StatutConseillerChange s:
                    if (_plateau.TryGetValue(s.ConseillerId, out var c))
                        _plateau[s.ConseillerId] = c with { Statut = s.Statut, Depuis = s.Heure };
                    break;
            }
        }
        Change?.Invoke(e);
    }
}
