# Script para compilar e gerar o APK Android do PTCG Battle Metrics
param(
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

Write-Host "Compilando projeto Android ($Configuration)..." -ForegroundColor Cyan
$projectPath = Join-Path $PSScriptRoot "..\src\Android\PTCGBattleMetrics.Android.csproj"
dotnet build $projectPath -c $Configuration

$binDir = Join-Path $PSScriptRoot "..\src\Android\bin\$Configuration"
$apkFile = Get-ChildItem -Path $binDir -Recurse -Filter "*Signed.apk" | Select-Object -First 1

if (-not $apkFile) {
    $apkFile = Get-ChildItem -Path $binDir -Recurse -Filter "*.apk" | Select-Object -First 1
}

if ($apkFile) {
    $outDir = Join-Path $PSScriptRoot "..\publish\apk"
    if (-not (Test-Path $outDir)) {
        New-Item -ItemType Directory -Path $outDir -Force | Out-Null
    }

    $destApk = Join-Path $outDir "PTCGBattleMetrics.apk"
    Copy-Item -Path $apkFile.FullName -Destination $destApk -Force

    Write-Host "`n========================================================" -ForegroundColor Green
    Write-Host "  APK GERADO COM SUCESSO!" -ForegroundColor Green
    Write-Host "========================================================" -ForegroundColor Green
    Write-Host "Arquivo: $destApk" -ForegroundColor Yellow
    Write-Host "Tamanho: $([math]::Round($apkFile.Length / 1MB, 2)) MB" -ForegroundColor Yellow
    Write-Host "`nComo instalar no celular:" -ForegroundColor Cyan
    Write-Host "1. Conecte o celular via cabo USB ou envie o arquivo 'PTCGBattleMetrics.apk' pelo WhatsApp / Google Drive."
    Write-Host "2. No celular, abra o arquivo e confirme a instalacao."
    Write-Host "3. O app abrira em tela cheia com o icone do PTCG Battle Metrics!"
}
else {
    Write-Error "Nenhum arquivo APK foi encontrado apos a compilacao."
}
