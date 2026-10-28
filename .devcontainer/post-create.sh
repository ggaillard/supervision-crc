#!/usr/bin/env bash
# Préparation du projet après la création du Codespace.
# IDEMPOTENT : peut être relancé à tout moment (bash .devcontainer/post-create.sh).
set -e

echo "▶ Outils : dotnet-ef, sqlite3…"
dotnet tool update --global dotnet-ef >/dev/null 2>&1 || dotnet tool install --global dotnet-ef
grep -q '.dotnet/tools' ~/.bashrc || echo 'export PATH="$PATH:$HOME/.dotnet/tools"' >> ~/.bashrc
if ! command -v sqlite3 >/dev/null; then
  sudo apt-get update -qq && sudo apt-get install -y -qq sqlite3 >/dev/null
fi

echo "▶ Restauration et compilation…"
dotnet restore SupervisionCrc.sln
dotnet build SupervisionCrc.sln --no-restore -v q

chmod +x lancer.sh

echo ""
echo "✅ Supervision CRC est prête."
bash .devcontainer/bienvenue.sh
