namespace Mfa.Crc.Supervision.Domaine;

/// <summary>Une file de traitement du CRC : Sinistres, Contrats, Sante, Accueil.</summary>
public class FileTraitement
{
    public string Code { get; set; } = "";
    public string Libelle { get; set; } = "";
}

/// <summary>Un conseiller du plateau.</summary>
public class Conseiller
{
    public string Id { get; set; } = "";          // C01 … C18
    public string Nom { get; set; } = "";
    public string Prenom { get; set; } = "";
    public bool Teletravail { get; set; }

    /// <summary>1 = conseiller, 2 = référent.</summary>
    public int Niveau { get; set; } = 1;

    /// <summary>
    /// Les files que le conseiller sait traiter, séparées par des points-virgules : « Sinistres;Contrats ».
    /// Choix du prestataire pour aller vite.
    /// </summary>
    public string Competences { get; set; } = "";

    public string NomComplet => $"{Prenom} {Nom}";
}

/// <summary>Un sociétaire (client et membre de la mutuelle).</summary>
public class Societaire
{
    public string Id { get; set; } = "";          // S001 …
    public string Nom { get; set; } = "";
    public string Prenom { get; set; } = "";
    public string Ville { get; set; } = "";
    public int SocietaireDepuis { get; set; }

    public List<Contrat> Contrats { get; set; } = [];
    public List<Reclamation> Reclamations { get; set; } = [];
}

public class Contrat
{
    public string Numero { get; set; } = "";
    public string SocietaireId { get; set; } = "";
    public Societaire? Societaire { get; set; }
    public string Type { get; set; } = "";         // Auto, Habitation, Santé
    public string Formule { get; set; } = "";
    public int Depuis { get; set; }

    public List<Sinistre> Sinistres { get; set; } = [];
}

public class Sinistre
{
    public string Id { get; set; } = "";
    public string ContratNumero { get; set; } = "";
    public Contrat? Contrat { get; set; }
    public string Nature { get; set; } = "";
    public DateOnly DeclareLe { get; set; }
    public string Statut { get; set; } = "";
    public int? ResponsabiliteRetenue { get; set; }
}

public class Reclamation
{
    public string Id { get; set; } = "";
    public string SocietaireId { get; set; } = "";
    public Societaire? Societaire { get; set; }
    public string? SinistreId { get; set; }
    public int Niveau { get; set; }
    public string Objet { get; set; } = "";
    public DateOnly RecueLe { get; set; }
    public DateOnly? AccuseReceptionLe { get; set; }
    public DateOnly? ReponseLe { get; set; }
}
