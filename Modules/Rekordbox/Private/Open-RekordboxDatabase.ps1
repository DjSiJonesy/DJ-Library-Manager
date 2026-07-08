function Open-RekordboxDatabase {

<#
.SYNOPSIS
Opens a Rekordbox SQLCipher database.

.DESCRIPTION
Loads the DJLM SQLCipher helper library and opens an encrypted
Rekordbox database.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [string]
        $Path,

        [Parameter(Mandatory)]
        [string]
        $SqlCipherKey

    )

    Write-Log "Opening Rekordbox database..." -Level Information

    if (-not (Test-Path $Path)) {

        throw "Database not found:`n$Path"

    }

    #
    # Locate SQLCipher runtime
    #

#
# Determine the project root
#

        $projectRoot = Split-Path (
            Split-Path (
                Split-Path $PSScriptRoot -Parent
            ) -Parent
        ) -Parent

        $libraryRoot = Join-Path `
            $projectRoot `
    'Libraries\SqlCipher'

    $assembly = Join-Path `
        $libraryRoot `
        'SqlCipher.dll'

    if (-not (Test-Path $assembly)) {

        throw "SqlCipher.dll not found:`n$assembly"

    }

    #
    # Load helper library once
    #

    if (-not ('DJLM.SqlCipher.SqlCipherDatabase' -as [type])) {

        Add-Type -Path $assembly

    }

    try {

        $connection = [DJLM.SqlCipher.SqlCipherDatabase]::Open(
            $Path,
            $SqlCipherKey
        )

    }
    catch {

        throw "Failed to open Rekordbox database.`n$($_.Exception.Message)"

    }

    Write-Log "Rekordbox database opened." -Level Success

    return $connection

}