function Get-RekordboxStatistics {

<#
.SYNOPSIS
Returns statistics for a Rekordbox database.

.DESCRIPTION
Analyses an imported Rekordbox database and returns summary
statistics.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [pscustomobject]
        $Database

    )

    Write-Log "Calculating Rekordbox statistics..." -Level Information

    $table = [DJLM.SqlCipher.SqlCipherDatabase]::Query(
        $Database.Connection,
@"
SELECT
    c.FolderPath,
    ar.Name AS Artist
FROM djmdContent c
LEFT JOIN djmdArtist ar
    ON c.ArtistID = ar.ID;
"@
    )

    $drives = $table |
        ForEach-Object {

            if ($_.FolderPath.Length -ge 2) {

                $_.FolderPath.Substring(0,2)

            }

        } |
        Sort-Object -Unique

    $artists = $table |
        Select-Object -ExpandProperty Artist |
        Where-Object {
            -not [string]::IsNullOrWhiteSpace($_)
        } |
        Sort-Object -Unique

    [PSCustomObject]@{

        DatabasePath  = $Database.Path

        Imported      = $Database.Loaded

        TotalItems    = $table.Rows.Count

        UniqueDrives  = $drives.Count

        UniqueArtists = $artists.Count

    }

}