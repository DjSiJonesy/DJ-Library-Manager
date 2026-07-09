function Get-LibraryFiles {

<#
.SYNOPSIS
Scans the file system for supported media files.

.DESCRIPTION
Returns a collection of library file objects from one or more
folders, or from all fixed/removable drives.

If no parameters are supplied, the configured Library.Path
from Settings.json is scanned.

.NOTES
DJ Library Manager
#>

    [CmdletBinding(DefaultParameterSetName = 'Configured')]
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

    switch ($PSCmdlet.ParameterSetName) {

        'Configured' {

            $Configuration = Get-Configuration

            if ([string]::IsNullOrWhiteSpace($Configuration.Library.Path)) {

                throw "Library.Path is not configured in Settings.json."

            }

            $ScanPaths = @($Configuration.Library.Path)

            #
            # Use configured recursion unless explicitly specified
            #

            if (-not $PSBoundParameters.ContainsKey('Recurse')) {

                $Recurse = [bool]$Configuration.Library.Recurse

            }

        }

        'Path' {

            $ScanPaths = $Path

        }

        'AllDrives' {

            $ScanPaths = Get-PSDrive -PSProvider FileSystem |
                Where-Object {
                    $_.Root -and (Test-Path $_.Root)
                } |
                Select-Object -ExpandProperty Root

        }

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

                        FilePath      = $File.FullName
                        FileName      = $File.Name
                        Directory     = $File.DirectoryName
                        Extension     = $File.Extension
                        FileSize      = $File.Length
                        LastModified  = $File.LastWriteTime

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