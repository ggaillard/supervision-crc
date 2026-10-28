# Cahier des charges — Supervision du CRC de la MFA

## 1. Règles de gestion

| N° | Règle |
| --- | --- |
| RG1 | **Priorité de la file d'attente** : Escalade > Haute > Normale. À priorité égale, le contact qui attend depuis le plus longtemps passe devant. |
| RG2 | Un contact n'est affecté qu'à un conseiller au statut **Disponible** qui possède la **compétence de la file** du contact. |
| RG3 | **Niveau de service** (par file et global) = contacts décrochés en moins de 20 s ÷ contacts reçus. Alerte quand il reste sous 80 % pendant 10 min consécutives. |
| RG4 | Alerte quand un conseiller est **en appel** ou **en pause** depuis plus de 10 min. Les deux seuils sont paramétrables dans `config/poste.json`. |
| RG5 | **Réclamation** : accusé de réception sous 10 jours ouvrables, réponse sous 2 mois après réception. Alerte à J-5 de chaque échéance. *Délais recommandés par l'ACPR : vérifiez-les sur le site de l'ACPR et citez votre source.* |
| RG6 | Une alerte acquittée n'est jamais supprimée : elle reste dans l'historique avec l'heure et l'identifiant du superviseur qui l'a acquittée. |
| RG7 | Chaque vue s'ouvre sur le moniteur prévu par la configuration. S'il manque un moniteur au démarrage, ou s'il est débranché en cours de route, la vue s'ouvre en fenêtre sur un écran restant. |

## 2. Statuts d'un conseiller

| Statut | Signification | Peut recevoir un contact |
| --- | --- | --- |
| Disponible | Connecté, en attente | Oui |
| En appel | Traite un appel téléphonique | Non |
| Chat | Traite 1 ou 2 conversations écrites | Non (au-delà de 2 conversations) |
| Post-appel | Saisit le compte rendu de l'appel précédent | Non |
| Pause | Pause déclarée | Non |
| Formation | Indisponible pour la journée ou la demi-journée | Non |

## 3. Indicateurs (moniteur 4)

| Indicateur | Calcul | Objectif MFA |
| --- | --- | --- |
| Taux de décroché | contacts décrochés ÷ contacts reçus | ≥ 95 % |
| Durée moyenne de traitement (DMT) | (durée d'appel + durée de post-appel) ÷ nombre d'appels traités | ≤ 7 min |
| Résolution au premier contact | contacts clos sans recontact sous 7 jours ÷ contacts clos | ≥ 75 % |
| Satisfaction à chaud | moyenne des notes de 1 à 5 laissées par les sociétaires | ≥ 4,0 |
| Niveau de service | voir RG3 | ≥ 80 % |

Les objectifs sont lus dans `config/poste.json`, jamais écrits en dur dans le code.

## 4. Interactions entre moniteurs

| Action | Moniteur d'origine | Effet |
| --- | --- | --- |
| Affecter un contact | 1 | Le contact quitte la file ; le conseiller passe « En appel » sur le moniteur 2 |
| Sélectionner un conseiller | 2 | Le dossier qu'il traite s'affiche sur le moniteur 3 |
| Cliquer une alerte | 4 | La file ou le conseiller concerné est mis en évidence sur le moniteur 1 ou 2 |
| Enregistrer une note | 3 | La note est ajoutée à l'historique du dossier et au journal des actions |

## 5. Glossaire

- **CRC** : centre de relation client.
- **Escalade** : contact remonté au superviseur parce que le conseiller ne peut pas le régler seul (réclamation, client mécontent, montant élevé).
- **Sociétaire** : client d'une mutuelle, qui en est aussi membre.
- **Réclamation** : mécontentement exprimé par écrit ou oralement envers la mutuelle ; elle obéit à des délais de réponse (RG5).
- **Acquitter une alerte** : indiquer qu'on l'a vue et prise en charge.
