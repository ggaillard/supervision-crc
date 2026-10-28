#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
#  🖥️  Lancer la supervision CRC
#
#    ./lancer.sh            la matinée simulée se déroule (20 fois plus vite que la réalité)
#    ./lancer.sh recette    la matinée figée à 11 h 42 : les valeurs de la recette (séance 8)
#    ./lancer.sh recette 12:00   … figée à l'heure choisie (08:00 à 12:00)
#    ./lancer.sh neuf       repartir d'une base neuve (supprime crc.db), puis lancer
#    ./lancer.sh arreter    arrêter l'application qui tourne
#
#  Une application déjà lancée (autre terminal, F5…) est arrêtée d'abord : pas de
#  « address already in use ». Ctrl+C pour arrêter.
#  Dans VS Code : Ctrl+Maj+B fait la même chose que ./lancer.sh
# ─────────────────────────────────────────────────────────────────────────────
set -euo pipefail
cd "$(dirname "$0")"

PROJET=src/Mfa.Crc.Supervision
PORT=5000
MODE="${1:-normal}"

port_occupe() { (exec 3<>"/dev/tcp/127.0.0.1/$PORT") 2>/dev/null; }

arreter() {
  pkill -f "[b]in/Debug/net[0-9.]*/Mfa\.Crc\.Supervision" 2>/dev/null || true
  pkill -f "[d]otnet run --project $PROJET" 2>/dev/null || true
  for _ in $(seq 1 20); do port_occupe || return 0; sleep 0.5; done
  echo "⚠️  Le port $PORT est encore occupé : fermez le terminal où tourne l'application."
  exit 1
}

case "$MODE" in
  normal|recette|neuf) ;;
  arreter|stop) arreter; echo "⏹  Application arrêtée."; exit 0 ;;
  *) sed -n '4,9p' "$0" | sed 's/^#//'; exit 1 ;;
esac

HEURE="${2:-11:42}"
if [[ "$MODE" == recette ]] && ! [[ "$HEURE" =~ ^(0[89]|1[01]):[0-5][0-9]$|^12:00$ ]]; then
  echo "⚠️  Heure de recette entre 08:00 et 12:00, par exemple : ./lancer.sh recette 09:30"
  exit 1
fi

arreter

if [[ "$MODE" == neuf ]]; then
  rm -f "$PROJET"/crc.db "$PROJET"/crc.db-shm "$PROJET"/crc.db-wal
  echo "♻  Base supprimée : elle sera recréée par les migrations au démarrage."
fi

echo "🔨 Compilation…"
if ! dotnet build "$PROJET" -v q -nologo -clp:ErrorsOnly; then
  echo ""
  echo "❌ La compilation échoue : lisez la PREMIÈRE erreur ci-dessus (fichier et ligne), puis relancez."
  exit 1
fi

if [[ -n "${CODESPACE_NAME:-}" ]]; then
  URL="https://${CODESPACE_NAME}-${PORT}.${GITHUB_CODESPACES_PORT_FORWARDING_DOMAIN:-app.github.dev}"
else
  URL="http://localhost:${PORT}"
fi

if [[ "$MODE" == recette ]]; then
  export Simulation__ArreterA="${HEURE}:00"
  HORLOGE="figée à ${HEURE/:/ h } (mode recette)"
else
  HORLOGE="la matinée se déroule, ×20"
fi

cat <<EOF

  🖥️  Supervision CRC — $HORLOGE

     Application     $URL
     Moniteurs       $URL/moniteur/1  …  /moniteur/4
     Poste 4 écrans  $URL/poste
     Compte          cmorel / Supervision2026!   (à partir de la séance 6)

     Ctrl+clic sur l'adresse pour l'ouvrir · Ctrl+C pour arrêter

EOF

exec dotnet run --project "$PROJET" --no-build --launch-profile http
