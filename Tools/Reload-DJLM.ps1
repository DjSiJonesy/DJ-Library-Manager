<#
===============================================================================
DJ Library Manager
Developer Reload Script

Reloads all DJLM modules after code changes.

===============================================================================
#>

Set-StrictMode -Version Latest

Write-Host ""
Write-Host "Reloading DJ Library Manager..." -ForegroundColor Cyan
Write-Host ""

#
# Remove all DJLM modules
#

Get-Module |
    Where-Object {
        $_.Name -in @(
            'Core',
            'VirtualDJ',
            'Library',
            'Analysis',
            'Dashboard',
            'Recovery',
            'Discovery'
        )
    } |
    Remove-Module -Force -ErrorAction SilentlyContinue

#
# Import every module manifest
#

Get-ChildItem .\Modules -Directory |
    Sort-Object Name |
    ForEach-Object {

        $Manifest = Join-Path $_.FullName "$($_.Name).psd1"

        if (Test-Path $Manifest) {

            Import-Module $Manifest -Force

        }

    }

Write-Host ""
Write-Host "Modules Reloaded Successfully." -ForegroundColor Green
Write-Host ""

Get-Module |
    Where-Object {
        $_.Name -in @(
            'Analysis',
            'Core',
            'Dashboard',
            'Discovery',
            'Library',
            'Recovery',
            'VirtualDJ'
        )
    } |
    Sort-Object Name |
    Format-Table Name, Version