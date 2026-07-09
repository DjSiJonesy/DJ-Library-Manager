function Update-VirtualDJMediaPaths {

<#
.SYNOPSIS
Updates media file paths in a VirtualDJ database.

.DESCRIPTION
Updates the location of media files after they have been
moved on disk.

.PARAMETER Database
A VirtualDJ database object returned by
Import-VirtualDJDatabase.

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

    Write-Log "Updating VirtualDJ media paths..." -Level Information

    if ($null -eq $Database.Xml) {

        throw "The supplied object is not a valid VirtualDJ database."

    }

    $updated = 0

    foreach ($Move in $MovedFiles) {

        $OldPath = $Move.Original.FilePath
        $NewPath = $Move.NewFile.FilePath

        if (-not $PSCmdlet.ShouldProcess($OldPath, "Update VirtualDJ media path")) {

            continue

        }

        #
        # Locate the matching Song node
        #

        $Song = $Database.Xml.SelectSingleNode(
            "//Song[@FilePath=`"$OldPath`"]"
        )

        if ($null -eq $Song) {

            Write-Log "Media path not found: $OldPath" -Level Warning
            continue

        }

        #
        # Update FilePath
        #

        $Song.SetAttribute("FilePath", $NewPath)

        $updated++

    }

    Write-Log "Updated $updated VirtualDJ media path(s)." -Level Success

    return $updated

}