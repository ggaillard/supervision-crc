# Supervision CRC — le contexte de la situation professionnelle

*Source écrite pour NotebookLM : elle raconte la situation sans code. Toutes les personnes,
l'organisation et les chiffres sont fictifs.*

## L'organisation : la Mutuelle Forézienne d'Assurance

La Mutuelle Forézienne d'Assurance, la MFA, est une mutuelle d'assurance installée à Saint-Étienne.
Elle compte 240 salariés et 85 000 sociétaires. Un sociétaire est un client de la mutuelle, qui en est
aussi membre : il a souscrit un contrat d'assurance habitation, auto ou santé.

Quand un sociétaire a une question, un sinistre à déclarer ou une réclamation, il contacte le **centre de
relation client**, le CRC : par téléphone, par chat ou par e-mail. Le CRC est un service du secteur
tertiaire comme on en trouve dans toutes les banques, assurances, mutuelles ou opérateurs.

## Le centre de relation client

Le plateau compte **18 conseillers**. Les contacts arrivent dans **quatre files** : Sinistres, Contrats,
Santé et Accueil général. Chaque conseiller sait traiter certaines files, pas toutes ; certains sont
référents sur une file.

Un conseiller a toujours un statut : disponible, en appel, en chat, en post-appel (il rédige le compte rendu
du contact précédent), en pause, en formation ou hors ligne. Seul un conseiller disponible peut recevoir un
nouveau contact.

Certains contacts sont plus urgents que d'autres. Une **escalade** est un contact remonté parce que le
conseiller ne peut pas le régler seul : un sociétaire très mécontent, une réclamation. Viennent ensuite les
priorités hautes, puis les priorités normales.

Deux **superviseurs** pilotent le plateau. La responsable du centre s'appelle **Claire Morel**. Son métier :
faire en sorte que personne n'attende trop, qu'aucun conseiller ne reste seul face à une difficulté, et que
les objectifs de la journée soient tenus.

## Les chiffres que suit la direction

La direction de la MFA suit cinq indicateurs chaque jour :

- le **taux de décroché** : la part des contacts pris en charge, plutôt qu'abandonnés par un sociétaire qui
  a raccroché. Objectif : au moins 95 %.
- la **durée moyenne de traitement** d'un contact. Objectif : 7 minutes au plus.
- la **résolution au premier contact** : la part des demandes réglées sans rappel. Objectif : 75 %.
- la **satisfaction à chaud**, une note de 1 à 5 donnée par le sociétaire. Objectif : 4 sur 5.
- le **niveau de service** de chaque file : la part des contacts décrochés en moins de 20 secondes, sur les
  30 dernières minutes. Objectif : 80 %. S'il reste sous 80 % pendant 10 minutes, c'est une alerte.

Les réclamations obéissent à des délais : un accusé de réception sous 10 jours ouvrables, une réponse sous
deux mois. Une échéance qui approche est une alerte.

## Pourquoi un poste à quatre moniteurs

Le poste de Claire Morel a **quatre écrans**, disposés en carré. Ce n'est pas un luxe : elle doit voir
**en même temps** quatre choses qui changent sans arrêt. Des onglets en cacheraient trois sur quatre, et
elle raterait l'essentiel.

1. **Le moniteur 1, la file d'attente**, répond à la question : qui attend, depuis combien de temps, pour
   quel motif ? Les escalades doivent sauter aux yeux.
2. **Le moniteur 2, le plateau**, répond à : qui est disponible, qui est en difficulté ? Un conseiller en
   appel depuis plus de 10 minutes a peut-être besoin d'aide.
3. **Le moniteur 3, le dossier**, répond à : que se passe-t-il sur le cas que je suis ? Quand Claire clique
   sur un conseiller du plateau, le dossier du sociétaire qu'il a en ligne s'ouvre ici : ses contrats, ses
   sinistres, ses réclamations, les notes.
4. **Le moniteur 4, les indicateurs**, répond à : l'objectif de la journée est-il tenu ? On y voit les
   chiffres de la direction et les alertes à prendre en charge.

Les quatre écrans sont reliés : une action sur l'un se voit tout de suite sur les autres. Affecter un
contact sur le moniteur 1 fait passer le conseiller « en appel » sur le moniteur 2. Sélectionner un
conseiller sur le moniteur 2 ouvre son dossier sur le moniteur 3.

Techniquement, c'est une **application web** : chaque moniteur affiche une page dans un navigateur, et
une page de lancement ouvre les quatre pages, chacune sur son écran.

## Une matinée au CRC

