function Get-LibraryStatistics {

<#
.SYNOPSIS
Returns summary statistics for a DJLM media library.

.DESCRIPTION
Analyses a collection of DJLM media objects and returns
high-level statistics about the library.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [PSCustomObject[]]
        $Media

    )

    Write-Log "Calculating library statistics..."

    $artists = $Media |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_.Artist) } |
        Select-Object -ExpandProperty Artist -Unique

    $albums = $Media |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_.Album) } |
        Select-Object -ExpandProperty Album -Unique

    $genres = $Media |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_.Genre) } |
        Select-Object -ExpandProperty Genre -Unique

    $drives = $Media |
        Where-Object { $_.FilePath.Length -ge 2 } |
        ForEach-Object { $_.FilePath.Substring(0,2) } |
        Sort-Object -Unique

    $stats = [PSCustomObject]@{

        MediaItems = $Media.Count

        UniqueArtists = $artists.Count

        UniqueAlbums = $albums.Count

        UniqueGenres = $genres.Count

        UniqueDrives = $drives.Count

        Drives = $drives

    }

    Write-Log "Library statistics complete." -Level Success

    return $stats

}