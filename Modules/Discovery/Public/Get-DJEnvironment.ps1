function Get-DJEnvironment {

<#
.SYNOPSIS
Builds the current DJ Library Manager environment.

.DESCRIPTION
Discovers the current DJ environment and returns a
DJLM.Environment object.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param()

    Write-Log "Building DJ environment..." -Level Information

    $providers = Find-DJProviders

    $databases = Find-ProviderDatabases

    $drives = Find-LibraryDrives

    $libraries = Find-MusicLibraries -Drives $drives

    $environment = [PSCustomObject]@{

        PSTypeName = 'DJLM.Environment'

        Created = Get-Date

        Providers = $providers

        Databases = $databases

        Drives = $drives

        Libraries = $libraries

        Warnings = @()

    }

    Write-Log "DJ environment built." -Level Success

    return $environment

}