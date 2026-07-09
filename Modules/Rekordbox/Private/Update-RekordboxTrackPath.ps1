function Update-RekordboxTrackPath {

<#
.SYNOPSIS
Updates the file path for a single Rekordbox track.

.DESCRIPTION
Updates the FolderPath and FileNameL fields for a single
track identified by its NativeId.

.NOTES
DJ Library Manager
#>

    [CmdletBinding(SupportsShouldProcess)]
    param(

        [Parameter(Mandatory)]
        $Database,

        [Parameter(Mandatory)]
        [string]
        $NativeId,

        [Parameter(Mandatory)]
        [string]
        $NewPath

    )

    if ($null -eq $Database.Connection) {

        throw "The supplied object is not a valid Rekordbox database."

    }

    $folder = Split-Path $NewPath -Parent
    $filename = Split-Path $NewPath -Leaf

    #
    # Rekordbox stores forward slashes
    #

    $folder = $folder -replace '\\', '/'

    if ($PSCmdlet.ShouldProcess($NativeId, "Update Rekordbox track path")) {

        $command = $Database.Connection.CreateCommand()

        $command.CommandText = @'
UPDATE djmdContent
SET
    FolderPath = $FolderPath,
    FileNameL  = $FileName
WHERE
    ID = $ID;
'@

        $null = $command.Parameters.AddWithValue('$FolderPath', $folder)
        $null = $command.Parameters.AddWithValue('$FileName', $filename)
        $null = $command.Parameters.AddWithValue('$ID', $NativeId)

        $rows = $command.ExecuteNonQuery()

        if ($rows -ne 1) {

            throw "Expected to update one track, but updated $rows."

        }

    }

}