# REviewer Automated Release Build Script
# Usage: .\build_release.ps1
$ErrorActionPreference = "Stop"

# Paths
$projectPath = ".\REviewer\REviewer.csproj"
$configPath = ".\REviewer\app.config"
$releaseBase = ".\Releases"

# 1. Detect Version
Write-Host "Detecting version..." -ForegroundColor Cyan
if (-not (Test-Path $configPath)) { Write-Error "Config file not found: $configPath" }

[xml]$xml = Get-Content $configPath
$version = ($xml.configuration.appSettings.add | Where-Object { $_.key -eq "Version" }).value
if ([string]::IsNullOrWhiteSpace($version)) { Write-Error "Version not found in app.config" }

Write-Host "Version: $version" -ForegroundColor Green

# 2. Clean
Write-Host "Cleaning previous builds..." -ForegroundColor Cyan
if (Test-Path $releaseBase) { Remove-Item $releaseBase -Recurse -Force }
New-Item -ItemType Directory -Path $releaseBase | Out-Null

$archs = @("win-x64", "win-x86")

foreach ($arch in $archs) {
    Write-Host "Building for $arch..." -ForegroundColor Yellow
    
    $publishDir = ".\REviewer\bin\Release\net6.0-windows\$arch\publish"
    
    # Clean publish dir first
    if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }

    # Build
    dotnet publish $projectPath -c Release -r $arch /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --output $publishDir | Out-Null
    
    if ($LASTEXITCODE -ne 0) { Write-Error "Build failed for $arch" }

    # Prepare Data
    Write-Host "Copying data files..." -ForegroundColor Gray
    $dataDir = "$publishDir\data"
    $savesDir = "$publishDir\saves"
    
    if (-not (Test-Path $dataDir)) { New-Item -ItemType Directory -Path $dataDir | Out-Null }
    if (-not (Test-Path $savesDir)) { New-Item -ItemType Directory -Path $savesDir | Out-Null }
    
    Copy-Item ".\REviewer\Resources\Files\re-data.json" -Destination $dataDir -Force
    Copy-Item ".\REviewer\Resources\Files\config.json" -Destination $dataDir -Force
    
    # Remove logs/pdb
    if (Test-Path "$publishDir\logs") { Remove-Item "$publishDir\logs" -Recurse -Force }
    if (Test-Path "$publishDir\REviewer.pdb") { Remove-Item "$publishDir\REviewer.pdb" -Force }

    # Zip
    $zipName = "REviewer-$version-$arch.zip"
    $zipPath = "$releaseBase\$zipName"
    
    Write-Host "Compressing to $zipName..." -ForegroundColor Cyan
    
    $compressionItems = @(
        "$publishDir\REviewer.exe",
        "$publishDir\REviewer.dll.config",
        "$publishDir\NLog.config",
        "$publishDir\data",
        "$publishDir\saves"
    )
    
    Compress-Archive -LiteralPath $compressionItems -DestinationPath $zipPath -Force
}

Write-Host "Success! Builds located in $releaseBase" -ForegroundColor Green
Get-ChildItem $releaseBase | Select-Object Name, Length
