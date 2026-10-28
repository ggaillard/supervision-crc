# Temps réel avec SignalR

> **Séance 5** · Lecture 8 min · ▶️ [Faire la séance](../seances/seance-05.md)

## L'idée

**Pull** : le navigateur redemande la page toutes les 5 secondes, que quelque chose ait changé ou non.
**Push** : le navigateur ouvre **une** connexion durable vers le serveur ; c'est le serveur qui prévient
quand l'état change. Moins de requêtes, et l'information arrive tout de suite.

SignalR fournit cette connexion (WebSocket quand c'est possible, sinon une solution de repli) : le
serveur appelle une **méthode nommée** chez les clients (`changement`, `selection`) avec des données.

| | Pull (`<meta refresh>`) | Push (SignalR) |
|---|---|---|
| Qui prend l'initiative ? | le client | le serveur |
| Délai d'affichage | jusqu'à 5 s | immédiat |
| Requêtes pour 4 moniteurs, 1 h | 2 880, presque toutes inutiles | 4 connexions + une par changement |

## Le code à reconnaître

```csharp
public sealed class HubCrc : Hub;                         // le point de connexion

public sealed class RelaisTempsReel(EtatCrc etat, IHubContext<HubCrc> hub)
{
    public void Brancher() => etat.Change += e =>
        _ = hub.Clients.All.SendAsync("changement", new { type = e.GetType().Name });
}

app.MapHub<HubCrc>("/hubs/crc");
```
```js
const connexion = new signalR.HubConnectionBuilder().withUrl("/hubs/crc").withAutomaticReconnect().build();
connexion.on("changement", (e) => rafraichir());
connexion.start();
```

## Les quatre moniteurs

Le navigateur ne place pas une fenêtre sur un écran au hasard : l'API **Window Management**
(`window.getScreenDetails()`, Chrome, avec l'autorisation de l'utilisateur) donne la liste des écrans et
leur position ; `window.open(url, nom, "left=…,top=…,width=…,height=…")` ouvre la vue au bon endroit.
`config/poste.json` dit quelle vue va sur quel écran : changer l'installation ne demande pas de recompiler.

## Les pièges

- **Rafraîchir à chaque événement** : à 11 h, plusieurs événements par seconde → on regroupe (au plus un rafraîchissement par seconde).
- **Garder la sélection dans une seule page** : les autres moniteurs ne la connaissent pas. L'état partagé vit côté serveur.
- **Oublier la reconnexion** : un réseau qui tousse, et le moniteur ne bouge plus. `withAutomaticReconnect()`.
- **Envoyer tout l'état à chaque fois** : le message dit *qu'*il y a un changement ; la page relit ce dont elle a besoin.

## ✅ Auto-évaluation

1. Dans le push, qui envoie le premier message après la connexion ?
2. À quoi sert `IHubContext<HubCrc>` alors qu'on a déjà la classe `HubCrc` ?
3. Pourquoi `SelectionPartagee` est-elle un singleton ?
4. Que renvoie `POST /api/selection` quand tout va bien, et pourquoi ce code ?
5. Que doit faire le lanceur de poste s'il ne détecte qu'un écran ?

<details><summary>Réponses</summary>

1. Le serveur, quand l'état change (`SendAsync("changement", …)`).
2. À envoyer des messages par le hub depuis n'importe quel service (ici `RelaisTempsReel`), en dehors d'un appel de client.
3. Il n'y a qu'une sélection pour tout le poste : tous les moniteurs (et toutes les requêtes) doivent lire la même.
4. 204 No Content : l'action a réussi et il n'y a rien à renvoyer.
5. Appliquer le repli de RG7 : ouvrir les vues en fenêtres (mosaïque) sur l'écran disponible.
</details>
