function Save-VirtualDJDatabase {

<#
.SYNOPSIS
Saves a VirtualDJ database to disk.

.DESCRIPTION
Writes a VirtualDJ database object to disk.

If the destination already exists, a timestamped backup can
be created before the new database is written.

.PARAMETER Database
A VirtualDJ database object returned by
Import-VirtualDJDatabase.

.PARAMETER Path
Optional destination path.

If omitted, the original database path is used.

.PARAMETER Backup
Creates a timestamped backup before saving.

.EXAMPLE
$db = Import-VirtualDJDatabase

Save-VirtualDJDatabase -Database $db

.EXAMPLE
Save-VirtualDJDatabase `
    -Database $db `
    -Path "$env:TEMP\TestDatabase.xml"

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

    Write-Log "Saving VirtualDJ database..." -Level Information

    #
    # Validate object
    #

    if ($null -eq $Database.Xml) {

        throw "The supplied object is not a valid VirtualDJ database."

    }

    #
    # Default to original path
    #

    if ([string]::IsNullOrWhiteSpace($Path)) {

        $Path = $Database.Path

    }

    $directory = Split-Path $Path -Parent

    if (-not (Test-Path $directory)) {

        New-Item `
            -ItemType Directory `
            -Path $directory `
            -Force | Out-Null

    }

    #
    # Backup
    #

    if ($Backup -and (Test-Path $Path)) {

        $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"

        $backup = Join-Path `
            $directory `
            ("database-{0}.bak.xml" -f $timestamp)

        Copy-Item `
            -Path $Path `
            -Destination $backup `
            -Force

        Write-Log "Backup created: $backup" -Level Information

    }

    #
    # Save
    #

    if ($PSCmdlet.ShouldProcess($Path, "Save VirtualDJ database")) {

        $Database.Xml.Save($Path)

    }

    Write-Log "VirtualDJ database saved." -Level Success

    return Get-Item $Path

}