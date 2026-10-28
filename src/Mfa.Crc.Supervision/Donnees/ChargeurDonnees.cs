using System.Text.Json;
using Mfa.Crc.Supervision.Domaine;
using Microsoft.EntityFrameworkCore;

namespace Mfa.Crc.Supervision.Donnees;

/// <summary>
/// Remplit une base vide à partir des fichiers du dossier data/ (conseillers, sociétaires).
/// Ne fait rien si la base contient déjà des conseillers.
/// </summary>
public static class ChargeurDonnees
{
    public static void Charger(CrcContext db, string dossierData)
    {
        if (db.Conseillers.Any()) return;

        using var cons = JsonDocument.Parse(File.ReadAllText(Path.Combine(dossierData, "conseillers.json")));
        var libelles = new Dictionary<string, string>
        {
            ["Sinistres"] = "Sinistres", ["Contrats"] = "Contrats", ["Sante"] = "Santé", ["Accueil"] = "Accueil général"
        };
        foreach (var code in cons.RootElement.GetProperty("files").EnumerateArray().Select(f => f.GetString()!))
        {
            db.Files.Add(new FileTraitement { Code = code, Libelle = libelles.GetValueOrDefault(code, code) });
        }

        foreach (var c in cons.RootElement.GetProperty("conseillers").EnumerateArray())
        {
            db.Conseillers.Add(new Conseiller
            {
                Id = c.GetProperty("id").GetString()!,
                Nom = c.GetProperty("nom").GetString()!,
                Prenom = c.GetProperty("prenom").GetString()!,
                Teletravail = c.GetProperty("teletravail").GetBoolean(),
                Niveau = c.GetProperty("niveau").GetInt32(),
                Competences = string.Join(";", c.GetProperty("competences").EnumerateArray().Select(x => x.GetString()))
            });
        }

        using var soc = JsonDocument.Parse(File.ReadAllText(Path.Combine(dossierData, "societaires.json")));
        foreach (var s in soc.RootElement.GetProperty("societaires").EnumerateArray())
        {
            var societaire = new Societaire
            {
                Id = s.GetProperty("id").GetString()!,
                Nom = s.GetProperty("nom").GetString()!,
                Prenom = s.GetProperty("prenom").GetString()!,
                Ville = s.GetProperty("ville").GetString()!,
                SocietaireDepuis = s.GetProperty("societaireDepuis").GetInt32()
            };
            foreach (var c in s.GetProperty("contrats").EnumerateArray())
            {
                societaire.Contrats.Add(new Contrat
                {
                    Numero = c.GetProperty("numero").GetString()!,
                    Type = c.GetProperty("type").GetString()!,
                    Formule = c.GetProperty("formule").GetString()!,
                    Depuis = c.GetProperty("depuis").GetInt32()
                });
            }
            db.Societaires.Add(societaire);
            foreach (var x in s.GetProperty("sinistres").EnumerateArray())
            {
                db.Sinistres.Add(new Sinistre
                {
                    Id = x.GetProperty("id").GetString()!,
                    ContratNumero = x.GetProperty("contrat").GetString()!,
                    Nature = x.GetProperty("nature").GetString()!,
                    DeclareLe = DateOnly.Parse(x.GetProperty("declareLe").GetString()!),
                    Statut = x.GetProperty("statut").GetString()!,
                    ResponsabiliteRetenue = x.GetProperty("responsabiliteRetenue").ValueKind == JsonValueKind.Number
                        ? x.GetProperty("responsabiliteRetenue").GetInt32() : null
                });
            }
            foreach (var r in s.GetProperty("reclamations").EnumerateArray())
            {
                db.Reclamations.Add(new Reclamation
                {
                    Id = r.GetProperty("id").GetString()!,
                    SocietaireId = societaire.Id,
                    SinistreId = r.GetProperty("sinistre").GetString(),
                    Niveau = r.GetProperty("niveau").GetInt32(),
                    Objet = r.GetProperty("objet").GetString()!,
                    RecueLe = DateOnly.Parse(r.GetProperty("recueLe").GetString()!),
                    AccuseReceptionLe = Date(r, "accuseReceptionLe"),
                    ReponseLe = Date(r, "reponseLe")
                });
            }
        }
        db.SaveChanges();
    }

    private static DateOnly? Date(JsonElement e, string nom) =>
        e.GetProperty(nom).ValueKind == JsonValueKind.String ? DateOnly.Parse(e.GetProperty(nom).GetString()!) : null;

    /// <summary>Cherche le dossier data/ en remontant depuis le dossier de l'application.</summary>
    public static string TrouverDossierData(string depart)
    {
        for (var d = new DirectoryInfo(depart); d != null; d = d.Parent)
        {
            var chemin = Path.Combine(d.FullName, "data");
            if (File.Exists(Path.Combine(chemin, "scenario-matinee.json"))) return chemin;
        }
        throw new DirectoryNotFoundException("Dossier data/ introuvable : lancez l'application depuis le dépôt.");
    }
}
