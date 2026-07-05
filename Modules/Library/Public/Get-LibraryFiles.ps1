function Get-LibraryFiles {

<#
.SYNOPSIS
Scans the file system for supported media files.

.DESCRIPTION
Returns a collection of library file objects from one or more
folders, or from all fixed/removable drives.

.NOTES
DJ Library Manager
#>

    [CmdletBinding(DefaultParameterSetName = 'Path')]
    param(

        [Parameter(
            Mandatory,
            ParameterSetName = 'Path'
        )]
        [string[]]
        $Path,

        [Parameter(
            Mandatory,
            ParameterSetName = 'AllDrives'
        )]
        [switch]
        $AllDrives,

        [switch]
        $Recurse

    )

    #
    # Supported media extensions
    #

    $SupportedExtensions = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::OrdinalIgnoreCase
    )

    @(
        '.mp3'
        '.wav'
        '.flac'
        '.m4a'
        '.aac'
        '.ogg'
        '.wma'
        '.aif'
        '.aiff'

        '.mp4'
        '.avi'
        '.mkv'
        '.mov'
        '.wmv'
        '.mpeg'
        '.mpg'

        '.cdg'
        '.zip'
    ) | ForEach-Object {

        $null = $SupportedExtensions.Add($_)

    }

    #
    # Resolve scan locations
    #

    $ScanPaths = @()

    if ($PSCmdlet.ParameterSetName -eq 'AllDrives') {

        $ScanPaths = Get-PSDrive -PSProvider FileSystem |
            Where-Object {

                $_.Root -and (Test-Path $_.Root)

            } |
            Select-Object -ExpandProperty Root

    }
    else {

        $ScanPaths = $Path

    }

    $Files = New-Object System.Collections.Generic.List[object]

    foreach ($Folder in $ScanPaths) {

        if (-not (Test-Path -LiteralPath $Folder)) {

            Write-Warning "Path not found: $Folder"
            continue

        }

        try {

            $ChildItems = Get-ChildItem `
                -LiteralPath $Folder `
                -File `
                -Force `
                -ErrorAction SilentlyContinue `
                -Recurse:$Recurse

            foreach ($File in $ChildItems) {

                if (-not $SupportedExtensions.Contains($File.Extension)) {

                    continue

                }

                $Files.Add(

                    [PSCustomObject]@{

                        FilePath = $File.FullName

                        FileName = $File.Name

                        Directory = $File.DirectoryName

                        Extension = $File.Extension

                        FileSize = $File.Length

                        LastModified = $File.LastWriteTime

                    }

                )

            }

        }
        catch {

            Write-Warning "Unable to scan '$Folder'."

        }

    }

    return $Files

}