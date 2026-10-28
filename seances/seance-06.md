# 6️⃣ Séance 6 — Le moniteur 3 et la sécurité

> **Durée** 2 h · **Tests** `dotnet test --filter Groupe=seance-06` · **Fiche de cours** [Authentification, autorisation, traçabilité](../cours/06-securite.md)

## 🎯 Objectifs

- **Authentifier** un superviseur sans jamais stocker son mot de passe
- **Protéger** toute l'application d'un coup, puis ouvrir explicitement ce qui doit rester public
- Afficher le **dossier** d'un sociétaire et **tracer** qui a fait quoi

## 🎬 La situation

> **De :** Claire Morel
> **Objet :** n'importe qui peut ouvrir la supervision !
>
> Un intérimaire a ouvert l'adresse de la supervision depuis son poste… et a vu les dossiers des
> sociétaires. Il faut une **connexion** pour les superviseurs. Je suis la première : identifiant
> `cmorel`, mot de passe `Supervision2026!` (compte de démonstration, à changer avant la mise en service).
> Le troisième moniteur doit enfin servir : quand je sélectionne un conseiller en appel, je veux le
> **dossier** du sociétaire qu'il a en ligne — contrats, sinistres, réclamations — et pouvoir y
> laisser une **note**. Et je dois pouvoir dire, en cas de litige, **qui** a affecté tel appel ou écrit telle note.

---

## 1 ▶️ Lancer

Ouvrez une fenêtre de navigation privée sur le port 5000 : rien ne vous est demandé. Essayez aussi
`/api/selection` et `/societaires?nom=roche`.

## 2 🔍 Comprendre

1. Pourquoi ne jamais stocker un mot de passe, même chiffré ? Que stocke-t-on à la place ?
2. Un identifiant inconnu et un mauvais mot de passe : faut-il le même message ? Pourquoi ?
3. **Authentification** et **autorisation** : quelle différence ?
4. Une page protégée renvoie vers le formulaire de connexion (302). Que doit renvoyer une **API** protégée, et pourquoi pas une redirection ?
5. Le journal des actions est-il une donnée personnelle ? Que faut-il y mettre, et ne pas y mettre ?

## 3 ✏️ Modifier

### Mission 6.1 🟢 — Les superviseurs et la connexion

1. **L'entité.** Dans `Domaine/Entites.cs` :
   ```csharp
   public class Superviseur
   {
       public int Id { get; set; }
       public string Login { get; set; } = "";
       public string Nom { get; set; } = "";
       public string Prenom { get; set; } = "";
       public string MotDePasseHache { get; set; } = "";   // jamais le mot de passe lui-même
   }
   ```
   Un `DbSet<Superviseur> Superviseurs`, un index unique sur `Login`, une migration.
2. **Le compte de démonstration**, créé au démarrage s'il n'y a aucun superviseur, avec `PasswordHasher` :
   ```csharp
   builder.Services.AddSingleton<IPasswordHasher<Superviseur>, PasswordHasher<Superviseur>>();
   // … au démarrage, si db.Superviseurs est vide :
   var s = new Superviseur { Login = "cmorel", Nom = "MOREL", Prenom = "Claire" };
   s.MotDePasseHache = hacheur.HashPassword(s, "Supervision2026!");
   ```
3. **Le cookie de session.** Dans `Program.cs` :
   ```csharp
   builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
       .AddCookie(o => { o.LoginPath = "/connexion"; o.Cookie.HttpOnly = true; });
   // … après UseRouting() :
   app.UseAuthentication();
   app.UseAuthorization();
   ```
4. **La page `/connexion`** (`Pages/Connexion.cshtml` et `.cs`) : un formulaire `post` avec les champs
   `Login` et `MotDePasse`. Dans `OnPostAsync` : chercher le superviseur, vérifier avec
   `hacheur.VerifyHashedPassword(...)`, puis :
   ```csharp
   var identite = new ClaimsIdentity(
   [
       new Claim(ClaimTypes.NameIdentifier, superviseur.Id.ToString()),
       new Claim(ClaimTypes.Name, superviseur.Login)
   ], CookieAuthenticationDefaults.AuthenticationScheme);
   await HttpContext.SignInAsync(new ClaimsPrincipal(identite));
   return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl! : "/");
   ```
   En cas d'échec, **un seul message** : `Identifiant ou mot de passe incorrect.`

✅ `dotnet test --filter "Groupe=seance-06&FullyQualifiedName~m1"`

### Mission 6.2 🟢 — Protéger les moniteurs et l'API

1. Une **politique par défaut** : tout exige un superviseur connecté.
   ```csharp
   builder.Services.AddAuthorizationBuilder()
       .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
   ```
