function Show-Dashboard {

<#
.SYNOPSIS
Displays the DJ Library Manager dashboard.
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        $Statistics,

        [Parameter(Mandatory)]
        $Health,

        [Parameter(Mandatory)]
        $HealthScore,

        [Parameter(Mandatory)]
        $Analysis

    )

    Clear-Host

    #
    # Banner
    #

    Write-Banner

    #
    # Library Summary
    #

    Write-Section "Library Summary"

    $Summary = [PSCustomObject]@{

        'Media Items'      = $Statistics.MediaItems
        'Unique Artists'   = $Statistics.UniqueArtists
        'Unique Albums'    = $Statistics.UniqueAlbums
        'Unique Genres'    = $Statistics.UniqueGenres
        'Unique Drives'    = $Statistics.UniqueDrives
        'Files Scanned'    = $Analysis.TotalFiles

    }

    Write-Object -InputObject $Summary

    #
    # Library Health
    #

    Write-Section "Library Health"

    $HealthSummary = [PSCustomObject]@{

        'Missing Artist' = $Health.MissingArtist
        'Missing Title'  = $Health.MissingTitle
        'Missing Album'  = $Health.MissingAlbum
        'Missing Genre'  = $Health.MissingGenre
        'Missing BPM'    = $Health.MissingBPM
        'Missing Key'    = $Health.MissingKey
        'Missing Path'   = $Health.MissingPath

    }

    Write-Object -InputObject $HealthSummary

    Write-HealthScore `
        -Score $HealthScore.OverallScore

    #
    # Analysis Summary
    #

    Write-Section "Library Analysis"

    $AnalysisSummary = [PSCustomObject]@{

        'Duplicate Tracks' = $Analysis.DuplicateTracks.Count
        'Moved Files'      = $Analysis.MovedFiles.Count
        'Missing Files'    = $Analysis.MissingFiles.Count
        'Orphan Files'     = $Analysis.OrphanFiles.Count

    }

    Write-Object -InputObject $AnalysisSummary

    #
    # Recommendations
    #

    Write-Section "Recommendations"

    Write-Recommendations `
        -Recommendations $HealthScore.Recommendations

}