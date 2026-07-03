function Get-LibraryHealthScore {

<#
.SYNOPSIS
Calculates the overall health score for a DJ library.

.DESCRIPTION
Uses library statistics and health analysis to produce an
overall health score together with category scores and
recommendations.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        $Statistics,

        [Parameter(Mandatory)]
        $Health

    )

    Write-Log "Calculating library health score..."

    $total = [double]$Statistics.MediaItems

    if ($total -eq 0) {

        throw "Cannot calculate health score for an empty library."

    }

    #
    # Metadata Score
    #

    $metadataPenalty = (

        ($Health.MissingArtist * 25) +
        ($Health.MissingTitle  * 30) +
        ($Health.MissingAlbum  * 10) +
        ($Health.MissingGenre  * 10) +
        ($Health.MissingBPM    * 15) +
        ($Health.MissingKey    * 10)

    ) / ($total * 100)

    $metadataScore = [math]::Round(
        [math]::Max(0, (1 - $metadataPenalty) * 100)
    )

    #
    # File Score
    #

    $filePenalty = (
        $Health.MissingPath
    ) / $total

    $fileScore = [math]::Round(
        [math]::Max(0, (1 - $filePenalty) * 100)
    )

    #
    # Organisation Score
    #
    # Version 1 uses metadata as a proxy.
    #

    $organisationScore = [math]::Round(
        ($metadataScore + $fileScore) / 2
    )

    #
    # Overall
    #

    $overallScore = [math]::Round(

        ($metadataScore * 0.60) +
        ($fileScore * 0.25) +
        ($organisationScore * 0.15)

    )

    #
    # Recommendations
    #

    $recommendations = @()

    if ($Health.MissingGenre -gt 0) {

        $recommendations +=
            "Add Genre tags to $($Health.MissingGenre) tracks."

    }

    if ($Health.MissingBPM -gt 0) {

        $recommendations +=
            "Analyse BPM for $($Health.MissingBPM) tracks."

    }

    if ($Health.MissingKey -gt 0) {

        $recommendations +=
            "Analyse musical key for $($Health.MissingKey) tracks."

    }

    if ($Health.MissingAlbum -gt 0) {

        $recommendations +=
            "Complete album information for $($Health.MissingAlbum) tracks."

    }

    Write-Log "Library health score calculated." -Level Success

    return [PSCustomObject]@{

        OverallScore = $overallScore

        MetadataScore = $metadataScore

        FileScore = $fileScore

        OrganisationScore = $organisationScore

        Recommendations = $recommendations

    }

}