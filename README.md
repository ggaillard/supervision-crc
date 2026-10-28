# Situation professionnelle — Poste de supervision 4 moniteurs

**BTS SIO SLAM — 2e année — 2026-2027**

Vous êtes développeur au service informatique de la **Mutuelle Forézienne d'Assurance (MFA)**, une mutuelle d'assurance fictive basée à Saint-Étienne. La responsable du centre de relation client vous confie le développement de l'application de supervision utilisée par ses superviseurs, sur un poste équipé de **4 moniteurs**.

> Toutes les personnes, sociétaires et numéros de ce dépôt sont fictifs.

---

## 1. L'organisation

| Élément | Description |
| --- | --- |
| Secteur | Tertiaire — assurance mutualiste (auto, habitation, santé) |
| Taille | 240 salariés, 85 000 sociétaires |
| Service concerné | Centre de relation client (CRC) : 18 conseillers, 2 superviseurs |
| Files de traitement | Sinistres, Contrats, Santé, Accueil général |
| Canaux | Téléphone, chat, e-mail, demandes de rappel |
| Demandeuse | Claire Morel, responsable du CRC |

**Le problème.** Aujourd'hui le superviseur jongle entre le logiciel de téléphonie, l'outil de gestion des sinistres et un tableur d'indicateurs. Il voit trop tard une file qui sature, un conseiller en difficulté ou une réclamation proche de son échéance.

## 2. Pourquoi 4 moniteurs

Le superviseur doit voir **en même temps** la demande, la ressource, le cas difficile et le résultat. Une seule fenêtre à onglets cacherait trois de ces informations à chaque instant.

| Moniteur | Question à laquelle il répond | Données affichées | Actualisation |
| --- | --- | --- | --- |
| 1. File d'attente multicanal | Qui attend, depuis combien de temps, pour quel motif ? | Contacts en attente par canal et par file, priorité, niveau de service | 5 s |
| 2. Plateau des conseillers | Qui est disponible, qui est en difficulté ? | Statut et durée de chaque conseiller, compétences, dépassements | 5 s |
| 3. Dossier en escalade | Que se passe-t-il sur le cas que je traite ? | Fiche sociétaire, contrat, sinistre, historique, échéance de réclamation, note | sur sélection |
| 4. Indicateurs & alertes | L'objectif de la journée est-il tenu ? | Taux de décroché, durée moyenne, résolution au 1er contact, satisfaction, alertes | 1 min |

Les moniteurs **dialoguent** :

- affecter un contact sur le moniteur 1 met à jour le moniteur 2 ;
- sélectionner un conseiller sur le moniteur 2 ouvre son dossier sur le moniteur 3 ;
- une alerte du moniteur 4 met en évidence la file ou le conseiller concerné.

La maquette validée par la demandeuse est fournie par votre enseignant (lien du prototype cliquable). Les captures sont à placer dans `docs/maquettes/`.

## 3. Besoin exprimé

User stories de la demandeuse :

- En tant que superviseur, je veux voir les contacts en attente triés par priorité puis par attente, pour traiter d'abord les escalades.
- En tant que superviseur, je veux affecter un contact à un conseiller disponible ayant la bonne compétence.
- En tant que superviseur, je veux être alerté quand un conseiller dépasse 10 min d'appel ou de pause.
- En tant que superviseur, je veux ouvrir le dossier d'un conseiller sur un moniteur dédié et y laisser une note.
- En tant que responsable du CRC, je veux suivre les indicateurs du jour et de la semaine face aux objectifs.

Les règles de gestion (RG1 à RG7) sont dans [`docs/cahier-des-charges.md`](docs/cahier-des-charges.md).

## 4. Contraintes techniques

