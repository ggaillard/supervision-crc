using Mfa.Crc.Simulateur;
using Mfa.Crc.Supervision.TempsReel;

namespace Mfa.Crc.Supervision.Regles;

/// <summary>Seuils d'alerte de durée, lus dans la configuration (section Supervision:Seuils).</summary>
public sealed class SeuilsAlerte
{
    public int AppelLongMinutes { get; set; } = 10;
    public int PauseLongueMinutes { get; set; } = 10;
}

/// <summary>Un conseiller qui dépasse un seuil.</summary>
public sealed record AlerteDuree(string ConseillerId, StatutConseiller Statut, TimeSpan Duree);

/// <summary>Règles de durée sur le plateau (moniteur 2).</summary>
public static class ReglesDuree
{
    /// <summary>
    /// RG4 — un conseiller en appel (EnAppel ou Chat) depuis plus de AppelLongMinutes, ou en pause
    /// depuis plus de PauseLongueMinutes, déclenche une alerte. Les alertes sont triées de la plus
    /// longue durée à la plus courte.
    /// </summary>
    /// <remarks>À compléter — séance 3.</remarks>
    public static IReadOnlyList<AlerteDuree> Alertes(IEnumerable<EtatConseiller> plateau, TimeSpan maintenant, SeuilsAlerte seuils)
    {
        throw new NotImplementedException("RG4 — séance 3");
    }
}
