# Simulateur du CRC

Bibliothèque C# (.NET 8) qui rejoue `data/scenario-matinee.json` : la matinée du 28 octobre 2026, de 08:00 à 12:00, avec un peu plus de 300 contacts et 18 conseillers. Elle remplace la téléphonie réelle pendant le développement.

## Essayer

```bash
dotnet run --project src/Simulateur.Demo          # 4 h rejouées en 24 s
dotnet run --project src/Simulateur.Demo -- 60    # 1 minute simulée par seconde
```

## Utiliser dans votre application

Ajoutez une référence au projet, puis abonnez-vous aux événements :

```csharp
using Mfa.Crc.Simulateur;

var simulateur = SimulateurCrc.Charger("data/scenario-matinee.json");
simulateur.Acceleration = 60;

simulateur.ContactArrive += (_, e) =>
    Application.Current.Dispatcher.Invoke(() => /* mettre à jour la file d'attente */);

_ = simulateur.RejouerAsync();   // ne bloque pas l'interface
```

Les événements arrivent sur un thread de travail : en WPF, passez par le `Dispatcher` avant de modifier une vue.

## Événements

| Événement | Données | Quand |
| --- | --- | --- |
| `ContactArrive` | contact, file, canal, priorité, sociétaire, motif | Un sociétaire entre en file d'attente |
| `ContactDecroche` | contact, conseiller | Un conseiller prend le contact |
| `ContactAbandonne` | contact | Le sociétaire raccroche avant d'être pris |
| `ContactClos` | contact, conseiller, satisfaction (1-5), résolu au premier contact | Fin de traitement |
| `StatutConseillerChange` | conseiller, nouveau statut | Disponible, EnAppel, Chat, PostAppel, Pause, Formation |
| `EscaladeSuperviseur` | contact, conseiller, sociétaire, réclamation | Le conseiller remonte le cas au superviseur |

`EvenementRejoue` est déclenché pour tous les types ; `Termine` à la fin.

## Pour les tests

`AvancerJusqua(new TimeSpan(10, 30, 0))` rejoue instantanément tous les événements jusqu'à 10:30, sans attendre. Cela permet de vérifier une règle de gestion à une heure précise.

## Ce que le simulateur ne fait pas

Il ne trie pas la file, n'affecte personne, ne calcule aucun indicateur et ne lève aucune alerte : c'est votre travail. Il rejoue ce qui s'est passé ; votre application décide de ce que le superviseur doit voir.
