# 🎓 Fiche de réalisation — Supervision CRC

> Cette fiche prépare la ligne « Supervision CRC » de votre **tableau de synthèse** (épreuve E5,
> rubrique *Réalisation en cours de formation*) et les pages correspondantes de votre portfolio.
> Ne cochez une compétence que si vous pouvez **montrer** ce que vous avez fait.

| | |
|---|---|
| **Intitulé** | Supervision CRC — application de pilotage d'un centre de relation client sur poste à 4 moniteurs (.NET) |
| **Contexte** | Situation professionnelle en formation, BTS SIO 2ᵉ année, option SLAM — organisation fictive du secteur tertiaire (mutuelle d'assurance) |
| **Période** | du __/__/__ au __/__/__ |
| **Dépôt** | https://github.com/… |
| **Technologies** | C# 13, ASP.NET Core .NET 10 (Razor Pages, API minimales), SignalR, EF Core + SQLite, xUnit, GitHub Actions, Codespaces |

## Ce que j'ai réalisé

*En 5 à 8 lignes, à la première personne : la demande, ce que vous avez livré, une difficulté et comment vous l'avez résolue.*

## Compétences du tableau de synthèse

| Compétence | Ce que je peux montrer | Séances | ☑ |
|---|---|---|---|
| **Gérer le patrimoine informatique** — *mettre en place et vérifier les niveaux d'habilitation associés à un service* | connexion des superviseurs, politique par défaut, 401 / 302, mots de passe hachés | 6 | ☐ |
| **Gérer le patrimoine informatique** — *gérer des sauvegardes* | migrations réversibles, reprise des compétences sans perte de données | 2 | ☐ |
| **Répondre aux incidents et aux demandes d'assistance et d'évolution** — *traiter des demandes concernant les applications* | chaque séance part d'une demande de Mme Morel ; la faille d'injection SQL corrigée | 1 à 8 | ☐ |
| **Développer la présence en ligne de l'organisation** — *participer à l'évolution d'un site Web exploitant les données de l'organisation* | moniteurs alimentés par les données du CRC, API, temps réel | 1, 3, 4, 5 | ☐ |
| **Travailler en mode projet** — *planifier les activités ; évaluer les indicateurs de suivi* | `PROGRESSION.md`, journal de séance, historique Git, tests verts par séance | 1 à 8 | ☐ |
| **Mettre à disposition des utilisateurs un service informatique** — *réaliser les tests d'intégration et d'acceptation ; déployer un service* | tests automatisés, CI, recette avec la demandeuse, publication en mode Production, sonde de santé | 8 | ☐ |
| **Organiser son développement professionnel** — *mettre en œuvre des outils et stratégies de veille* | documentation ASP.NET Core / EF Core, OWASP, recommandation ACPR sur les réclamations | 7, 8 | ☐ |

## Preuves à joindre au portfolio

- [ ] Capture : les quatre moniteurs ouverts sur le poste (ou en mosaïque)
- [ ] Capture : l'onglet **Actions** avec les séances au vert
- [ ] Extrait de code commenté : la migration de reprise des compétences **ou** l'affectation sous verrou
- [ ] La faille d'injection : la saisie qui la prouve, le code avant / après, le test qui la verrouille
- [ ] Une requête d'API et sa réponse (avec le code HTTP), par exemple un refus 409
