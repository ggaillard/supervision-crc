# 🚑 Dépannage — classé par ce que vous voyez à l'écran

> Cherchez (Ctrl+F) un morceau du message d'erreur. Toujours commencer par **lire la première ligne**
> de l'erreur : elle dit presque toujours où regarder (fichier et numéro de ligne).
>
> Une erreur absente de cette page ? Ouvrez une *issue* avec le message complet : elle y sera ajoutée.

---

## Au démarrage

### `Failed to bind to address http://0.0.0.0:5000: address already in use`
L'application tourne déjà dans un autre terminal (souvent lancée avec `dotnet run`). Lancez plutôt avec
`./lancer.sh` ou **Ctrl+Maj+B** : l'ancienne instance est arrêtée d'abord. Ou arrêtez-la seule :
```bash
./lancer.sh arreter
```

### `bash: ./lancer.sh: Permission denied`
```bash
chmod +x lancer.sh
```

### `❌ La compilation échoue`
L'application ne démarre pas tant que le code ne compile pas. Lisez la **première** erreur affichée
(fichier, ligne, code `CS…`) et cherchez ce code plus bas dans cette page.

### Le navigateur ne s'ouvre pas tout seul
Ctrl+clic sur l'adresse `https://…-5000.app.github.dev` affichée par `./lancer.sh`. Ou onglet **PORTS** →
ligne 5000 → 🌐. Si la ligne 5000 manque : attendez `Now listening on: http://0.0.0.0:5000`, puis
**PORTS → Ajouter un port → 5000**.

### La page affiche `localhost a refusé la connexion` · l'adresse est devenue `http://localhost:5000/…`
Le navigateur a été renvoyé vers `localhost`, qui n'existe pas sur votre ordinateur : l'application tourne
dans le Codespace. Deux causes possibles :
- vous avez ouvert vous-même `http://localhost:5000` : utilisez l'adresse `https://…app.github.dev` ;
- les deux lignes `Hebergement.…` ont disparu de `Program.cs` : remettez-les
  (`Hebergement.Preparer(builder);` juste après `CreateBuilder`, `Hebergement.Utiliser(app);` juste après `Build()`).

### Les moniteurs ne se mettent pas à jour, F12 montre une erreur sur `/hubs/crc`
Le Codespace a été mis en veille : rechargez la page (F5 du navigateur). Si l'application s'est arrêtée,
relancez-la.

### `dotnet ef : command not found` · `Could not execute because the specified command or file was not found`
```bash
dotnet tool install --global dotnet-ef
export PATH="$PATH:$HOME/.dotnet/tools"
```

### `SQLite Error 1: 'no such table: …'` · `no such column`
La base a été créée avant votre migration, ou la migration n'a pas été générée. Vérifiez :
```bash
dotnet ef migrations list --project src/Mfa.Crc.Supervision
```
Base de développement seulement : `./lancer.sh neuf` supprime `crc.db` et relance ; les migrations recréent tout.

---

## À la compilation

### `CS0246: The type or namespace name '…' could not be found`
Un `using` manque en haut du fichier (`using Mfa.Crc.Supervision.Regles;`, `using Microsoft.EntityFrameworkCore;`…).
Dans VS Code : curseur sur le mot souligné → **Ctrl+.** propose l'ajout.

### `CS1061: 'string' does not contain a definition for 'Select'` (séance 2)
Du code lit encore l'ancienne chaîne `Competences`. Cherchez toutes les utilisations :
```bash
grep -rn "Competences" src/Mfa.Crc.Supervision --include=*.cs
```

### `The model for context 'CrcContext' has pending changes`
Vous avez modifié une entité ou `OnModelCreating` sans créer de migration : `dotnet ef migrations add …`.

---

## Dans les tests

### `System.NotImplementedException : RG1 — séance 1`
Normal : la méthode a sa signature mais pas encore son corps. C'est la mission à faire.

### Un test du **socle** devient rouge
Vous avez cassé quelque chose qui marchait. Lisez le message, puis `git diff` pour voir ce qui a changé
depuis votre dernier commit.

### `Expected: 2 · Actual: 0` sur un attribut `data-…`
Le test cherche un attribut HTML précis (`data-file="Sinistres" data-nombre="2"`). Comparez avec le guide,
au caractère près, en regardant la source de la page (Ctrl+U).

### `Cannot consume scoped service 'CrcContext' from singleton …`
Un service singleton (`EtatCrc`, `JournalStatuts`, `RelaisTempsReel`) demande un `CrcContext` dans son
constructeur. Injectez `IServiceScopeFactory` et ouvrez un scope (séance 2).

### Les tests de la séance 2 passent, mais `m2_la_migration_transfere…` échoue
Votre migration supprime la colonne avant d'avoir copié les données, ou la jointure ne trouve rien.
Relisez l'ordre de `Up()`.

---

## Dans le navigateur

### `400 Bad Request` en envoyant le formulaire de connexion
Le jeton anti-falsification manque : le formulaire doit être un `<form method="post">` dans une page
Razor (le *tag helper* ajoute le champ caché tout seul).

### `401` sur un appel `fetch` (séance 6)
Normal pour une API sans connexion. Connectez-vous dans la page avant ; `fetch` envoie alors le cookie.

### La console affiche `Failed to start the connection: Error: Failed to complete negotiation … 404`
Le hub n'est pas déclaré : `app.MapHub<HubCrc>("/hubs/crc");` dans `Program.cs`.

### `signalR is not defined`
Le script `~/lib/signalr/signalr.min.js` doit être chargé **avant** `moniteur.js`.

### Le moniteur ne bouge plus
Ouvrez F12 → Console : le hub s'est peut-être déconnecté (application relancée). Rechargez la page.

### `window.getScreenDetails is not a function` (séance 5, bonus)
Seul Chrome (ou Edge) récent propose cette API, et seulement si la page est servie en HTTPS ou sur
`localhost`. Le lanceur doit alors basculer sur le repli en fenêtres.

---

## Avec Git

### `rejected … (fetch first)` au `git push`
Le dépôt GitHub a des commits que vous n'avez pas (une modification faite en ligne) :
```bash
git pull --rebase && git push
```
