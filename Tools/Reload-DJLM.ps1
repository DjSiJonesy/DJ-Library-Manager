<#
===============================================================================
DJ Library Manager
Developer Reload Script

Reloads all DJLM modules after code changes.

===============================================================================
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Write-Host ""
Write-Host "Reloading DJ Library Manager..." -ForegroundColor Cyan
Write-Host ""

#
# Discover all module manifests
#

$ModuleManifests = Get-ChildItem `
    -Path .\Modules `
    -Directory |
ForEach-Object {

    $Manifest = Join-Path $_.FullName "$($_.Name).psd1"

    if (Test-Path $Manifest) {

        Get-Item $Manifest

    }

}

#
# Remove loaded DJLM modules
#

foreach ($Manifest in $ModuleManifests) {

    $ModuleName = $Manifest.BaseName

    if (Get-Module -Name $ModuleName) {

        Remove-Module `
            -Name $ModuleName `
            -Force `
            -ErrorAction SilentlyContinue

    }

}

#
# Import modules
#

foreach ($Manifest in ($ModuleManifests | Sort-Object BaseName)) {

    Import-Module `
        -Name $Manifest.FullName `
        -Force

}

Write-Host ""
Write-Host "Modules Reloaded Successfully." -ForegroundColor Green
Write-Host ""

Get-Module |
    Where-Object {

        $_.Name -in $ModuleManifests.BaseName

    } |
    Sort-Object Name |
    Format-Table Name, Version