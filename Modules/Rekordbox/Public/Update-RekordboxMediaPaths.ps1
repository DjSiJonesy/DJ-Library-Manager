function Update-RekordboxMediaPaths {

<#
.SYNOPSIS
Updates media file paths in a Rekordbox database.

.DESCRIPTION
Updates the location of media files after they have been
moved on disk.

.PARAMETER Database
A Rekordbox database object returned by
Import-RekordboxDatabase.

.PARAMETER MovedFiles
Collection returned by Find-MovedFiles.

.NOTES
DJ Library Manager
#>

    [CmdletBinding(SupportsShouldProcess)]
    param(

        [Parameter(Mandatory)]
        $Database,

        [Parameter(Mandatory)]
        [object[]]
        $MovedFiles

    )

    Write-Log "Updating Rekordbox media paths..." -Level Information

    if ($null -eq $Database.Connection) {

        throw "The supplied object is not a valid Rekordbox database."

    }

    $updated = 0

    foreach ($move in $MovedFiles) {

        if ([string]::IsNullOrWhiteSpace($move.Original.NativeId)) {

            Write-Log (
                "Skipping '$($move.Original.FilePath)' because no NativeId exists."
            ) -Level Warning

            continue

        }

        if ($PSCmdlet.ShouldProcess(
            $move.Original.FilePath,
            "Update Rekordbox media path"
        )) {

            Update-RekordboxMediaPath `
                -Database $Database `
                -NativeId $move.Original.NativeId `
                -NewPath $move.NewFile.FilePath

            $updated++

        }

    }

    Write-Log "Updated $updated Rekordbox media path(s)." -Level Success

    return $updated

}