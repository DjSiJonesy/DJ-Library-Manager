<#
===============================================================================
DJ Library Manager
Application Bootstrap

Entry point for the DJ Library Manager application.

Protect the music. Respect the DJ.
===============================================================================
#>

[CmdletBinding()]
param()

Set-StrictMode -Version Latest

$ErrorActionPreference = 'Stop'

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host " DJ Library Manager v0.1.0" -ForegroundColor Cyan
Write-Host " Protect the music. Respect the DJ." -ForegroundColor DarkGray
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

try {

    Write-Host "Loading Core Module..." -ForegroundColor Gray
    Import-Module "$PSScriptRoot\Modules\Core" -Force

    Write-Host "Loading VirtualDJ Module..." -ForegroundColor Gray
    Import-Module "$PSScriptRoot\Modules\VirtualDJ" -Force

    Write-Log "Application started." -Level Information

    $config = Get-Configuration

    Write-Log "Configuration loaded." -Level Success

    Write-Host ""
    Write-Host "DJ Library Manager is ready." -ForegroundColor Green
    Write-Host ""

}
catch {

    Write-Host ""
    Write-Host "APPLICATION STARTUP FAILED" -ForegroundColor Red
    Write-Host ""
    Write-Host $_.Exception.Message -ForegroundColor Yellow
    Write-Host ""

    exit 1

}