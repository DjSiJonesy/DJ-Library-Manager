function Find-MissingFiles {

<#
.SYNOPSIS
Finds media items whose files no longer exist.

.DESCRIPTION
Checks each media item's FilePath and returns any items whose
underlying file cannot be found.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [object[]]
        $Media

    )

    $missing = New-Object System.Collections.Generic.List[object]

    foreach ($track in $Media) {

        if ([string]::IsNullOrWhiteSpace($track.FilePath)) {

            $missing.Add($track)
            continue

        }

        if (-not (Test-Path -LiteralPath $track.FilePath)) {

            $missing.Add($track)

        }

    }

    return $missing

}