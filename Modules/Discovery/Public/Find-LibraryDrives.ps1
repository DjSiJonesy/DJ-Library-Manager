function Find-LibraryDrives {

<#
.SYNOPSIS
Discovers available library drives.

.DESCRIPTION
Returns all ready fixed and removable drives that are
available for DJ Library Manager.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param()

    Write-Log "Discovering library drives..." -Level Information

    $drives = Get-Volume |
        Where-Object {

            $_.DriveLetter -and
            $_.DriveType -in @('Fixed', 'Removable')

        } |
        Sort-Object DriveLetter |
        ForEach-Object {

            [PSCustomObject]@{

                PSTypeName = 'DJLM.LibraryDrive'

                DriveLetter = "$($_.DriveLetter):"

                Label = $_.FileSystemLabel

                DriveType = $_.DriveType

                FileSystem = $_.FileSystem

                SizeGB = [math]::Round($_.Size / 1GB, 2)

                FreeSpaceGB = [math]::Round($_.SizeRemaining / 1GB, 2)

                Ready = $true

                Role = 'Unknown'

            }

        }

    Write-Log "Library drive discovery complete." -Level Success

    return $drives

}