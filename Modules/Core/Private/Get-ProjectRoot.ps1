function Get-ProjectRoot {
<#
.SYNOPSIS
Returns the root folder of the DJ Library Manager project.

.DESCRIPTION
Determines the project root based on the location of the Core module.

.NOTES
Private helper function.
#>

    [CmdletBinding()]
    param()

    $moduleRoot = Split-Path -Parent $PSScriptRoot

    return (Resolve-Path (
        Join-Path $moduleRoot "..\.."
    )).Path

}