Pour développer sans téléphonie réelle, l'application rejoue une matinée enregistrée : le mercredi
**28 octobre 2026, de 8 h à midi**, avec **313 contacts**. Elle la rejoue vingt fois plus vite : quatre
heures passent en douze minutes. On voit la file se remplir au pic de 10 h à 11 h, des conseillers partir en
pause, une escalade arriver vers 11 h 29.

À 11 h 42, l'image est parlante. Sept contacts attendent en Sinistres, le plus ancien depuis plus de trois
minutes. Un conseiller, Karim Belaïd, est en appel depuis plus de douze minutes : il est en alerte. Le
niveau de service de la file Sinistres est tombé à 52 %, loin des 80 % visés. Cinq alertes attendent d'être
acquittées. C'est exactement le moment où une superviseure a besoin de ses quatre écrans.

## Le point de départ : une application laissée par un prestataire

Un prestataire a commencé l'application, puis il est parti. Elle tourne, mais elle fait très peu : un seul
écran, une file d'attente affichée dans l'ordre d'arrivée, des compétences saisies en texte libre, et une
recherche de sociétaires qui cache une faille de sécurité. Personne ne le sait encore.

C'est la situation la plus courante en entreprise : on ne part presque jamais d'une page blanche. On
**reprend** une application existante, on la comprend, puis on la fait évoluer sans la casser.

## Le rôle de l'étudiant

L'étudiant de BTS SIO option SLAM joue le développeur qui reprend l'application. Claire Morel lui envoie
une demande par séance, sous forme de message. En huit séances de deux heures :

1. **Trier la file d'attente** : les escalades d'abord, puis l'ancienneté ; afficher les attentes.
2. **Réparer le modèle de données** : remplacer les compétences en texte par une vraie table, sans perdre
   aucune donnée d'une base déjà en service, et garder l'historique des statuts.
3. **Construire le moniteur 2** : le plateau des conseillers, avec des alertes de durée dont le seuil se
   règle sans toucher au code.
4. **Affecter un contact** d'un clic, au bon conseiller, sans jamais l'affecter deux fois quand deux
   superviseurs cliquent ensemble.
5. **Passer au temps réel** : les écrans se mettent à jour d'eux-mêmes au lieu de se recharger ; chaque vue
   s'ouvre sur son écran.
6. **Sécuriser** : une connexion pour les superviseurs, des mots de passe jamais stockés en clair, le
   moniteur 3 avec le dossier, et un journal de qui a fait quoi.
7. **Calculer les indicateurs** du moniteur 4 et les alertes, avec des dates en jours ouvrables.
8. **Préparer la mise en service** : un audit qui révèle la faille, des tests écrits par l'étudiant, une
   recette avec Claire Morel sur la matinée du 28 octobre, et la fiche du dossier E5.

## Comment l'étudiant travaille

Chaque séance suit le même chemin : **lancer** l'application pour voir ce qui marche, **comprendre** le code
existant, **modifier** en suivant des missions de plus en plus autonomes, puis **valider**.

Quatre repères organisent tous les documents :

- **Lire** : la fiche de cours avant la séance, le message de Claire Morel, le code existant. Il n'y a rien
  à produire.
- **Faire** : chaque mission laisse une trace vérifiable. Le code, vérifié par des tests automatiques. Les
  réponses écrites, dans un carnet. Un commit à la fin de la séance.
- **Retenir** : cinq notions par séance, interrogées au début de la séance suivante, sans machine.
- **Pourquoi** : ce que la séance apporte à Claire Morel, et ce qu'elle apprend du métier de développeur.

Chaque guide de séance commence par une feuille de route qui reprend ces quatre repères.

Les **tests** sont le cahier des charges exécutable. Au départ, ils sont rouges : ils décrivent ce qu'il
reste à construire. Une mission est terminée quand ses tests passent au vert. Les missions vertes sont
guidées pas à pas, les jaunes donnent un indice, les rouges sont facultatives et laissent l'étudiant seul
face à l'objectif.

Tout se fait dans un environnement en ligne, un Codespace : rien à installer. Un raccourci lance
l'application, et le navigateur s'ouvre dessus.

## Ce que l'étudiant en retire

À la fin, il sait reprendre une application existante, traduire des règles de gestion en code et les
prouver par des tests, faire évoluer une base de données en service, exposer une API qui résiste aux accès
simultanés, diffuser un état en temps réel à plusieurs écrans, protéger une application et la préparer pour
l'exploitation.

C'est une réalisation complète, dans une organisation crédible, qu'il présente dans son dossier pour
l'épreuve E5 du BTS SIO.
