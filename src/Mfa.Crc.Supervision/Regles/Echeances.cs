namespace Mfa.Crc.Supervision.Regles;

/// <summary>Les deux dates limites d'une réclamation.</summary>
public sealed record EcheancesReclamation(DateOnly AccuseReceptionAvant, DateOnly ReponseAvant);

/// <summary>RG5 — délais de traitement des réclamations.</summary>
public static class Echeances
{
    /// <summary>
    /// Accusé de réception sous 10 jours ouvrables après la réception (lundi au vendredi, hors jours fériés),
    /// réponse sous 2 mois calendaires.
    /// </summary>
    /// <remarks>À compléter — séance 7.</remarks>
    public static EcheancesReclamation Calculer(DateOnly recueLe) =>
        throw new NotImplementedException("RG5 — séance 7");

    /// <summary>
    /// Vrai si une échéance non tenue (accusé ou réponse manquant) tombe dans les <paramref name="joursAvant"/>
    /// jours à venir, ou est déjà dépassée.
    /// </summary>
    public static bool EnAlerte(DateOnly recueLe, DateOnly? accuseLe, DateOnly? reponseLe, DateOnly aujourdhui, int joursAvant) =>
        throw new NotImplementedException("RG5 — séance 7");

    /// <summary>Jours fériés en France métropolitaine, 2026 et 2027.</summary>
    public static readonly IReadOnlySet<DateOnly> JoursFeries = new[]
    {
        "2026-01-01", "2026-04-06", "2026-05-01", "2026-05-08", "2026-05-14", "2026-05-25", "2026-07-14",
        "2026-08-15", "2026-11-01", "2026-11-11", "2026-12-25",
        "2027-01-01", "2027-03-29", "2027-05-01", "2027-05-06", "2027-05-08", "2027-05-17", "2027-07-14",
        "2027-08-15", "2027-11-01", "2027-11-11", "2027-12-25"
    }.Select(DateOnly.Parse).ToHashSet();
}
