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

$modules = @(
    'VirtualDJ',
    'Core'
)

foreach ($module in $modules) {

    Remove-Module $module -ErrorAction SilentlyContinue

}

Get-ChildItem .\Modules -Directory | ForEach-Object {

    $manifest = Join-Path $_.FullName "$($_.Name).psd1"

    if (Test-Path $manifest) {

        Import-Module $manifest -Force

    }

}

Write-Host ""
Write-Host "Modules Reloaded Successfully." -ForegroundColor Green
Write-Host ""