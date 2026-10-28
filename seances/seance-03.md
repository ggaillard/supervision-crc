# 3️⃣ Séance 3 — Moniteur 2 : le plateau des conseillers

> **Durée** 2 h · **Tests** `dotnet test --filter Groupe=seance-03` · **Fiche de cours** [Configuration et règles de gestion](../cours/03-configuration-regles.md)

## 🎯 Objectifs

- Construire une page Razor à partir de l'état en mémoire
- Écrire la règle **RG4** (durées) comme une fonction pure, puis l'afficher
- Sortir les **seuils** du code : la configuration (`appsettings.json`) et le modèle *Options*

## 🎬 La situation

> **De :** Claire Morel
> **Objet :** le moniteur 2
>
> Merci pour la file, c'est déjà beaucoup plus clair. Le deuxième moniteur doit me montrer **mes
> 18 conseillers** : leur statut, depuis combien de temps, et ce qu'ils savent traiter.
> Deux situations doivent sauter aux yeux : un **appel qui dure plus de 10 minutes** (le conseiller a
> peut-être besoin d'aide) et une **pause de plus de 10 minutes**. Ces 10 minutes, je veux pouvoir
> les changer sans vous rappeler : l'été, on passe à 15.

RG4, [cahier des charges](../docs/cahier-des-charges.md) : appel (téléphone **ou** chat) ou pause au-delà du seuil.

---

## 1 ▶️ Lancer

Lancez l'application, attendez 09 h 30 de l'heure simulée. Il n'existe pas encore de moniteur 2 :
le lien de l'accueil est grisé. Regardez `EtatCrc.Plateau` : que contient un `EtatConseiller` ?

## 2 🔍 Comprendre

1. `Depuis` contient l'heure du dernier changement de statut. Comment obtenir la **durée** du statut actuel ?
2. Pourquoi un conseiller en **post-appel** (il rédige son compte rendu) ne doit-il pas déclencher l'alerte « appel long » ?
3. « 10 minutes » écrit en dur dans le code : que faut-il faire pour passer à 15 l'été ? Et avec la configuration ?
4. `ReglesDuree.Alertes` reçoit `maintenant` en paramètre au lieu de lire l'horloge elle-même. Qu'est-ce que cela change pour les tests ?

## 3 ✏️ Modifier

### Mission 3.1 🟢 — La page du plateau

**Attendu :** `/moniteur/2` affiche une carte par conseiller :
`<article data-conseiller="C01" data-statut="EnAppel" data-alerte="false">` avec son nom, son statut,
la durée du statut dans un élément `data-duree` au format `mm:ss`, et ses compétences.
L'accueil mène à cette page.

1. Créez `Pages/Moniteurs/Plateau.cshtml` (`@page "/moniteur/2"`) et `Plateau.cshtml.cs`, sur le modèle de la file d'attente :
   ```csharp
   public class PlateauModel(EtatCrc etat) : PageModel
   {
       public IReadOnlyList<EtatConseiller> Conseillers { get; private set; } = [];
       public TimeSpan Maintenant { get; private set; }

       public void OnGet()
       {
           Maintenant = etat.Heure;
           Conseillers = etat.Plateau;
       }

       public string Duree(EtatConseiller c) =>
           c.Statut == StatutConseiller.HorsLigne ? "—" : (Maintenant - c.Depuis).ToString(@"mm\:ss");
   }
   ```
2. Dans la vue, une boucle `@foreach` qui écrit les `<article>` dans un `<div class="plateau">`
   (les styles `plateau`, `carte`, `statut-EnAppel`… sont prêts dans `site.css`).
3. Dans `Pages/Index.cshtml`, rendez le lien du moniteur 2 actif ; ajoutez-le au menu de `_Layout.cshtml`.

✅ `dotnet test --filter "Groupe=seance-03&FullyQualifiedName~m1"`

### Mission 3.2 🟢 — RG4, avec des seuils configurables

1. **La règle.** Complétez `Regles/ReglesDuree.cs` :
   ```csharp
   var appelLong = TimeSpan.FromMinutes(seuils.AppelLongMinutes);
   var pauseLongue = TimeSpan.FromMinutes(seuils.PauseLongueMinutes);
   return plateau
       .Select(c => new AlerteDuree(c.Id, c.Statut, maintenant - c.Depuis))
       .Where(a => (a.Statut is StatutConseiller.EnAppel or StatutConseiller.Chat && a.Duree > appelLong)
                || (a.Statut == StatutConseiller.Pause && a.Duree > pauseLongue))
       .OrderByDescending(a => a.Duree)
       .ToList();
   ```
2. **La configuration.** Dans `appsettings.json` :
   ```json
   "Supervision": {
     "Seuils": { "AppelLongMinutes": 10, "PauseLongueMinutes": 10 }
   },
   ```
   et dans `Program.cs`, avant `builder.Build()` :
   ```csharp
   builder.Services.Configure<SeuilsAlerte>(builder.Configuration.GetSection("Supervision:Seuils"));
   ```
3. **La page.** Injectez `IOptions<SeuilsAlerte> seuils` dans `PlateauModel`, calculez une fois les
   alertes dans `OnGet()`, et écrivez `data-alerte="true"` (et la classe `en-alerte`) sur les cartes concernées.

✅ `dotnet test --filter "Groupe=seance-03&FullyQualifiedName~m2"`

> Le test `m2_les_seuils_viennent_de_la_configuration` démarre l'application avec un seuil de 5 minutes :
> il échoue si le 10 est resté écrit en dur quelque part.

### Mission 3.3 🟡 — Compter et filtrer par statut

**Attendu :**
- au-dessus des cartes, un lien par statut (les 7, même à 0) : `<a href="/moniteur/2?statut=Pause" data-statut-compteur="Pause" data-nombre="1">` ;
- `/moniteur/2?statut=Pause` n'affiche que les conseillers en pause.

> 💡 **Indice.** `Enum.GetValues<StatutConseiller>()` donne tous les statuts. Pour lire `?statut=` :
> une propriété `public StatutConseiller? Statut { get; set; }` avec `[BindProperty(SupportsGet = true)]`
> — ASP.NET Core convertit le texte en énumération tout seul.

✅ `dotnet test --filter Groupe=seance-03`

### Mission 3.4 🔴 — Trier par durée

**Attendu :** `/moniteur/2?tri=duree` met en tête les statuts les plus anciens (les conseillers hors
ligne à la fin). *(test : `--filter "Groupe=bonus&FullyQualifiedName~s3_m5"`)*

## 4 ✅ Valider

```bash
dotnet test --filter Groupe=seance-03 && dotnet test --filter Groupe=socle
git add -A && git commit -m "feat: moniteur 2, alertes de durée (RG4) configurables" && git push
```

---

## 📌 Concepts à connaître

1. **Fonction pure** — mêmes entrées, même résultat, aucun effet de bord : `ReglesDuree.Alertes(plateau, maintenant, seuils)` se teste sans l'application.
2. **Configuration** — une valeur qui change selon le contexte (seuil, objectif) vit dans `appsettings.json`, pas dans le code.
3. **Options** — `Configure<T>(section)` relie une section de configuration à une classe ; on l'injecte avec `IOptions<T>`.
4. **Liaison de la query string** — `[BindProperty(SupportsGet = true)]` remplit une propriété avec `?statut=Pause`.
5. **`TimeSpan`** — une durée ; la différence de deux heures en est une, et se formate avec `ToString(@"mm\:ss")`.

## 📓 Journal de fin de séance (portail)

Ce que j'ai fait · ce qui m'a bloqué · ce que j'ai compris aujourd'hui.
