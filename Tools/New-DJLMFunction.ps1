# ============================================================================
# Script:  New-DJLMFunction.ps1
# Purpose: Creates a new DJ Library Manager function from templates.
#
# DJ Library Manager
# ============================================================================

[CmdletBinding()]
param(

    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string]$Module,

    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string]$Name,

    [Parameter(Mandatory, ParameterSetName = 'Public')]
    [switch]$Public,

    [Parameter(Mandatory, ParameterSetName = 'Private')]
    [switch]$Private,

    [string]$Synopsis = 'Function synopsis.',

    [string]$Description = 'Function description.'

)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Write-Host ""
Write-Host "Creating function '$Name'..." -ForegroundColor Cyan
Write-Host ""

# ============================================================================
# Paths
# ============================================================================

$ProjectRoot = Split-Path $PSScriptRoot -Parent

$ModulesRoot   = Join-Path $ProjectRoot 'Modules'
$TemplatesRoot = Join-Path $PSScriptRoot 'Templates'

$ModuleRoot = Join-Path $ModulesRoot $Module

# ============================================================================
# Configuration
# ============================================================================

$PublicTemplate  = 'Function.Public.ps1.template'
$PrivateTemplate = 'Function.Private.ps1.template'

# ============================================================================
# Validation
# ============================================================================

if (-not (Test-Path $ModuleRoot)) {

    throw "Module '$Module' does not exist."

}

if ($PSCmdlet.ParameterSetName -eq 'Public') {

    $FunctionFolder = Join-Path $ModuleRoot 'Public'
    $Template = $PublicTemplate

}
else {

    $FunctionFolder = Join-Path $ModuleRoot 'Private'
    $Template = $PrivateTemplate

}

$FunctionFile = Join-Path $FunctionFolder "$Name.ps1"

if (Test-Path $FunctionFile) {

    throw "Function '$Name' already exists."

}

# ============================================================================
# Create Function
# ============================================================================

Copy-Item `
    (Join-Path $TemplatesRoot $Template) `
    $FunctionFile

# ============================================================================
# Token Replacement
# ============================================================================

$Tokens = @{

    '{{FunctionName}}' = $Name
    '{{Synopsis}}'     = $Synopsis
    '{{Description}}'  = $Description

}

$content = Get-Content `
    -Path $FunctionFile `
    -Raw

foreach ($token in $Tokens.Keys) {

    $content = $content.Replace($token, $Tokens[$token])

}

Set-Content `
    -Path $FunctionFile `
    -Value $content `
    -Encoding UTF8

# ============================================================================
# Complete
# ============================================================================

Write-Host ""
Write-Host "Function created successfully." -ForegroundColor Green
Write-Host ""
Write-Host "Module  :" -NoNewline
Write-Host " $Module" -ForegroundColor Yellow
Write-Host "Function:" -NoNewline
Write-Host " $Name" -ForegroundColor Yellow
Write-Host "Type    :" -NoNewline
Write-Host " $($PSCmdlet.ParameterSetName)" -ForegroundColor Yellow
Write-Host "Location:" -NoNewline
Write-Host " $FunctionFile" -ForegroundColor Yellow
Write-Host ""