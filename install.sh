#!/usr/bin/env bash
# Instala o template SaaS Starter localmente.
# Execute uma vez — depois use 'dotnet new saas-starter' quantas vezes quiser.

set -e

TEMPLATE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo ""
echo "Instalando SaaS Starter template..."

dotnet new install "$TEMPLATE_DIR"

echo ""
echo "Template instalado com sucesso!"
echo ""
echo "Como usar:"
echo "  dotnet new saas-starter -n NomeDoProjeto -o ./NomeDoProjeto"
echo ""
echo "Exemplo:"
echo "  dotnet new saas-starter -n MinhaPlataforma -o ./MinhaPlataforma"
echo ""
echo "Para desinstalar:"
echo "  dotnet new uninstall \"$TEMPLATE_DIR\""
