#!/usr/bin/env bash
# Rappel affiché à chaque ouverture du Codespace.
cat <<'TXT'

  🖥️  Supervision CRC
     Lancer l'application   ./lancer.sh          (ou Ctrl+Maj+B)
     Avec points d'arrêt    F5
     Mode recette 11 h 42   ./lancer.sh recette
     Base neuve             ./lancer.sh neuf
     Tests d'une séance     dotnet test --filter Groupe=seance-01
     Un souci ?             DEPANNAGE.md

TXT
