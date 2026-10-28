# 📝 Mon carnet — Supervision CRC

> ✏️ **Ici, vous écrivez.** Les réponses aux questions *Comprendre* de chaque séance, et ce que les
> guides vous demandent de noter. Quelques lignes suffisent : avec vos mots, sans copier la fiche.
> Committez ce fichier à chaque séance : c'est une trace de ce que vous avez compris, relue par l'enseignant
> et utile pour réviser les interrogations.

**Nom :** · **Prénom :**

---

## Séance 1 — Moniteur 1 : la file d'attente

*Questions de la partie « 2 · Comprendre » du [guide de la séance](seances/seance-01.md).*

**1.** Dans `Program.cs`, `EtatCrc` est enregistré avec `AddSingleton`. Que veut dire *singleton* ici, et pourquoi faut-il **une seule** instance pour toute l'application ?

> ✍️ 

**2.** Dans `TempsReel/ServiceSimulation.cs`, quelle ligne transmet chaque événement du simulateur à l'état ?

> ✍️ 

**3.** Dans `TempsReel/EtatCrc.cs`, méthode `Appliquer` : que se passe-t-il pour un `ContactArrive` ? pour un `ContactDecroche` ?

> ✍️ 

**4.** Pourquoi la propriété `FileAttente` renvoie-t-elle `_attente.ToList()` **sous le verrou** plutôt que la liste elle-même ?

> ✍️ 

**5.** Dans `Pages/Moniteurs/FileAttente.cshtml`, quelle ligne fixe l'adresse `/moniteur/1` ? Quelle classe prépare les données de la page ?

> ✍️ 

**6.** Quelle balise recharge la page toutes les 5 secondes ? Quel inconvénient voyez-vous (on y reviendra en séance 5) ?

> ✍️ 

