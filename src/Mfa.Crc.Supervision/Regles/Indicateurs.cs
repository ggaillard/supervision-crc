using Mfa.Crc.Supervision.TempsReel;

namespace Mfa.Crc.Supervision.Regles;

/// <summary>Calcul des indicateurs du moniteur 4 à partir des traces de contacts.</summary>
public static class Indicateurs
{
    /// <summary>Contacts décrochés ÷ contacts terminés (décrochés ou abandonnés), en %. 0 s'il n'y en a aucun.</summary>
    /// <remarks>À compléter — séance 7.</remarks>
    public static double TauxDecroche(IEnumerable<ContactTrace> contacts) =>
        throw new NotImplementedException("séance 7");

    /// <summary>Moyenne de (Clos − Decroche) sur les contacts clos. TimeSpan.Zero s'il n'y en a aucun.</summary>
    public static TimeSpan DureeMoyenneTraitement(IEnumerable<ContactTrace> contacts) =>
        throw new NotImplementedException("séance 7");

    /// <summary>Contacts résolus au premier contact ÷ contacts clos qui ont l'information, en %.</summary>
    public static double ResolutionPremierContact(IEnumerable<ContactTrace> contacts) =>
        throw new NotImplementedException("séance 7");

    /// <summary>Moyenne des notes de satisfaction (1 à 5) laissées.</summary>
    public static double Satisfaction(IEnumerable<ContactTrace> contacts) =>
        throw new NotImplementedException("séance 7");

    /// <summary>
    /// RG3 — contacts décrochés en moins de <paramref name="seuil"/> ÷ contacts reçus, en %, sur les contacts
    /// arrivés dans ]maintenant − fenetre ; maintenant − seuil]. null si aucun contact dans la fenêtre.
    /// <paramref name="file"/> null = toutes les files.
    /// </summary>
    public static double? NiveauService(IEnumerable<ContactTrace> contacts, TimeSpan maintenant,
        TimeSpan fenetre, TimeSpan seuil, string? file = null) =>
        throw new NotImplementedException("RG3 — séance 7");

    /// <summary>
    /// RG3 (alerte) — vrai si le niveau de service est resté sous <paramref name="seuilPourcent"/> à chaque
    /// minute de [maintenant − duree ; maintenant].
    /// </summary>
    public static bool AlerteNiveauService(IEnumerable<ContactTrace> contacts, TimeSpan maintenant, TimeSpan fenetre,
        TimeSpan seuil, double seuilPourcent, TimeSpan duree, string? file = null) =>
        throw new NotImplementedException("RG3 — séance 7");
}
