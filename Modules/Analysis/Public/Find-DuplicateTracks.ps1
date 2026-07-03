function Find-DuplicateTracks {

<#
.SYNOPSIS
Finds likely duplicate tracks within a media collection.

.DESCRIPTION
Groups tracks by normalised Artist + Title before performing
duplicate comparisons. This dramatically reduces the number
of comparisons required whilst still using the Analysis
Engine matching functions.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [object[]]
        $Media

    )

    $duplicates = New-Object System.Collections.Generic.List[object]

    #
    # Build candidate groups
    #

    $groups = @{}

    foreach ($track in $Media) {

        $artist = if ([string]::IsNullOrWhiteSpace($track.Artist)) {
            ''
        }
        else {
            $track.Artist.Trim().ToUpperInvariant()
        }

        $title = if ([string]::IsNullOrWhiteSpace($track.Title)) {
            ''
        }
        else {
            $track.Title.Trim().ToUpperInvariant()
        }

        $key = "$artist|$title"

        if (-not $groups.ContainsKey($key)) {

            $groups[$key] = New-Object System.Collections.Generic.List[object]

        }

        $groups[$key].Add($track)

    }

    #
    # Compare only within each candidate group
    #

    foreach ($group in $groups.Values) {

        if ($group.Count -lt 2) {
            continue
        }

        for ($i = 0; $i -lt $group.Count - 1; $i++) {

            for ($j = $i + 1; $j -lt $group.Count; $j++) {

                $comparison = Compare-MediaItem `
                    -Reference $group[$i] `
                    -Candidate $group[$j]

                $score = Get-MatchScore -Comparison $comparison

                if (Test-StrongMatch -Score $score) {

                    $duplicates.Add(

                        [PSCustomObject]@{

                            Reference = $group[$i]

                            Candidate = $group[$j]

                            Score = $score

                            Comparison = $comparison

                        }

                    )

                }

            }

        }

    }

    return $duplicates

}