**Mission 1.2.** Pourquoi `(int)c.Priorite` donne-t-il le bon ordre ? (voir l'énumération `Priorite`)

> ✍️ 

---

## Séance 2 — Le modèle de données : compétences et historique

*Questions de la partie « 2 · Comprendre » du [guide de la séance](seances/seance-02.md).*

**1.** Pourquoi une clé primaire composée `(ConseillerId, FileCode)` plutôt qu'un `Id` auto-incrémenté ?

> ✍️ 

**2.** Que devient le niveau « référent » d'un conseiller polyvalent dans une colonne texte ?

> ✍️ 

**3.** Ouvrez `Donnees/Migrations/…_InitialCreate.cs` : qui a écrit ce fichier ? Qui l'exécute, et quand ?

> ✍️ 

**4.** `EtatCrc` vit toute la durée de l'application ; un `CrcContext` vit le temps d'une requête. Comment `EtatCrc` pourrait-il écrire en base ?

> ✍️ 

---

## Séance 3 — Moniteur 2 : le plateau des conseillers

*Questions de la partie « 2 · Comprendre » du [guide de la séance](seances/seance-03.md).*

**1.** `Depuis` contient l'heure du dernier changement de statut. Comment obtenir la **durée** du statut actuel ?

> ✍️ 

**2.** Pourquoi un conseiller en **post-appel** (il rédige son compte rendu) ne doit-il pas déclencher l'alerte « appel long » ?

> ✍️ 

**3.** « 10 minutes » écrit en dur dans le code : que faut-il faire pour passer à 15 l'été ? Et avec la configuration ?

> ✍️ 

**4.** `ReglesDuree.Alertes` reçoit `maintenant` en paramètre au lieu de lire l'horloge elle-même. Qu'est-ce que cela change pour les tests ?

> ✍️ 

---

## Séance 4 — Affecter un contact à un conseiller

*Questions de la partie « 2 · Comprendre » du [guide de la séance](seances/seance-04.md).*

**1.** Une **API** répond à un programme (le JavaScript de la page), pas à un humain : que renvoie-t-elle, si ce n'est du HTML ?

> ✍️ 

**2.** Le contact demandé n'est plus en attente. Quel code HTTP ? Et si le conseiller choisi est en pause ?

> ✍️ 

**3.** Deux requêtes arrivent en même temps. Chacune vérifie « le contact est-il encore en attente ? », lit *oui*, puis l'affecte. Que s'est-il passé ?

> ✍️ 

**4.** Où écrire RG2 pour qu'elle s'applique au bouton **et** à une future application mobile ?

> ✍️ 

---

## Séance 5 — Le temps réel et les quatre moniteurs

*Questions de la partie « 2 · Comprendre » du [guide de la séance](seances/seance-05.md).*

**1.** Combien de requêtes par heure pour 4 moniteurs qui se rechargent toutes les 5 s ? Combien sont utiles ?

> ✍️ 

**2.** Avec le *push*, qui prend l'initiative de la communication ?

> ✍️ 

**3.** `EtatCrc` déclare `public event Action<Evenement>? Change;`. Qui le déclenche, et quand ?

> ✍️ 

**4.** Pourquoi la sélection d'un conseiller doit-elle être gardée **côté serveur** plutôt que dans la page du moniteur 2 ?

> ✍️ 

---

## Séance 6 — Le moniteur 3 et la sécurité

*Questions de la partie « 2 · Comprendre » du [guide de la séance](seances/seance-06.md).*

**1.** Pourquoi ne jamais stocker un mot de passe, même chiffré ? Que stocke-t-on à la place ?

> ✍️ 

**2.** Un identifiant inconnu et un mauvais mot de passe : faut-il le même message ? Pourquoi ?

> ✍️ 

**3.** **Authentification** et **autorisation** : quelle différence ?

> ✍️ 

**4.** Une page protégée renvoie vers le formulaire de connexion (302). Que doit renvoyer une **API** protégée, et pourquoi pas une redirection ?

> ✍️ 

**5.** Le journal des actions est-il une donnée personnelle ? Que faut-il y mettre, et ne pas y mettre ?

> ✍️ 

---

## Séance 7 — Moniteur 4 : indicateurs et alertes

*Questions de la partie « 2 · Comprendre » du [guide de la séance](seances/seance-07.md).*

**Les quatre indicateurs, en fractions** (cahier des charges, section *Indicateurs*)

| Indicateur | Numérateur | Dénominateur | Exclus |
|---|---|---|---|
| Taux de décroché | | | |
| Durée moyenne de traitement | | | |
| Résolution au premier contact | | | |
| Satisfaction | | | |

**1.** Pourquoi le niveau de service se calcule-t-il sur les **30 dernières minutes** plutôt que depuis 8 h ?

> ✍️ 

**2.** Un contact arrivé il y a 5 secondes et pas encore décroché : à temps ou en retard ? Que faire de lui ?

> ✍️ 

**3.** Une réclamation reçue le jeudi 5 novembre 2026 : à quelle date limite l'accusé de réception (10 jours ouvrables) ? Comptez sur un calendrier.

> ✍️ 

**4.** Pourquoi ne pas supprimer une alerte acquittée ? Qui pourrait en avoir besoin, et quand ?

> ✍️ 

---

## Séance 8 — Avant la mise en service

*Questions de la partie « 2 · Comprendre » du [guide de la séance](seances/seance-08.md).*

**La grille d'audit** — pour chaque ligne : ce que vous avez trouvé, et le verdict ✅ / ⚠️ / ❌.

| # | Risque (question dans le guide) | Ce que j'ai trouvé (fichier, ligne) | Verdict |
|---|---|---|---|
| 1 | A01 Contrôle d'accès | | |
| 2 | A02 Cryptographie | | |
| 3 | **A03 Injection** | | |
| 4 | A04 Conception | | |
| 5 | A05 Configuration | | |
| 6 | A07 Authentification | | |
| 7 | A09 Journalisation | | |

**Mission 8.1 — la faille.** Ce qu'elle permettait (avec la saisie qui le prouve) :

> ✍️ 

Pourquoi ma correction la ferme :

> ✍️
