# Simulateur du CRC

Bibliothèque C# (.NET 10) qui rejoue `data/scenario-matinee.json` : la matinée du mardi 28 octobre 2026,
de 08:00 à 12:00, avec 313 contacts et 18 conseillers. Elle remplace la téléphonie réelle pendant le
développement. **Vous n'avez pas à la modifier.**

## Essayer seul

```bash
dotnet run --project src/Mfa.Crc.Simulateur.Demo           # 4 h rejouées en 24 s
dotnet run --project src/Mfa.Crc.Simulateur.Demo -- 60     # 1 minute simulée par seconde
```

## Comment l'application l'utilise

`TempsReel/ServiceSimulation.cs` le charge au démarrage et transmet chaque événement à `EtatCrc` :

```csharp
var simulateur = SimulateurCrc.Charger(chemins.Scenario);
simulateur.Acceleration = options.Acceleration;               // appsettings.json, section Simulation
simulateur.EvenementRejoue += (_, e) => etat.Appliquer(e);
simulateur.HorlogeAvancee += (_, h) => etat.AvancerHorloge(h); // l'horloge avance entre deux événements
await simulateur.RejouerAsync(arret);
```

Les événements arrivent sur un fil d'exécution de fond : c'est pourquoi `EtatCrc` protège ses données
par un verrou.

## Événements

| Événement | Données | Quand |
|---|---|---|
| `ContactArrive` | contact, file, canal, priorité, sociétaire, motif | un sociétaire entre en file d'attente |
| `ContactDecroche` | contact, conseiller | un conseiller prend le contact |
| `ContactAbandonne` | contact | le sociétaire raccroche avant d'être pris |
| `ContactClos` | contact, conseiller, satisfaction (1-5), résolu au premier contact | fin de traitement |
| `StatutConseillerChange` | conseiller, nouveau statut | Disponible, EnAppel, Chat, PostAppel, Pause, Formation |
| `EscaladeSuperviseur` | contact, conseiller, sociétaire, réclamation | le conseiller remonte le cas au superviseur |

`EvenementRejoue` est déclenché pour tous les types, `HorlogeAvancee` au moins une fois par seconde
réelle, `Termine` à la fin.

## Pour les tests

`AvancerJusqua(new TimeSpan(11, 42, 0))` rejoue instantanément tous les événements jusqu'à 11:42 : les
tests s'en servent (`RejouerScenarioJusqua`) pour vérifier une règle à une heure précise.

## Ce que le simulateur ne fait pas

Il ne trie pas la file, n'affecte personne, ne calcule aucun indicateur et ne lève aucune alerte :
c'est votre travail. Il rejoue ce qui s'est passé ; votre application décide de ce que le superviseur doit voir.
