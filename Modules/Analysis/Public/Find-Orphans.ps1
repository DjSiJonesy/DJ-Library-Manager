function Find-Orphans {

<#
.SYNOPSIS
Finds media files that exist on disk but are not referenced
by the media library.

.DESCRIPTION
Compares the physical library files against the imported
media collection and returns any files that are not present
within the library database.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [object[]]
        $Media,

        [Parameter(Mandatory)]
        [object[]]
        $Files

    )

    #
    # Build a fast lookup of all library file paths
    #

    $LibraryPaths = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::OrdinalIgnoreCase
    )

    foreach ($Track in $Media) {

        if ([string]::IsNullOrWhiteSpace($Track.FilePath)) {
            continue
        }

        $null = $LibraryPaths.Add(

            [IO.Path]::GetFullPath($Track.FilePath).Trim()

        )

    }

    #
    # Find orphaned files
    #

    $Orphans = New-Object System.Collections.Generic.List[object]

    foreach ($File in $Files) {

        $Path = [IO.Path]::GetFullPath($File.FilePath).Trim()

        if (-not $LibraryPaths.Contains($Path)) {

            $Orphans.Add($File)

        }

    }

    return $Orphans

}