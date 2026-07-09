function Update-RekordboxMediaPath {

<#
.SYNOPSIS
Updates the path of a single Rekordbox media item.

.DESCRIPTION
Updates the location of a single media item in the
Rekordbox database.

.PARAMETER Database
A Rekordbox database object returned by
Import-RekordboxDatabase.

.PARAMETER MediaItem
The DJLM media item to update.

.PARAMETER NewPath
The new full path to the media file.

.EXAMPLE
$item = $media | Select-Object -First 1

Update-RekordboxMediaPath `
    -Database $db `
    -MediaItem $item `
    -NewPath 'D:\Music\Test.mp3'

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

    Write-Log "Updating Rekordbox media path..." -Level Information

        if ([string]::IsNullOrWhiteSpace($NativeId)) {

        throw "A valid NativeId must be supplied."

    }

   if ($PSCmdlet.ShouldProcess($NativeId, "Update Rekordbox media path")) {

        Update-RekordboxTrackPath `
        -Database $Database `
        -NativeId $NativeId `
        -NewPath $NewPath

    }

    Write-Log "Rekordbox media path updated." -Level Success

}