2. Ouvrez ce qui doit rester public : `[AllowAnonymous]` sur `ConnexionModel`, et
   `@attribute [Microsoft.AspNetCore.Authorization.AllowAnonymous]` dans `Erreur.cshtml`.
   La feuille de style reste servie car `UseStaticFiles()` est **avant** `UseAuthorization()`.
3. Pour `/api` et `/hubs`, **401** au lieu d'une redirection :
   ```csharp
   o.Events.OnRedirectToLogin = contexte =>
   {
       if (contexte.Request.Path.StartsWithSegments("/api") || contexte.Request.Path.StartsWithSegments("/hubs"))
           contexte.Response.StatusCode = StatusCodes.Status401Unauthorized;
       else
           contexte.Response.Redirect(contexte.RedirectUri);
       return Task.CompletedTask;
   };
   ```
4. Dans `_Layout.cshtml`, le nom du superviseur connecté et un bouton **Se déconnecter**
   (`<form method="post" action="/connexion?handler=Deconnexion">` → `OnPostDeconnexionAsync` → `SignOutAsync()`).

✅ `dotnet test --filter "Groupe=seance-06&FullyQualifiedName~m2"` — puis **toutes** les séances précédentes :
les tests se connectent tout seuls avec le compte de démonstration.

### Mission 6.3 🟡 — Le moniteur 3 : le dossier en cours

**Attendu :** `/moniteur/3` affiche le dossier du **conseiller sélectionné** (séance 5) : le contact qu'il
traite (décroché, pas encore clos) mène au sociétaire. On y voit son nom, ses contrats
(`data-contrat="AUTO-FICT-0001"`), ses sinistres (`data-sinistre`), ses réclamations (`data-reclamation`).
`/moniteur/3?societaire=S009` ouvre un dossier directement. Sans sélection : « Aucun dossier ouvert ».

> 💡 **Indices.** `etat.Contacts.Where(c => c.ConseillerId == id && c.Clos is null).OrderByDescending(c => c.Decroche)`.
> Pour charger le dossier d'un coup : `.Include(s => s.Contrats).ThenInclude(c => c.Sinistres).Include(s => s.Reclamations)`.
> Le moniteur 3 écoute aussi le hub : à l'événement `moniteur:selection`, rechargez la page.

✅ `dotnet test --filter "Groupe=seance-06&FullyQualifiedName~m3"`

### Mission 6.4 🟡 — Notes et journal des actions

**Attendu :**
- `POST /api/notes` avec `{"societaireId":"S001","texte":"…"}` → **201** ; texte vide → **400** ; la note
  est enregistrée avec **son auteur** (table `Notes`) et s'affiche sur le dossier ;
- table `JournalActions` (`SuperviseurId`, `Action`, `Cible`, `Horodatage`) : une ligne `Note` (cible :
  le sociétaire) à chaque note, une ligne `Affectation` (cible : le contact) à chaque affectation.

> 💡 **Indice.** Dans une API minimale, ajoutez un paramètre `ClaimsPrincipal utilisateur` : c'est le
> superviseur connecté. Son identifiant : `int.Parse(utilisateur.FindFirstValue(ClaimTypes.NameIdentifier)!)`.
> `TypedResults.ValidationProblem(...)` pour le 400.

✅ `dotnet test --filter Groupe=seance-06`

### Mission 6.5 🔴 — Limiter les tentatives

**Attendu :** après 5 échecs pour un même identifiant, la connexion est refusée pendant une minute,
même avec le bon mot de passe : « Trop de tentatives… ». *(test : `--filter "Groupe=bonus&FullyQualifiedName~s6_m5"`)*

## 4 ✅ Valider

```bash
dotnet test     # toutes les séances : la sécurité ne doit rien avoir cassé
git add -A && git commit -m "feat: connexion des superviseurs, moniteur 3, notes et journal" && git push
```

---

## 📌 Concepts à connaître

1. **Hachage** — une empreinte à sens unique (`PasswordHasher`, avec sel et itérations) : on vérifie un mot de passe sans pouvoir le retrouver.
2. **Message neutre** — le même échec que l'identifiant existe ou non : on ne révèle pas quels comptes existent.
3. **Authentification / autorisation** — qui êtes-vous (cookie de session) / avez-vous le droit (politique par défaut, `[AllowAnonymous]`).
4. **401 ou 302** — une page redirige vers le formulaire ; une API répond 401 : un programme ne remplit pas de formulaire.
5. **Traçabilité** — le journal dit qui a fait quoi et quand ; on n'y écrit jamais de mot de passe ni de donnée inutile.

## 📓 Journal de fin de séance (portail)

Ce que j'ai fait · ce qui m'a bloqué · ce que j'ai compris aujourd'hui.
