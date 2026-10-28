namespace Mfa.Crc.Simulateur;

/// <summary>Canal par lequel un sociétaire contacte le CRC.</summary>
public enum Canal { Telephone, Chat, Email, Rappel }

/// <summary>Priorité d'un contact (RG1 : Escalade &gt; Haute &gt; Normale).</summary>
public enum Priorite { Escalade, Haute, Normale }

/// <summary>Statut d'un conseiller sur le plateau.</summary>
public enum StatutConseiller { HorsLigne, Disponible, EnAppel, Chat, PostAppel, Pause, Formation }

/// <summary>Base de tous les événements du scénario.</summary>
/// <param name="Heure">Heure simulée de l'événement.</param>
public abstract record Evenement(TimeSpan Heure);

/// <summary>Un sociétaire entre dans la file d'attente.</summary>
public sealed record ContactArrive(TimeSpan Heure, string ContactId, string File, Canal Canal,
    Priorite Priorite, string SocietaireId, string Motif) : Evenement(Heure);

/// <summary>Un conseiller prend en charge un contact.</summary>
public sealed record ContactDecroche(TimeSpan Heure, string ContactId, string ConseillerId) : Evenement(Heure);

/// <summary>Le sociétaire raccroche avant d'avoir été pris en charge.</summary>
public sealed record ContactAbandonne(TimeSpan Heure, string ContactId) : Evenement(Heure);

/// <summary>Fin de traitement d'un contact, avec la note de satisfaction laissée (1 à 5).</summary>
public sealed record ContactClos(TimeSpan Heure, string ContactId, string ConseillerId,
    int Satisfaction, bool ResoluPremierContact) : Evenement(Heure);

/// <summary>Un conseiller change de statut.</summary>
public sealed record StatutConseillerChange(TimeSpan Heure, string ConseillerId,
    StatutConseiller Statut) : Evenement(Heure);

/// <summary>Un conseiller remonte son contact au superviseur.</summary>
public sealed record EscaladeSuperviseur(TimeSpan Heure, string ContactId, string ConseillerId,
    string SocietaireId, string ReclamationId) : Evenement(Heure);
