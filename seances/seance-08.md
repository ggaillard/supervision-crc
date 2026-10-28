# 8️⃣ Séance 8 — Avant la mise en service

> **Durée** 2 h (dont 30 min d'interrogation) · **Tests** `dotnet test --filter Groupe=seance-08` · **Fiche de cours** [Qualité et exploitation](../cours/08-qualite-exploitation.md)

> ✍️ **0 – 30 min : interrogation écrite n° 2** (séances 5 à 7), sur papier, sans document.

## 🎯 Objectifs

- **Auditer** une application avec une grille, trouver une faille, la prouver, la corriger
- Écrire **vos** tests : un test qui ne peut pas échouer ne protège rien
- Préparer l'**exploitation** : configuration de production, page de santé, recette avec la demandeuse
- Présenter le travail dans votre **dossier E5**

## 🎬 La situation

> **De :** Claire Morel
> **Objet :** lundi, on bascule
>
> Lundi, la supervision remplace le tableur. Avant, le responsable de la sécurité veut un **audit** : il
> se méfie du code du prestataire. Le service d'exploitation demande une adresse à surveiller pour savoir
> si l'application est **vivante**. Et je voudrais une **recette** : on rejoue la matinée du 28 octobre
> ensemble, et je vérifie que les moniteurs disent ce que j'attends.

---

## 1 ▶️ Lancer

```bash
dotnet test     # où en êtes-vous ? une ligne par séance dans l'onglet Actions aussi
```

## 2 🔍 Comprendre — la grille d'audit

Pour chaque ligne : **où regarder** dans le code, **ce que vous avez trouvé**, **verdict** (✅ / ⚠️ / ❌).

| # | Risque (OWASP 2021) | Question à se poser | Où regarder |
|---|---|---|---|
| 1 | A01 Contrôle d'accès | Une page ou une API est-elle accessible sans connexion ? | `Program.cs`, `[AllowAnonymous]` |
| 2 | A02 Cryptographie | Un mot de passe est-il stocké ou journalisé en clair ? | `Superviseurs`, `JournalActions` |
| 3 | **A03 Injection** | Une saisie de l'utilisateur est-elle **collée** dans une requête SQL ? | `grep -rn "FromSqlRaw\|ExecuteSqlRaw" src/` |
| 4 | A04 Conception | Une règle de gestion est-elle vérifiée seulement dans le navigateur ? | `wwwroot/js/`, `Api/` |
| 5 | A05 Configuration | En production, une erreur affiche-t-elle la pile d'appels ? | `Program.cs`, `ASPNETCORE_ENVIRONMENT` |
| 6 | A07 Authentification | Le message d'échec révèle-t-il si un identifiant existe ? | `Pages/Connexion.cshtml.cs` |
| 7 | A09 Journalisation | Peut-on dire qui a affecté un appel, acquitté une alerte ? | `JournalActions` |

## 3 ✏️ Modifier

### Mission 8.1 🟡 — Trouver, prouver, corriger la faille

Le prestataire a laissé une faille dans l'application, présente depuis le premier jour. La grille
vous dit où chercher ; le test ne vous dit pas où elle est.

1. **Trouvez**-la (ligne 3 de la grille).
2. **Prouvez**-la : quelle saisie fait afficher **tous** les sociétaires ? Et cette saisie-ci, que montre-t-elle ?
   `%' UNION SELECT Id, Login, Login, Login, 0 FROM Superviseurs --`
3. **Corrigez**-la : la saisie ne doit **jamais** être collée dans le texte SQL. Deux corrections justes :
   LINQ (`.Where(s => EF.Functions.Like(s.Nom, motif))`) ou une requête paramétrée (`FromSql($"…{motif}…")`,
   interpolée : EF Core en fait un paramètre).
4. Écrivez dans votre journal ce que la faille permettait, et pourquoi votre correction la ferme.

✅ `dotnet test --filter "Groupe=seance-08&FullyQualifiedName~m1"`

> Les caractères `%` et `_` gardent un sens spécial dans `LIKE`, même paramétrés : un nom contenant `%`
> renverrait tout. Neutralisez-les avec un caractère d'échappement (troisième argument de `EF.Functions.Like`).

### Mission 8.2 🟢 — Vos propres tests

**Attendu :** au moins **deux** tests dans `tests/Mfa.Crc.Tests/Etudiant/`, classe marquée
`[Trait("Groupe", "etudiant")]`, héritant de `TestCrc`. Chacun protège une règle **que les tests fournis ne vérifient pas**.

La méthode, pour chaque test :
1. écrivez-le, lancez-le : **vert** ;
2. **cassez** volontairement le code qu'il protège (`>` devient `>=`, une condition supprimée…) : il doit passer au **rouge** ;
3. remettez le code : **vert**. Notez dans le commentaire du test la modification qui le fait échouer.

Idées : un chat long déclenche aussi RG4 ; une affectation refusée ne change rien au plateau ; une
réclamation déjà répondue n'est jamais en alerte ; un accusé de réception reçu un vendredi…

✅ `dotnet test --filter "Groupe=seance-08&FullyQualifiedName~m2"` puis `--filter Groupe=etudiant`

### Mission 8.3 🟢 — Le style du code

```bash
dotnet format --verify-no-changes     # liste les écarts au style du projet (.editorconfig)
dotnet format                         # les corrige
```

La ligne **🎨 Style du code** de l'onglet *Actions* doit être verte.

### Mission 8.4 🟡 — Prête pour l'exploitation

**Attendu :**
- `GET /sante`, **sans connexion**, répond `200` avec `{ "statut": "ok", "base": "ok", "heureSimulee": "…", "version": "…" }` ;
- en environnement **Production**, une requête mal formée (`/api/reclamations/echeances?date=pas-une-date`)
  répond **400** en `application/problem+json`, sans aucun détail technique.

> 💡 **Indices.** `.AllowAnonymous()` sur une route minimale. `db.Database.CanConnect()` pour la base.
> `app.UseStatusCodePages()` donne un corps `problem+json` aux réponses d'erreur vides ; vérifiez que la
> page d'erreur détaillée n'est active **qu'en** développement.

Puis publiez et essayez comme en production :

```bash
dotnet publish src/Mfa.Crc.Supervision -c Release -o publication
ASPNETCORE_ENVIRONMENT=Production ASPNETCORE_URLS=http://0.0.0.0:5000 dotnet publication/Mfa.Crc.Supervision.dll
```

**La recette avec Mme Morel.** Le mode recette rejoue la matinée d'un coup jusqu'à une heure donnée, puis
fige les moniteurs :

```bash
Simulation__ArreterA=11:42:00 dotnet run --project src/Mfa.Crc.Supervision     # puis 12:00:00
```

Vérifiez avec elle, moniteur par moniteur :

| Heure simulée | Moniteur | Ce qu'elle attend |
|---|---|---|
| 11:42 | 1 | 7 contacts en attente, tous *Sinistres* ; aucun ne peut être affecté (pas de conseiller Sinistres disponible) |
| 11:42 | 2 | Karim Belaïd en appel depuis plus de 10 minutes, en alerte |
| 11:42 | 3 | Karim sélectionné : le dossier d'Hélène ROCHE, sa réclamation REC-FICT-0042 |
| 12:00 | 4 | taux de décroché 95,8 % · satisfaction 4,00 / 5 · réclamation REC-FICT-0039 en alerte |

✅ `dotnet test --filter Groupe=seance-08`

### Mission 8.5 🟢 — La fiche de réalisation E5

Complétez [e5/FICHE-REALISATION.md](../e5/FICHE-REALISATION.md) : la demande, ce que vous avez livré, une
difficulté et comment vous l'avez résolue, les preuves. Une compétence ne se coche que si vous pouvez la **montrer**.

## 4 ✅ Valider

```bash
dotnet test && dotnet format --verify-no-changes
git add -A && git commit -m "fix: injection SQL ; feat: santé, production, tests personnels" && git push
```

Toutes les lignes de l'onglet **Actions** au vert. Écrivez un journal-bilan des huit séances.

---

## 📌 Concepts à connaître

1. **Injection SQL** — une saisie collée dans le texte SQL devient du code ; une requête paramétrée la traite comme une donnée.
2. **Test qui protège** — un test vaut s'il échoue quand on casse la règle : on le vérifie en la cassant.
3. **Environnement** — Development affiche les détails pour le développeur ; Production ne montre rien de technique.
4. **Sonde de santé** — une adresse publique, légère, qui dit si l'application et sa base répondent.
5. **Recette** — la demandeuse vérifie, sur un scénario connu, que le résultat est celui qu'elle attend.

## 📓 Journal de fin de séance (portail)

Le journal-bilan : ce que j'ai livré · ce qui m'a le plus appris · ce que je referais autrement.
