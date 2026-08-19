# setup.ps1 — Automated Full Microservices Backend Launcher for Flutter (Team 1)
param (
    [string]$EnvVarName = "BASE_URL",
    [switch]$Recreate
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$EnvFile = Join-Path $ScriptDir ".env"
$ComposeFile = Join-Path $ScriptDir "docker-compose.prod.yml"

Write-Host "[setup_backend] Starting Flower E-Commerce Microservices backend (Team 1)..."

if (-not (Test-Path $EnvFile)) {
    Write-Host "[setup_backend] Creating default .env file..."
    New-Item -Path $EnvFile -ItemType File | Out-Null
}

if ($Recreate) {
    Write-Host "[setup_backend] Pulling & Recreating containers (--Recreate)..."
    docker compose --env-file $EnvFile -f $ComposeFile pull
    docker compose --env-file $EnvFile -f $ComposeFile up -d --force-recreate
} else {
    docker compose --env-file $EnvFile -f $ComposeFile up -d
}

$LanIp = (Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.IPAddress -notmatch "^127\." -and $_.IPAddress -notmatch "^169\.254" -and $_.InterfaceAlias -notmatch "vEthernet" -and $_.InterfaceAlias -notmatch "WSL" }).IPAddress | Select-Object -First 1
if (-not $LanIp) { $LanIp = "127.0.0.1" }

$HostPort = "8080"
$BaseUrl = "http://" + $LanIp + ":" + $HostPort

$Content = Get-Content $EnvFile
$Updated = $false
$NewLines = foreach ($line in $Content) {
    if ($line -match "^$EnvVarName=") {
        "$EnvVarName=$BaseUrl"
        $Updated = $true
    } else {
        $line
    }
}
if (-not $Updated) {
    $NewLines += "$EnvVarName=$BaseUrl"
}
Set-Content -Path $EnvFile -Value $NewLines

Write-Host "================================================================="
Write-Host "  Flower E-Commerce Backend (Team 1) Running Successfully!"
Write-Host "================================================================="
Write-Host "  API Gateway Base URL:     $BaseUrl"
Write-Host "  Gateway Identity Health:  $BaseUrl/identity"
Write-Host "  Gateway Catalog Health:   $BaseUrl/catalog"
Write-Host "  Gateway Cart Health:      $BaseUrl/cart"
Write-Host "  Gateway Order Health:     $BaseUrl/orders"
Write-Host "  Gateway Payment Health:   $BaseUrl/payments"
Write-Host "  Gateway Address Health:   $BaseUrl/address"
Write-Host "-----------------------------------------------------------------"
Write-Host "  Direct Microservice Swagger UI Endpoints:"
Write-Host "  Swagger Identity API: http://localhost:5022/swagger"
Write-Host "  Swagger Catalog API:  http://localhost:5129/swagger"
Write-Host "  Swagger Cart API:     http://localhost:5292/swagger"
Write-Host "  Swagger Order API:    http://localhost:5109/swagger"
Write-Host "  Swagger Payment API:  http://localhost:5260/swagger"
Write-Host "  Swagger Address API:  http://localhost:5272/swagger"
Write-Host "================================================================="
