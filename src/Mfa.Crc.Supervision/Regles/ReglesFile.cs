using Mfa.Crc.Supervision.TempsReel;

namespace Mfa.Crc.Supervision.Regles;

/// <summary>Règles de la file d'attente (moniteur 1).</summary>
public static class ReglesFile
{
    /// <summary>
    /// RG1 — Escalade, puis Haute, puis Normale ; à priorité égale, celui qui attend
    /// depuis le plus longtemps passe devant.
    /// </summary>
    /// <remarks>À compléter — séance 1.</remarks>
    public static IReadOnlyList<ContactEnAttente> Trier(IEnumerable<ContactEnAttente> contacts)
    {
        throw new NotImplementedException("RG1 — séance 1");
    }
}
