function Get-RekordboxMediaItems {

<#
.SYNOPSIS
Returns Rekordbox media items.

.DESCRIPTION
Reads media records from the Rekordbox database and converts
them into provider-independent DJLM media objects.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        $Database

    )

    Write-Log "Reading Rekordbox media..." -Level Information

    $table = [DJLM.SqlCipher.SqlCipherDatabase]::Query(
        $Database.Connection,
@"
SELECT
    c.*,

    ar.Name       AS Artist,

    al.Name       AS Album,

    g.Name        AS Genre,

    k.ScaleName   AS MusicalKey

FROM djmdContent c

LEFT JOIN djmdArtist ar
    ON c.ArtistID = ar.ID

LEFT JOIN djmdAlbum al
    ON c.AlbumID = al.ID

LEFT JOIN djmdGenre g
    ON c.GenreID = g.ID

LEFT JOIN djmdKey k
    ON c.KeyID = k.ID

ORDER BY c.Title;
"@
    )

    $items = foreach ($row in $table.Rows) {

        ConvertTo-DJLMMediaItem -Row $row

    }

    Write-Log "Retrieved $($items.Count) media items." -Level Success

    return $items

}