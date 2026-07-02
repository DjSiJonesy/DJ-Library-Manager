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

Import-Module "$PSScriptRoot\..\Modules\Core" -Force
Import-Module "$PSScriptRoot\..\Modules\VirtualDJ" -Force

Write-Host ""
Write-Host "Modules Reloaded Successfully." -ForegroundColor Green
Write-Host ""