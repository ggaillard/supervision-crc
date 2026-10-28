# Indicateurs et dates

> **Séance 7** · Lecture 7 min · ▶️ [Faire la séance](../seances/seance-07.md)

## L'idée

Un **indicateur** est une fraction dont on a fixé, avant de coder, ce qui compte au numérateur, au
dénominateur, et ce qu'on **exclut**. Deux développeurs qui codent la même définition doivent trouver le
même chiffre — c'est ce que vérifient les tests sur la matinée du 28 octobre.

| Indicateur | Numérateur | Dénominateur | Exclus |
|---|---|---|---|
| Taux de décroché | contacts décrochés | décrochés + abandonnés | ceux qui attendent encore |
| Durée moyenne de traitement | Σ (clos − décroché) | contacts clos | non clos |
| Résolution au 1er contact | clos résolus | clos avec l'information | — |
| Niveau de service (RG3) | décrochés en < 20 s | arrivés dans la fenêtre | arrivés il y a moins de 20 s |

**Fenêtre glissante** : le niveau de service porte sur les 30 dernières minutes ; à chaque minute la
fenêtre avance. L'alerte RG3 exige que le niveau soit sous 80 % **à chaque minute** des 10 dernières :
un creux isolé ne suffit pas.

**Jours ouvrables** : on avance d'un jour à la fois et on ne compte que du lundi au vendredi, hors jours fériés.

```
Réclamation reçue le jeudi 5/11/2026 → accusé de réception avant :
ve 6 (1) · lu 9 (2) · ma 10 (3) · me 11 FÉRIÉ · je 12 (4) · ve 13 (5) · lu 16 (6) · ma 17 (7) · me 18 (8) · je 19 (9) · ve 20 (10)
```

## Le code à reconnaître

```csharp
var retenus = contacts.Where(c => c.Arrivee > maintenant - fenetre && c.Arrivee <= maintenant - seuil).ToList();
if (retenus.Count == 0) return null;                    // pas de chiffre plutôt qu'un faux 0 %
return 100.0 * retenus.Count(c => c.Decroche - c.Arrivee < seuil) / retenus.Count;
```
```csharp
for (var ouvres = 0; ouvres < 10;)
{
    jour = jour.AddDays(1);
    if (jour.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday) && !JoursFeries.Contains(jour)) ouvres++;
}
```

## Les pièges

- **Division par zéro** : à 8 h 00, aucun contact. `null` ou 0, mais décidé et documenté — jamais une exception.
- **Division entière** : `3 / 4` vaut 0 en C#. `100.0 *` force le calcul en `double`.
- **Compter les contacts en cours** : celui qui attend depuis 5 s n'est ni à temps ni en retard.
- **Supprimer l'alerte acquittée** : on perd la preuve que quelqu'un l'a vue et quand (RG6).

## ✅ Auto-évaluation

1. 40 contacts décrochés, 2 abandonnés, 3 en attente : taux de décroché ?
2. Pourquoi renvoyer `null` plutôt que 0 % quand la fenêtre est vide ?
3. Une réclamation reçue le vendredi 30 octobre 2026 : avant quelle date l'accusé de réception ?
4. Pourquoi l'alerte RG3 regarde-t-elle chaque minute des 10 dernières, et pas seulement maintenant ?
5. Relancer deux fois `POST /api/alertes/generer` crée-t-il des doublons ? Pourquoi ?

<details><summary>Réponses</summary>

1. 40 / (40 + 2) ≈ 95,2 % ; les 3 en attente ne comptent pas.
2. 0 % dirait « personne n'a été servi à temps » : c'est faux, il n'y avait personne. `null` s'affiche « — ».
3. Le lundi 16 novembre 2026 : on compte le 2, 3, 4, 5, 6, 9, 10, 12, 13 et 16 novembre (le 11 est férié, le 1er tombe un dimanche).
4. Pour ne pas alerter sur un creux d'une minute : la règle veut une dégradation qui dure.
5. Non : une alerte du même type sur la même cible, encore ouverte, n'est pas levée une seconde fois.
</details>
