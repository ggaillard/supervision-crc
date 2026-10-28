using Mfa.Crc.Supervision.TempsReel;

namespace Mfa.Crc.Supervision.Regles;

/// <summary>Une affectation refusée par une règle de gestion. Le message est destiné au superviseur.</summary>
public sealed class AffectationImpossible(string message) : Exception(message);

/// <summary>Affectation d'un contact en attente à un conseiller (moniteur 1 → moniteur 2).</summary>
public static class ServiceAffectation
{
    /// <summary>
    /// RG2 — parmi les conseillers Disponibles qui ont la compétence de la file du contact,
    /// propose celui qui est disponible depuis le plus longtemps. null si personne ne convient.
    /// </summary>
    /// <remarks>À compléter — séance 4.</remarks>
    public static EtatConseiller? ConseillerPropose(ContactEnAttente contact, IEnumerable<EtatConseiller> plateau)
    {
        throw new NotImplementedException("RG2 — séance 4");
    }
}
