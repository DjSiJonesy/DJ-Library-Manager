# ============================================================================
# Script:  New-DJLMModule.ps1
# Purpose: Creates a new DJ Library Manager module from templates.
#
# DJ Library Manager
# ============================================================================

[CmdletBinding()]
param(

    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string]$Name

)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Write-Host ""
Write-Host "Creating module '$Name'..." -ForegroundColor Cyan
Write-Host ""

# ============================================================================
# Paths
# ============================================================================

$ProjectRoot = Split-Path $PSScriptRoot -Parent

$ModulesRoot   = Join-Path $ProjectRoot 'Modules'
$TemplatesRoot = Join-Path $PSScriptRoot 'Templates'

$ModuleRoot = Join-Path $ModulesRoot $Name

# ============================================================================
# Configuration
# ============================================================================

$ModuleFolders = @(
    'Private'
    'Public'
    'Tests'
)

$ModulePsmTemplate = 'Module.psm1.template'
$ModulePsdTemplate = 'Module.psd1.template'

# ============================================================================
# Validation
# ============================================================================

if (Test-Path $ModuleRoot) {

    throw "Module '$Name' already exists."

}

if (-not (Test-Path $TemplatesRoot)) {

    throw "Template folder not found: $TemplatesRoot"

}

# ============================================================================
# Create Folder Structure
# ============================================================================

$ModuleFolders | ForEach-Object {

    $folder = Join-Path $ModuleRoot $_

    New-Item `
        -ItemType Directory `
        -Path $folder `
        -Force | Out-Null

}

# ============================================================================
# Copy Templates
# ============================================================================

Copy-Item `
    (Join-Path $TemplatesRoot $ModulePsmTemplate) `
    (Join-Path $ModuleRoot "$Name.psm1")

Copy-Item `
    (Join-Path $TemplatesRoot $ModulePsdTemplate) `
    (Join-Path $ModuleRoot "$Name.psd1")

# ============================================================================
# Token Replacement
# ============================================================================

$ModuleGuid = ([guid]::NewGuid()).Guid

$Tokens = @{

    '{{ModuleName}}' = $Name
    '{{GUID}}'       = $ModuleGuid

}

Get-ChildItem `
    -Path $ModuleRoot `
    -File |
ForEach-Object {

    $content = Get-Content `
        -Path $_.FullName `
        -Raw

    foreach ($token in $Tokens.Keys) {

        $content = $content.Replace($token, $Tokens[$token])

    }

    Set-Content `
        -Path $_.FullName `
        -Value $content `
        -Encoding UTF8

}

# ============================================================================
# Complete
# ============================================================================

Write-Host ""
Write-Host "Module created successfully." -ForegroundColor Green
Write-Host ""
Write-Host "Module :" -NoNewline
Write-Host " $Name" -ForegroundColor Yellow
Write-Host "Location:" -NoNewline
Write-Host " $ModuleRoot" -ForegroundColor Yellow
Write-Host "GUID    :" -NoNewline
Write-Host " $ModuleGuid" -ForegroundColor Yellow
Write-Host ""