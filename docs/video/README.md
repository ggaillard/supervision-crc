# 🎬 Vidéo de présentation — comprendre le contexte en 7 minutes

Une vidéo à regarder **avant la séance 1** : l'organisation, le poste à quatre écrans, une matinée au centre
de relation client, ce que vous allez construire et comment travailler.

> **Lien de la vidéo :** *(à venir — l'enseignant le publie ici)*

Les fichiers de ce dossier permettent de la produire avec **NotebookLM** (Google), qui fabrique une vidéo
commentée à partir de documents.

| Fichier | Rôle |
|---|---|
| [source-contexte.md](source-contexte.md) | le contexte raconté sans code : la source principale |
| [script.md](script.md) | le découpage en 9 scènes, avec le texte de la voix et l'image : ce que la vidéo doit suivre |

---

## Produire la vidéo avec NotebookLM

1. **Créer un notebook** sur [notebooklm.google.com](https://notebooklm.google.com), nommé « Supervision CRC — présentation ».
2. **Ajouter les sources**, en important les fichiers (ou en collant leur texte) :
   - `docs/video/source-contexte.md` — **indispensable** ;
   - `docs/video/script.md` — pour que la vidéo suive le découpage ;
   - `docs/cahier-des-charges.md` — facultatif, pour les règles et les indicateurs exacts ;
   - les captures de `docs/maquettes/` — facultatif, si votre NotebookLM accepte les images.
3. Dans le panneau **Studio**, choisir **Video Overview** (*Aperçu vidéo*), puis le crayon pour le personnaliser :
   - **Format :** *Explainer* (explication structurée) ;
   - **Langue :** français ;
   - **Style visuel :** *Whiteboard* (tableau blanc) ou *Classic*. Avec *Custom*, décrire par exemple :
     « schémas sobres, tons bleu et blanc, écrans de supervision stylisés » ;
   - **Sujet à mettre en avant :** coller la consigne ci-dessous.
4. **Générer**, regarder **en entier**, puis vérifier avant de diffuser (voir plus bas).
5. **Télécharger** la vidéo, ou partager le notebook en lecture, et mettre le lien en haut de cette page.

### La consigne à coller

```text
Vidéo de présentation pour des étudiants de BTS SIO SLAM (2e année), en français, ton clair et
concret. Suis le découpage en 9 scènes du document « Script ». Explique : la mutuelle MFA et son
centre de relation client ; pourquoi la superviseure Claire Morel a besoin de 4 écrans (une question
par écran) ; la matinée du 28 octobre 2026 figée à 11 h 42 ; l'application laissée par un prestataire ;
les 8 demandes, une par séance ; les quatre repères Lire, Faire, Retenir, Pourquoi ; les tests qui
passent du rouge au vert. Termine par la première chose à faire. Ne montre aucun code.
L'organisation et les personnes sont fictives : n'invente ni marque, ni logo, ni personne réelle.
```

### Une version courte (60 secondes)

Même notebook, format **Short**, avec cette consigne :

```text
En 60 secondes, en français, pour des étudiants de BTS SIO : une superviseure de centre de relation
client pilote 18 conseillers sur 4 écrans (file d'attente, plateau, dossier, indicateurs). Une
application laissée par un prestataire ne fait presque rien. En 8 séances, l'étudiant la reprend et
la livre. Organisation fictive, aucun code.
```

### Un résumé audio

Si la vidéo n'est pas disponible, l'**Audio Overview** (*Aperçu audio*) produit un échange à deux voix à
partir des mêmes sources ; la même consigne convient.

---

## Avant de diffuser : vérifier

NotebookLM peut se tromper ou ajouter des détails. Contrôler au moins :

- [ ] les chiffres : 18 conseillers, 4 files, 313 contacts, 8 h – 12 h, arrêt à 11 h 42, niveau de service Sinistres 52 % (objectif 80 %) ;
- [ ] les noms : Mutuelle Forézienne d'Assurance (MFA), Claire Morel, Karim Belaïd — et **aucune** entreprise, marque ou personne réelle ;
- [ ] la place des écrans : file en haut à gauche, plateau en haut à droite, dossier en bas à gauche, indicateurs en bas à droite ;
- [ ] les huit demandes dans le bon ordre ;
- [ ] la mention que l'organisation est **fictive**.

*Remarques.* Les styles visuels sont réservés aux comptes de plus de 18 ans : c'est l'enseignant qui génère
la vidéo et la partage. Les options de NotebookLM évoluent ; si un nom de menu a changé, le principe reste
le même : des sources, un format, une langue, une consigne.
