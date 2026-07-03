function Get-MatchScore {

<#
.SYNOPSIS
Calculates a duplicate confidence score.
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        $Comparison

    )

    $score = 0

    if ($Comparison.ArtistMatch) {

        $score += 35

    }

    if ($Comparison.TitleMatch) {

        $score += 35

    }

    if ($Comparison.AlbumMatch) {

        $score += 5

    }

    if ($Comparison.BPMMatch) {

        $score += 10

    }

    if ($Comparison.KeyMatch) {

        $score += 10

    }

    if ($Comparison.DurationDifference -le 2) {

        $score += 5

    }

    return $score

}