# 🗺️ Parcours Supervision CRC — 8 séances de 2 heures

> Suivez les séances **dans l'ordre**, cochez [PROGRESSION.md](PROGRESSION.md) au fur et à mesure,
> et committez souvent.

---

## 🧭 La méthode : quatre temps, à chaque séance

Vous ne partez jamais d'une page blanche : l'application tourne, vous la comprenez, puis vous la faites évoluer.

```mermaid
flowchart LR
    L["1 ▶️ LANCER<br/>voir ce qui marche"] --> C["2 🔍 COMPRENDRE<br/>lire le code existant"]
    C --> M["3 ✏️ MODIFIER<br/>🟢 guidé → 🟡 semi → 🔴 autonome"]
    M --> V["4 ✅ VALIDER<br/>tests verts + commit"]
    V -.->|séance suivante| L
```

| Palier | Ce qu'on vous donne | Tests |
|---|---|---|
| 🟢 **Guidé** | L'objectif, la démarche pas à pas, le code clé | groupe `seance-NN` |
| 🟡 **Semi-guidé** | L'objectif, les grandes étapes, un indice | groupe `seance-NN` |
| 🔴 **Autonome** | L'objectif et le critère de réussite | groupe `bonus` (facultatif) |

Les solutions complètes ne sont pas dans le dépôt : chercher fait partie du métier.
Bloqué plus de 10 minutes ? Relisez l'indice, puis [DEPANNAGE.md](DEPANNAGE.md), puis ouvrez une
*issue* (modèle « Question » fourni) ou levez la main.

## ⏱️ Le déroulé d'une séance

| Durée | Temps | Ce que vous faites |
|---|---|---|
| 10 min | **Contrôle d'entrée** | 5 questions au portail sur les concepts de la séance précédente |
| 10 min | **Situation** | Le message de Mme Morel, lu ensemble ; on lance l'application |
| 80 min | **Missions** | 🟢 puis 🟡 ; 🔴 si vous avez fini |
| 10 min | **Valider** | `dotnet test --filter Groupe=seance-NN`, commit, push |
| 10 min | **Débriefing** | Les concepts de la séance, puis votre **journal de fin de séance** |

Le journal tient en trois lignes, dans le portail : *ce que j'ai fait · ce qui m'a bloqué ·
ce que j'ai compris aujourd'hui*. Il compte dans l'évaluation de votre engagement.

## 🗺️ Les 8 séances

| # | Séance | La demande de Mme Morel | Concepts | Guide |
|---|---|---|---|---|
| 1 | **Moniteur 1 : la file d'attente** | « Les escalades sont noyées au milieu de la file » | trajet d'un événement, règle de gestion, Razor | [▶️](seances/seance-01.md) |
| 2 | **Le modèle de données** | « Impossible de savoir qui traite quelle file » | association porteuse, migration, reprise de données | [▶️](seances/seance-02.md) |
| 3 | **Moniteur 2 : le plateau** | « Je veux voir mes conseillers, et qui déborde » | page Razor, options de configuration, RG4 | [▶️](seances/seance-03.md) |
| 4 | **Affecter un contact** | « On affecte en criant dans le plateau » | API REST, codes HTTP, exception métier, verrou | [▶️](seances/seance-04.md) |
| 5 | **Temps réel et quatre moniteurs** | « L'écran clignote toutes les 5 secondes » | SignalR, configuration du poste, état partagé | [▶️](seances/seance-05.md) |
| 6 | **Moniteur 3 et sécurité** | « N'importe qui peut ouvrir la supervision ! » | hachage, cookie, autorisation, traçabilité | [▶️](seances/seance-06.md) |
| 7 | **Moniteur 4 : indicateurs et alertes** | « La direction veut des chiffres fiables » | indicateurs, fenêtre glissante, jours ouvrables | [▶️](seances/seance-07.md) |
| 8 | **Avant la mise en service** | « Avant le premier jour, on veut être rassurés » | injection SQL, tests, style, exploitation, E5 | [▶️](seances/seance-08.md) |

Deux **interrogations écrites** de 30 minutes, sur papier, ouvrent les séances 5 et 8.
Elles portent sur ce que vous avez codé : écrire une ligne de C#, choisir et justifier un code HTTP,
prévoir ce qu'affiche un moniteur, repérer et corriger une erreur. Pas de QCM.

## 📦 Ce qui prouve votre travail

| Preuve | Où |
|---|---|
| Les tests de chaque séance au vert | onglet **Actions** de votre dépôt (une ligne par séance) |
| Des commits réguliers et lisibles | `feat: file triée par priorité`, `fix: reprise des compétences`… |
| `PROGRESSION.md` à jour | à la racine, committé |
| Le journal de fin de séance | portail |
| La fiche de réalisation E5 | [e5/FICHE-REALISATION.md](e5/FICHE-REALISATION.md), complétée en séance 8 |

## 🎓 Les fiches de cours

Une fiche courte par séance, avec son auto-évaluation : **[cours/](cours/README.md)**.
La bonne méthode : *lire la fiche → faire l'auto-évaluation → pratiquer dans la séance*.

## 🏆 Ce que vous saurez faire à la fin

- Reprendre une application existante, la lire et la faire évoluer sans la casser
- Traduire des règles de gestion en code, et les prouver par des tests
- Faire évoluer une base en service : migrations, reprise de données
- Exposer une API, répondre avec les bons codes HTTP, résister aux accès simultanés
- Diffuser un état en temps réel à plusieurs écrans
- Protéger une application : connexion, mots de passe hachés, journal des actions, injection SQL
- Préparer une application pour l'exploitation, et la présenter dans votre dossier E5
