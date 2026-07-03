function Compare-MediaItem {

<#
.SYNOPSIS
Compares two DJLM media items.

.DESCRIPTION
Returns a comparison object containing the fields required
by the Analysis Engine.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        $Reference,

        [Parameter(Mandatory)]
        $Candidate

    )

    return [PSCustomObject]@{

        ArtistMatch =
            ($Reference.Artist -eq $Candidate.Artist)

        TitleMatch =
            ($Reference.Title -eq $Candidate.Title)

        AlbumMatch =
            ($Reference.Album -eq $Candidate.Album)

        BPMMatch =
            ($Reference.BPM -eq $Candidate.BPM)

        KeyMatch =
            ($Reference.Key -eq $Candidate.Key)

        DurationDifference =

            [math]::Abs(

                (
                    $Reference.Duration.TotalSeconds -
                    $Candidate.Duration.TotalSeconds

                )

            )

    }

}