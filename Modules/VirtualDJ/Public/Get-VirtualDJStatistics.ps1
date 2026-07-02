function Get-VirtualDJStatistics {

<#
.SYNOPSIS
Returns statistics for a VirtualDJ database.

.DESCRIPTION
Analyses an imported VirtualDJ database and returns summary
statistics.

#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [pscustomobject]$Database

    )

    $songs = $Database.Xml.VirtualDJ_Database.Song

    $drives = $songs |
        ForEach-Object {

            if ($_.FilePath.Length -gt 2) {

                $_.FilePath.Substring(0,2)

            }

        } |
        Sort-Object -Unique

$artists = foreach ($song in $songs) {

    if ($song.Tags -and $song.Tags.HasAttribute("Author")) {

        $song.Tags.Author

    }

}

$artists = $artists |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
    Sort-Object -Unique

    [PSCustomObject]@{

        DatabasePath = $Database.Path

        Imported = $Database.Loaded

        TotalItems = $songs.Count

        UniqueDrives = $drives.Count

        UniqueArtists = $artists.Count

    }

}