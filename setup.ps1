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

Write-Host "Backend Running Successfully!"
Write-Host "API Gateway Base URL:  $BaseUrl"
Write-Host "Identity Health Check: $BaseUrl/identity"
Write-Host "Catalog Health Check:  $BaseUrl/catalog"
Write-Host "Cart Health Check:     $BaseUrl/cart"
Write-Host "Order Health Check:    $BaseUrl/orders"
Write-Host "Payment Health Check:  $BaseUrl/payments"
Write-Host "Address Health Check:  $BaseUrl/address"
