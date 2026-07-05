function Get-DJLMVersion {

<#
.SYNOPSIS
Returns the current DJ Library Manager version.

.DESCRIPTION
Reads the application version from the root DJLM.psd1
manifest.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param()

    #
    # Locate project root
    #

        $ProjectRoot = $PSScriptRoot

        for ($i = 0; $i -lt 3; $i++) {

        $ProjectRoot = Split-Path $ProjectRoot -Parent

    }

    $ManifestPath = Join-Path $ProjectRoot 'DJLM.psd1'

    if (-not (Test-Path $ManifestPath)) {

        throw "Unable to locate application manifest: $ManifestPath"

    }

    $Manifest = Import-PowerShellDataFile $ManifestPath

    return $Manifest.ApplicationVersion

}