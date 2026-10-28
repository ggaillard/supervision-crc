# 📐 Cahier des charges — Supervision du CRC de la MFA

*Rédigé avec Claire Morel, responsable du centre de relation client. Les paramètres chiffrés sont dans
`appsettings.json` (section `Supervision`) et `config/poste.json`, jamais écrits en dur dans le code.*

## 1. Règles de gestion

| N° | Règle | Séance |
|---|---|---|
| RG1 | **Priorité de la file d'attente** : Escalade > Haute > Normale. À priorité égale, le contact qui attend depuis le plus longtemps passe devant. | 1 |
| RG2 | Un contact n'est affecté qu'à un conseiller au statut **Disponible** qui possède la **compétence de la file** du contact. Parmi eux, l'application propose celui qui est disponible **depuis le plus longtemps**. Un contact n'est affecté qu'une fois ; un conseiller ne reçoit qu'un contact à la fois. | 4 |
| RG3 | **Niveau de service** (par file) = contacts décrochés en moins de 20 s ÷ contacts arrivés, sur une **fenêtre glissante de 30 minutes** ; les contacts arrivés depuis moins de 20 s ne comptent pas encore. **Alerte** quand il reste sous 80 % à chaque minute pendant 10 minutes. | 7 |
| RG4 | **Alerte de durée** quand un conseiller est en appel (téléphone ou chat) ou en pause depuis **plus** de 10 minutes. Les deux seuils sont paramétrables. Le post-appel n'est pas concerné. | 3 |
| RG5 | **Réclamation** : accusé de réception sous **10 jours ouvrables** (lundi au vendredi, hors jours fériés), réponse sous **2 mois** après réception. Alerte quand une échéance non tenue tombe dans les **5 jours** ou est dépassée. *Délais recommandés par l'ACPR pour le traitement des réclamations : à vérifier sur le site de l'ACPR, source à citer.* | 7 |
| RG6 | Une alerte acquittée n'est **jamais supprimée** : elle reste avec l'heure et l'identité du superviseur qui l'a acquittée. | 7 |
| RG7 | Chaque vue s'ouvre sur le moniteur prévu par `config/poste.json`. S'il manque un moniteur, les vues s'ouvrent **en fenêtres** sur les écrans restants. | 5 |

## 2. Statuts d'un conseiller

| Statut (code) | Signification | Peut recevoir un contact |
|---|---|---|
| Disponible | Connecté, en attente | Oui |
| EnAppel | Traite un appel téléphonique | Non |
| Chat | Traite une ou deux conversations écrites | Non |
| PostAppel | Rédige le compte rendu du contact précédent | Non |
| Pause | Pause déclarée | Non |
| Formation | Indisponible (formation) | Non |
| HorsLigne | Pas encore connecté | Non |

## 3. Indicateurs (moniteur 4)

| Indicateur | Calcul | Objectif MFA |
|---|---|---|
| Taux de décroché | contacts décrochés ÷ (décrochés + abandonnés) | ≥ 95 % |
| Durée moyenne de traitement | moyenne de (clos − décroché) sur les contacts clos | ≤ 7 min |
| Résolution au premier contact | contacts clos résolus ÷ contacts clos | ≥ 75 % |
| Satisfaction à chaud | moyenne des notes de 1 à 5 | ≥ 4,0 |
| Niveau de service | voir RG3 | ≥ 80 % |

Un contact encore en attente n'entre dans aucun de ces calculs.

## 4. Les quatre moniteurs et leurs échanges

| Moniteur | URL | Contenu |
|---|---|---|
| 1 · File d'attente | `/moniteur/1` | contacts en attente triés (RG1), attente, compteurs par file, bouton Affecter |
| 2 · Plateau | `/moniteur/2` | 18 conseillers, statut, durée, alertes RG4, sélection |
| 3 · Dossier | `/moniteur/3` | le dossier du sociétaire en ligne avec le conseiller sélectionné, notes |
| 4 · Indicateurs | `/moniteur/4` | indicateurs, niveau de service par file, alertes RG3 RG4 RG5, acquittement |

| Action | Depuis | Effet |
|---|---|---|
| Affecter un contact | moniteur 1 | le contact quitte la file ; le conseiller passe *En appel* sur le moniteur 2 |
| Sélectionner un conseiller | moniteur 2 | son dossier s'ouvre sur le moniteur 3 |
| Écrire une note | moniteur 3 | la note rejoint le dossier ; une ligne au journal des actions |
| Acquitter une alerte | moniteur 4 | l'alerte passe en « acquittée par … à … » ; une ligne au journal |

Tous les moniteurs se mettent à jour **sans rechargement**, dès que l'état change.

## 5. Sécurité

- Seuls les superviseurs connectés accèdent aux moniteurs et à l'API (sauf la connexion et la sonde de santé).
- Les mots de passe sont stockés sous forme d'empreinte ; le message d'échec de connexion ne dit pas si l'identifiant existe.
- Chaque affectation, note et acquittement est journalisé : qui, quoi, sur quoi, quand.

## 6. Glossaire

- **CRC** : centre de relation client.
- **Escalade** : contact remonté parce que le conseiller ne peut pas le régler seul (réclamation, client mécontent).
- **Sociétaire** : client d'une mutuelle, qui en est aussi membre.
- **Réclamation** : mécontentement exprimé envers la mutuelle ; elle obéit à des délais de réponse (RG5).
- **Acquitter une alerte** : indiquer qu'on l'a vue et qu'on la prend en charge.
- **Décrocher** : un conseiller prend en charge un contact (appel, chat, e-mail).
- **Abandon** : le sociétaire raccroche avant d'avoir été pris en charge.
