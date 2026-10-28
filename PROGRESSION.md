# 📋 Ma progression — Supervision CRC

> Cochez (`[ ]` → `[x]`) au fur et à mesure, puis **committez** ce fichier.
> Une mission 🟢 ou 🟡 n'est cochée que si ses tests passent.

**Nom :** · **Dépôt :**

---

## 🚀 Mise en route
- [ ] Dépôt créé depuis le modèle, Codespace ouvert
- [ ] L'application s'affiche (port 5000), l'horloge avance
- [ ] `dotnet test --filter Groupe=socle` : 8 tests verts

## 1️⃣ Moniteur 1 : la file d'attente
- [ ] 🎓 Fiche [Événements, état et pages Razor](cours/01-evenements-razor.md) + auto-évaluation
- [ ] 🟢 1.1 Suivre un événement du simulateur jusqu'à l'écran (questions du guide)
- [ ] 🟢 1.2 RG1 : trier la file
- [ ] 🟡 1.3 L'attente et la priorité d'un coup d'œil
- [ ] 🟡 1.4 Un compteur par file
- [ ] 🔴 1.5 Filtrer par canal
- [ ] ✅ `Groupe=seance-01` vert

## 2️⃣ Le modèle de données
- [ ] 🎓 Fiche [Associations et migrations EF Core](cours/02-associations-migrations.md) + auto-évaluation
- [ ] 🟢 2.1 L'entité `Competence` (association porteuse)
- [ ] 🟢 2.2 La migration de reprise : ajouter, transférer, supprimer
- [ ] 🟡 2.3 L'historique des statuts
- [ ] 🔴 2.4 La page des conseillers d'une file
- [ ] ✅ `Groupe=seance-02` et `socle` verts

## 3️⃣ Moniteur 2 : le plateau
- [ ] 🎓 Fiche [Configuration et règles de gestion](cours/03-configuration-regles.md) + auto-évaluation
- [ ] 🟢 3.1 La page du plateau
- [ ] 🟢 3.2 RG4 : appels longs et pauses longues, seuils configurables
- [ ] 🟡 3.3 Compter et filtrer par statut
- [ ] 🔴 3.4 Trier par durée
- [ ] ✅ `Groupe=seance-03` vert

## 4️⃣ Affecter un contact
- [ ] 🎓 Fiche [API REST et accès concurrents](cours/04-api-concurrence.md) + auto-évaluation
- [ ] 🟢 4.1 RG2 : qui peut prendre ce contact ?
- [ ] 🟢 4.2 Affecter (API + bouton)
- [ ] 🟡 4.3 Les refus : 404 et 409 en problem+json
- [ ] 🟡 4.4 Deux superviseurs, un seul contact
- [ ] 🔴 4.5 Affecter au conseiller proposé
- [ ] ✅ `Groupe=seance-04` vert

## 5️⃣ Temps réel et quatre moniteurs
- [ ] ✍️ Interrogation écrite n° 1 (séances 1 à 4)
- [ ] 🎓 Fiche [Temps réel avec SignalR](cours/05-temps-reel.md) + auto-évaluation
- [ ] 🟢 5.1 Le hub diffuse chaque changement
- [ ] 🟢 5.2 Les moniteurs écoutent au lieu de se recharger
- [ ] 🟡 5.3 L'API du poste
- [ ] 🟡 5.4 Une sélection partagée par les moniteurs
- [ ] 🔴 5.5 Le lanceur de poste sur 4 écrans
- [ ] ✅ `Groupe=seance-05` vert

## 6️⃣ Moniteur 3 et sécurité
- [ ] 🎓 Fiche [Authentification, autorisation, traçabilité](cours/06-securite.md) + auto-évaluation
- [ ] 🟢 6.1 Les superviseurs et la connexion
- [ ] 🟢 6.2 Protéger les moniteurs et l'API
- [ ] 🟡 6.3 Le moniteur 3 : le dossier en cours
- [ ] 🟡 6.4 Notes et journal des actions
- [ ] 🔴 6.5 Limiter les tentatives de connexion
- [ ] ✅ `Groupe=seance-06` et toutes les séances précédentes vertes

## 7️⃣ Moniteur 4 : indicateurs et alertes
- [ ] 🎓 Fiche [Indicateurs et dates](cours/07-indicateurs.md) + auto-évaluation
- [ ] 🟢 7.1 Les indicateurs de la journée
- [ ] 🟡 7.2 RG3 : le niveau de service
- [ ] 🟡 7.3 RG5 : les échéances des réclamations
- [ ] 🟢 7.4 RG6 : des alertes acquittées, jamais effacées
- [ ] 🔴 7.5 Les indicateurs d'une seule file
- [ ] ✅ `Groupe=seance-07` vert

## 8️⃣ Avant la mise en service
- [ ] ✍️ Interrogation écrite n° 2 (séances 5 à 7)
- [ ] 🎓 Fiche [Qualité et exploitation](cours/08-qualite-exploitation.md) + auto-évaluation
- [ ] 🟡 8.1 Audit : trouver et corriger la faille
- [ ] 🟢 8.2 Écrire vos propres tests
- [ ] 🟢 8.3 Style du code au vert
- [ ] 🟡 8.4 Prête pour l'exploitation, recette avec Mme Morel
- [ ] 🟢 8.5 Fiche de réalisation E5
- [ ] ✅ `Groupe=seance-08` vert, toutes les coches vertes dans **Actions**

---
🎓 fiche + auto-évaluation · 🟢 guidé · 🟡 semi-guidé · 🔴 autonome · ✅ tests · ✍️ interrogation