| Point | Exigence |
| --- | --- |
| Poste | 1 PC Windows, 4 moniteurs Full HD (1920 × 1080) en 2 × 2 |
| Technologie | C# / .NET 8, WPF : une seule application qui ouvre 4 fenêtres |
| Architecture | Présentation (4 vues MVVM) / services / accès aux données ; un service d'événements partagé notifie les 4 vues |
| Données | Base MySQL (ou SQLite) que **vous** concevez, alimentée par le simulateur fourni |
| Configuration | `config/poste.json` : vue ↔ moniteur, seuils d'alerte, objectifs |
| Sécurité | Authentification du superviseur, journal des actions (affectation, acquittement, note) |

**Pas 4 écrans en salle ?** L'application doit ouvrir ses vues en fenêtres redimensionnables quand il y a moins de 4 moniteurs (RG7). Vous développez sur 1 ou 2 écrans.

## 5. Ce qui vous est fourni

```
supervision-crc/
├── README.md                    ← ce sujet
├── docs/
│   ├── cahier-des-charges.md    ← règles de gestion, indicateurs, glossaire
│   ├── fiche-E5.md              ← trame de fiche de réalisation professionnelle
│   └── maquettes/               ← captures du prototype (à ajouter)
├── config/
│   └── poste.json               ← exemple de configuration du poste
├── data/
│   ├── conseillers.json         ← 18 conseillers et leurs compétences
│   ├── societaires.json         ← sociétaires, contrats, sinistres, réclamations
│   └── scenario-matinee.json    ← déroulé d'une matinée (contacts, statuts)
└── src/
    └── Simulateur/              ← bibliothèque C# qui rejoue le scénario
```

Le **simulateur** rejoue `scenario-matinee.json` en accéléré et déclenche des événements C# (`ContactArrive`, `ContactDecroche`, `StatutConseillerChange`…). Votre application s'y abonne, comme elle s'abonnerait demain à la vraie téléphonie. Mode d'emploi : [`src/Simulateur/README.md`](src/Simulateur/README.md).

## 6. Vos missions

1. **Analyse** — jouer le prototype, compléter les user stories, produire le MCD et le script SQL de création.
2. **Socle multi-écrans** — ouvrir les 4 fenêtres sur les bons moniteurs d'après `config/poste.json`, avec repli en fenêtres (RG7).
3. **Moniteurs 1 et 2** — file d'attente triée (RG1), plateau des conseillers, affectation (RG2), alertes de durée (RG4).
4. **Moniteurs 3 et 4** — dossier synchronisé sur la sélection, note enregistrée, indicateurs (RG3) et alertes acquittables (RG5, RG6).
5. **Recette** — tests unitaires des règles RG1 à RG5, démonstration à la demandeuse.

## 7. Livrables (dans votre dépôt)

- [ ] MCD, dictionnaire de données et script SQL
- [ ] Code source et fichier de configuration
- [ ] Tests unitaires des règles de gestion
- [ ] Documentation utilisateur du superviseur (1 page par moniteur)
- [ ] README technique : installation, lancement du simulateur, limites connues
- [ ] Fiche de réalisation professionnelle (trame `docs/fiche-E5.md`)

## 8. Évaluation

| Critère | Points |
| --- | --- |
| Analyse du besoin, MCD et base conformes aux règles | 4 |
| Socle multi-écrans et repli en fenêtres | 3 |
| File d'attente, plateau et affectation corrects | 4 |
| Dossier synchronisé, indicateurs et alertes | 4 |
| Tests unitaires des règles de gestion | 2 |
| Sécurité : authentification et journal des actions | 1 |
| Présentation à la demandeuse et documentation | 2 |
| **Total** | **20** |

## 9. Démarrer

1. Cliquez sur **Use this template** pour créer votre propre dépôt (privé), puis clonez-le.
2. Ouvrez `src/Simulateur` et lancez la démo console : `dotnet run --project src/Simulateur.Demo`.
3. Commencez par la mission 1 : le MCD est le premier jalon.

Compétences du référentiel : bloc 2 « Conception et développement d'applications », et bloc 3 (cybersécurité) pour l'authentification et la journalisation.
