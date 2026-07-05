function Write-Banner {

<#
.SYNOPSIS
Displays the DJ Library Manager application banner.
#>

    [CmdletBinding()]
    param()

    $Version = Get-DJLMVersion

    $Line = ("═" * 62)

    Write-Host $Line
    Write-Host
    Write-Host "               DJ Library Manager"
    Write-Host
    Write-Host "                  v$Version"
    Write-Host
    Write-Host "   Intelligent Music Management for Professional DJs."
    Write-Host
    Write-Host $Line
    Write-Host

}