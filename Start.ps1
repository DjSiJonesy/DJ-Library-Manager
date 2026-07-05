<#
===============================================================================
DJ Library Manager
Application Bootstrap

Entry point for the DJ Library Manager application.
===============================================================================
#>

[CmdletBinding()]
param()

Set-StrictMode -Version Latest

$ErrorActionPreference = 'Stop'

try {

    #
    # Import all modules
    #

Get-ChildItem "$PSScriptRoot\Modules" -Directory |
    Where-Object {
        Test-Path (Join-Path $_.FullName "$($_.Name).psd1")
    } |
    Sort-Object Name |
    ForEach-Object {
        Import-Module $_.FullName -Force
    }

    #
    # Start the application
    #

    Start-DJLM

}
catch {

    Write-Host ""
    Write-Host "DJ Library Manager failed to start." -ForegroundColor Red
    Write-Host ""

    Write-Host $_.Exception.Message -ForegroundColor Yellow
    Write-Host ""

    exit 1

}