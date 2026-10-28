# 7️⃣ Séance 7 — Moniteur 4 : indicateurs et alertes

> **Durée** 2 h · **Tests** `dotnet test --filter Groupe=seance-07` · **Fiche de cours** [Indicateurs et dates](../cours/07-indicateurs.md)

## 🧭 Feuille de route

> 🎯 **Pourquoi cette séance ?** La direction suit quatre chiffres chaque jour, recopiés à la main d'un tableur. Le moniteur 4 doit les calculer en direct et alerter avant qu'il soit trop tard.
> **Pour le métier :** traduire une **définition métier** en calcul exact : un indicateur faux, c'est une mauvaise décision de la direction ; une échéance ratée, c'est une sanction du régulateur.
>
> **À la fin, vous saurez :** définir un indicateur sans ambiguïté (numérateur, dénominateur, exclus) · calculer sur une fenêtre glissante (RG3) et en jours ouvrables (RG5) · lever des alertes, les acquitter, ne jamais les effacer (RG6)

### 📖 À lire — on lit, on ne produit rien

| Quand | Quoi | Temps |
|---|---|---|
| **avant** la séance | la fiche [Indicateurs et dates](../cours/07-indicateurs.md), puis son auto-évaluation | 7 min |
| au début | [la situation](#-la-situation) : le message de Mme Morel ; la section *Indicateurs*, RG3, RG5 et RG6 dans le [cahier des charges](../docs/cahier-des-charges.md) | 5 min |
| avant de modifier | le code existant : `Regles/Indicateurs.cs`, `Regles/Echeances.cs`, `appsettings.json` | 10 min |

### ✏️ À faire — chaque ligne laisse une trace que l'on peut vérifier

*Chemins depuis `src/Mfa.Crc.Supervision/` · 🆕 fichier à créer · 🟢 guidé · 🟡 semi-guidé · 🔴 facultatif.*

| Mission | | Vous produisez | Où | Preuve |
|---|---|---|---|---|
| 2 · Comprendre | 🟢 | les 4 fractions + 4 réponses écrites (dont un calcul de date) | `CARNET.md` | interrogation n° 2 |
| 7.1 | 🟢 | les 4 indicateurs et `GET /api/indicateurs` | `Regles/Indicateurs.cs`, `Api/ApiIndicateurs.cs` 🆕, `Program.cs` | tests `m1` |
| 7.2 | 🟡 | RG3 : le niveau de service sur 30 minutes | `Regles/Indicateurs.cs`, `appsettings.json` | tests `m2` |
| 7.3 | 🟡 | RG5 : les échéances des réclamations | `Regles/Echeances.cs`, `Api/ApiIndicateurs.cs` | tests `m3` |
| 7.4 | 🟢 | RG6 : alertes acquittées, jamais effacées ; le moniteur 4 | `Domaine/Entites.cs`, `Regles/ServiceAlertes.cs` 🆕, `Pages/Moniteurs/Indicateurs.cshtml` 🆕 et `.cs` | tests `m4` |
| 7.5 | 🔴 | les indicateurs d'une seule file | `Api/ApiIndicateurs.cs` | bonus `s7_m5` |
| **Fin** | ✅ | commit + push · cases cochées dans [`PROGRESSION.md`](../PROGRESSION.md) · journal au portail | dépôt, portail | onglet *Actions* vert |

### 🧠 À retenir — interrogé dans l'interrogation n° 2 (début de la séance 8)

**Indicateur** · **Fenêtre glissante** · **Jours ouvrables** · **Idempotence** · **Traçabilité (RG6)** — définitions en [fin de séance](#-à-retenir).

---

## 📖 La situation

> **De :** Claire Morel
> **Objet :** des chiffres pour la direction
>
> La direction suit quatre chiffres chaque jour : le **taux de décroché**, la **durée moyenne de
> traitement**, la **résolution au premier contact** et la **satisfaction**. Aujourd'hui je les recopie
> d'un tableur. Le moniteur 4 doit les calculer en direct.
> Deux alertes comptent plus que tout : une file dont le **niveau de service** reste sous 80 % pendant
> 10 minutes, et une **réclamation** dont l'échéance réglementaire approche. Quand je prends une alerte
> en charge, je l'**acquitte** ; on doit pouvoir retrouver plus tard qui l'a vue et quand.

Les définitions exactes sont dans le [cahier des charges](../docs/cahier-des-charges.md) (indicateurs, RG3, RG5, RG6).

---

## 1 ▶️ Lancer

Ouvrez le cahier des charges, section *Indicateurs*. Pour chaque indicateur, écrivez la fraction dans [`CARNET.md`](../CARNET.md) :
qu'y a-t-il au numérateur, au dénominateur ? Un contact **encore en attente** compte-t-il ?

## 2 🔍 Comprendre

> ✏️ Répondez dans [`CARNET.md`](../CARNET.md), section *Séance 7* : quelques lignes, avec vos mots.

1. Pourquoi le niveau de service se calcule-t-il sur les **30 dernières minutes** plutôt que depuis 8 h ?
2. Un contact arrivé il y a 5 secondes et pas encore décroché : à temps ou en retard ? Que faire de lui ?
3. Une réclamation reçue le jeudi 5 novembre 2026 : à quelle date limite l'accusé de réception (10 jours ouvrables) ? Comptez sur un calendrier.
4. Pourquoi ne pas supprimer une alerte acquittée ? Qui pourrait en avoir besoin, et quand ?

## 3 ✏️ Modifier

### Mission 7.1 🟢 — Les indicateurs de la journée

1. Complétez `Regles/Indicateurs.cs`, en commençant par :
   ```csharp
   public static double TauxDecroche(IEnumerable<ContactTrace> contacts)
   {
       var termines = contacts.Where(c => c.Decroche is not null || c.Abandonne).ToList();
       return termines.Count == 0 ? 0 : 100.0 * termines.Count(c => c.Decroche is not null) / termines.Count;
   }
   ```
   puis `DureeMoyenneTraitement` (moyenne de `Clos − Decroche` sur les contacts clos),
   `ResolutionPremierContact` et `Satisfaction`, en suivant les commentaires `///`.
2. Une API `GET /api/indicateurs` (fichier `Api/ApiIndicateurs.cs`) qui renvoie
   `tauxDecroche`, `dureeMoyenneSecondes`, `resolutionPremierContact`, `satisfaction` à partir de `etat.Contacts`.

✅ `dotnet test --filter "Groupe=seance-07&FullyQualifiedName~m1"`

> Le test `m1_l_api_des_indicateurs_en_fin_de_matinee` rejoue la vraie matinée : à 12 h, vous devez
> trouver un taux de décroché de **95,8 %**.

### Mission 7.2 🟡 — RG3 : le niveau de service

**Attendu :**
- `Indicateurs.NiveauService(contacts, maintenant, fenetre, seuil, file)` : contacts décrochés en moins
  de 20 s ÷ contacts **arrivés** dans `]maintenant − 30 min ; maintenant − 20 s]` ; `null` si aucun ;
- `Indicateurs.AlerteNiveauService(...)` : vrai si le niveau est resté sous 80 % **à chaque minute** des 10 dernières ;
- `/api/indicateurs` ajoute `niveauxService` : une entrée par file `{ file, valeur, alerte }` ;
- les paramètres (30, 20, 80, 10) viennent d'une section `Supervision:Indicateurs` de `appsettings.json`.

> 💡 **Indices.** Pourquoi `maintenant − 20 s` ? Relisez la question 2. Pour « à chaque minute » :
> une boucle `for (var t = maintenant - duree; t <= maintenant; t += TimeSpan.FromMinutes(1))` qui
> s'arrête dès qu'une minute est au-dessus du seuil. Une classe d'options `OptionsIndicateurs`, comme `SeuilsAlerte`.

✅ `dotnet test --filter "Groupe=seance-07&FullyQualifiedName~m2"`

### Mission 7.3 🟡 — RG5 : les échéances des réclamations

**Attendu :**
- `Echeances.Calculer(recueLe)` : accusé de réception **10 jours ouvrables** après la réception (ni samedi,
  ni dimanche, ni jour férié — la liste est dans `Echeances.JoursFeries`), réponse **2 mois** après ;
- `Echeances.EnAlerte(...)` : une échéance non tenue tombe dans les 5 jours, ou est dépassée ;
- `GET /api/reclamations/echeances?date=2026-10-28` : une entrée par réclamation sans réponse,
  `{ reclamationId, accuseReceptionAvant, reponseAvant, alerte }`, dates au format `yyyy-MM-dd`.

> 💡 **Indices.** Avancez jour par jour avec `AddDays(1)` et ne comptez que les jours ouvrables.
> `DateOnly.AddMonths(2)` gère les fins de mois. Un paramètre `DateOnly? date` dans l'API se lit dans la
> query string ; par défaut, le jour simulé (`Simulation:Date` dans la configuration).

✅ `dotnet test --filter "Groupe=seance-07&FullyQualifiedName~m3"`

### Mission 7.4 🟢 — RG6 : des alertes acquittées, jamais effacées

1. **L'entité** `Alerte` : `Id`, `Type` (`AppelLong`, `PauseLongue`, `NiveauService`, `EcheanceReclamation`),
   `Cible` (le conseiller, la file ou la réclamation), `Message`, `LeveeA`, `AcquitteeA` (nullable),
   `AcquitteeParId` (nullable, clé étrangère vers `Superviseurs`). Une migration.
2. **Lever les alertes** : un service `Regles/ServiceAlertes.cs` (*scoped*) dont la méthode `Generer()`
   - appelle `ReglesDuree.Alertes` (RG4), `Indicateurs.AlerteNiveauService` par file (RG3), `Echeances.EnAlerte` par réclamation (RG5) ;
   - **n'ajoute pas** une alerte si une alerte du même `Type` sur la même `Cible` est encore ouverte ;
   - enregistre les nouvelles et les renvoie.
3. **L'API**, groupe `/api/alertes` :
   ```csharp
   alertes.MapPost("/generer", (ServiceAlertes s) => Results.Ok(s.Generer().Select(a => new { a.Id, a.Type, a.Cible, a.Message })));
   alertes.MapGet("", (CrcContext db) => /* toutes, les ouvertes d'abord */);
   alertes.MapPost("/{id:int}/acquitter", (int id, CrcContext db, ClaimsPrincipal utilisateur) =>
   {
       var alerte = db.Alertes.Find(id);
       if (alerte is null) return Results.Problem("…", statusCode: 404);
       if (alerte.AcquitteeA is not null) return Results.Problem("Cette alerte est déjà acquittée.", statusCode: 409);
       alerte.AcquitteeA = DateTime.UtcNow;
       alerte.AcquitteeParId = /* le superviseur connecté */;
       db.SaveChanges();
       /* une ligne « Acquittement » dans le journal, cible : l'id de l'alerte */
       return Results.NoContent();
   });
   ```
   **Aucune** route `DELETE` : c'est RG6.
4. **Le moniteur 4** (`/moniteur/4`) : quatre tuiles `data-indicateur="tauxDecroche"` (puis `dureeMoyenne`,
   `resolutionPremierContact`, `satisfaction`), le niveau de service par file, la liste des alertes
   (`data-alerte-id`), un bouton `data-acquitter` par alerte ouverte.

✅ `dotnet test --filter Groupe=seance-07`

> En fonctionnement réel, `Generer()` doit tourner toute seule : un `BackgroundService` avec un
> `PeriodicTimer` de 15 secondes, qui ouvre un *scope* à chaque tour (séance 2), et qui ne démarre pas
> quand `Simulation:Active` vaut `false` (les tests).

### Mission 7.5 🔴 — Les indicateurs d'une seule file

**Attendu :** `/api/indicateurs?file=Sinistres` calcule les quatre indicateurs sur cette seule file.
*(test : `--filter "Groupe=bonus&FullyQualifiedName~s7_m5"`)*

## 4 ✅ Valider

```bash
dotnet test
git add -A && git commit -m "feat: moniteur 4, indicateurs, RG3, RG5, alertes acquittées" && git push
```

---

## 🧠 À retenir

1. **Indicateur** — une fraction définie sans ambiguïté : ce qui compte au numérateur, au dénominateur, et ce qui est exclu.
2. **Fenêtre glissante** — on ne regarde que les N dernières minutes : l'indicateur réagit vite et oublie la matinée.
3. **Jours ouvrables** — ni week-end ni jour férié : on avance jour par jour en ne comptant que ceux-là.
4. **Idempotence** — relancer `Generer()` ne duplique rien : une alerte ouverte n'est pas levée deux fois.
5. **Traçabilité (RG6)** — on acquitte (qui, quand), on ne supprime pas : l'historique sert en cas de litige ou d'audit.

## ✏️ Journal de fin de séance (portail)

Ce que j'ai fait · ce qui m'a bloqué · ce que j'ai compris aujourd'hui.
