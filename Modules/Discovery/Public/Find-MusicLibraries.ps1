function Find-MusicLibraries {

<#
.SYNOPSIS
Discovers music library folders.

.DESCRIPTION
Searches the discovered drives for common music library
folders.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

    [Parameter(Mandatory)]
    $Drives

)

    Write-Log "Discovering music libraries..." -Level Information

    $libraries = @()

    $commonFolders = @(
        'DJ_Library',
        'Music',
        'Music Library',
        'Audio',
        'Songs'
    )

    foreach ($drive in $Drives) {

        foreach ($folder in $commonFolders) {

            $path = Join-Path $drive.DriveLetter $folder

            if (Test-Path $path) {

                $libraries += [PSCustomObject]@{

                    PSTypeName = 'DJLM.MusicLibrary'

                    Name = Split-Path $path -Leaf

                    Path = $path

                    Drive = $drive.DriveLetter

                    Source = 'Discovery'

                }

            }

        }

    }

    Write-Log "Music library discovery complete." -Level Success

    return $libraries

}