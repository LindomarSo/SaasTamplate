#!/usr/bin/env pwsh
# Instala o template SaaS Starter localmente.
# Execute uma vez — depois use 'dotnet new saas-starter' quantas vezes quiser.

$templateFolder = $PSScriptRoot

Write-Host ""
Write-Host "Instalando SaaS Starter template..." -ForegroundColor Cyan

dotnet new install $templateFolder

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "Template instalado com sucesso!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Como usar:" -ForegroundColor Yellow
    Write-Host "  dotnet new saas-starter -n NomeDoProjeto -o ./NomeDoProjeto" -ForegroundColor White
    Write-Host ""
    Write-Host "Exemplo:" -ForegroundColor Yellow
    Write-Host "  dotnet new saas-starter -n MinhaPlataforma -o ./MinhaPlataforma" -ForegroundColor White
    Write-Host ""
    Write-Host "Para desinstalar:" -ForegroundColor Yellow
    Write-Host "  dotnet new uninstall $templateFolder" -ForegroundColor White
} else {
    Write-Host ""
    Write-Host "Falha na instalacao. Verifique se o .NET SDK esta instalado." -ForegroundColor Red
    exit 1
}
