# Script de preparacao e publicacao do Blazor WASM para GitHub Pages
param(
    [string]$RepoName = "PTCG-BattleMetrics",
    [switch]$PushToGhPages = $false
)

$ErrorActionPreference = "Stop"

Write-Host "Compilando Blazor WebAssembly em modo Release..." -ForegroundColor Cyan
$publishDir = Join-Path $PSScriptRoot "..\publish\gh-pages"
dotnet publish "$PSScriptRoot\..\src\Client\Client.csproj" -c Release -o $publishDir --nologo

$wwwroot = Join-Path $publishDir "wwwroot"
$indexPath = Join-Path $wwwroot "index.html"
$notFoundPath = Join-Path $wwwroot "404.html"
$noJekyllPath = Join-Path $wwwroot ".nojekyll"

Write-Host "Configurando base href para GitHub Pages (/$RepoName/)..." -ForegroundColor Cyan
$content = Get-Content -Path $indexPath -Raw
$content = $content -replace '<base href="/" />', ('<base href="/' + $RepoName + '/" />')
Set-Content -Path $indexPath -Value $content -NoNewline

Write-Host "Gerando 404.html para roteamento SPA..." -ForegroundColor Cyan
Copy-Item -Path $indexPath -Destination $notFoundPath -Force

Write-Host "Criando .nojekyll..." -ForegroundColor Cyan
New-Item -ItemType File -Path $noJekyllPath -Force | Out-Null

Write-Host "Pacote pronto em: $wwwroot" -ForegroundColor Green

if ($PushToGhPages) {
    Write-Host "Publicando diretamente na branch gh-pages..." -ForegroundColor Yellow
    Push-Location $wwwroot
    try {
        git init
        git checkout -B gh-pages
        git add .
        git commit -m "Deploy GitHub Pages PWA"
        $remoteUrl = git -C "$PSScriptRoot\.." config --get remote.origin.url
        if ($remoteUrl) {
            git remote add origin $remoteUrl
            git push -u origin gh-pages --force
            Write-Host "Publicado com sucesso na branch gh-pages!" -ForegroundColor Green
        }
    }
    finally {
        Pop-Location
    }
}
else {
    Write-Host "`nPara publicar automaticamente via GitHub Actions:" -ForegroundColor Yellow
    Write-Host "1. Faca commit e 'git push origin main'"
    Write-Host "2. No GitHub: Settings -> Pages -> Build and deployment -> Source: GitHub Actions"
    Write-Host "Seu app estara no ar em: https://luizhbrandao.github.io/$RepoName/" -ForegroundColor Cyan
}
