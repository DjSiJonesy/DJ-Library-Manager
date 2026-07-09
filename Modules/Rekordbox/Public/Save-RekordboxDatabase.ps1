function Save-RekordboxDatabase {

<#
.SYNOPSIS
Saves a Rekordbox database.

.DESCRIPTION
Persists any outstanding changes to the Rekordbox database
and closes the database connection.

SQLite writes changes immediately, therefore this function
primarily validates the database object, optionally creates
a backup, and closes the connection cleanly.

.PARAMETER Database
A Rekordbox database object returned by
Import-RekordboxDatabase.

.PARAMETER Path
Optional destination path.

If omitted, the original database path is used.

.PARAMETER Backup
Creates a timestamped backup before closing the database.

.EXAMPLE
$db = Import-RekordboxDatabase

Save-RekordboxDatabase -Database $db

.NOTES
DJ Library Manager
#>

    [CmdletBinding(SupportsShouldProcess)]
    param(

        [Parameter(Mandatory)]
        $Database,

        [string]
        $Path,

        [switch]
        $Backup

    )

    Write-Log "Saving Rekordbox database..." -Level Information

    #
    # Validate object
    #

    if ($null -eq $Database.Connection) {

        throw "The supplied object is not a valid Rekordbox database."

    }

    #
    # Default to original path
    #

    if ([string]::IsNullOrWhiteSpace($Path)) {

        $Path = $Database.Path

    }

    #
    # Backup
    #

    if ($Backup -and (Test-Path $Path)) {

        $directory = Split-Path $Path -Parent

        $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"

        $backup = Join-Path `
            $directory `
            ("master-{0}.bak.db" -f $timestamp)

        Copy-Item `
            -Path $Path `
            -Destination $backup `
            -Force

        Write-Log "Backup created: $backup" -Level Information

    }

    #
    # Close connection
    #

    if ($PSCmdlet.ShouldProcess($Path, "Close Rekordbox database")) {

        $Database.Connection.Close()
        $Database.Connection.Dispose()

    }

    Write-Log "Rekordbox database saved." -Level Success

    return Get-Item $Path

}