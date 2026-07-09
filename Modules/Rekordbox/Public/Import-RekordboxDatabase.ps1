function Import-RekordboxDatabase {

<#
.SYNOPSIS
Imports a Rekordbox database.

.DESCRIPTION
Loads the configured Rekordbox database and returns a
DJ Library Manager Rekordbox database object.

If no path is supplied, the configured database path from
Settings.json is used.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [string]
        $Path

    )

    $configuration = Get-Configuration

    #
    # Use configured path if one wasn't supplied
    #

    if ([string]::IsNullOrWhiteSpace($Path)) {

        if (-not $configuration.Providers.Rekordbox.DatabasePath) {

            throw "Providers.Rekordbox.DatabasePath is not configured in Settings.json."

        }

        $Path = $configuration.Providers.Rekordbox.DatabasePath

    }

    Write-Log "Importing Rekordbox database..." -Level Information

    $connection = Open-RekordboxDatabase `
        -Path $Path `
        -SqlCipherKey $configuration.Providers.Rekordbox.SqlCipherKey

    if ($null -eq $connection) {

        throw "Failed to open the Rekordbox database."

    }

    Write-Log "Rekordbox database imported." -Level Success

    return [PSCustomObject]@{

        PSTypeName = 'DJLM.RekordboxDatabase'

        Path = (Resolve-Path $Path).Path

        Connection = $connection

        Loaded = Get-Date

    }

}