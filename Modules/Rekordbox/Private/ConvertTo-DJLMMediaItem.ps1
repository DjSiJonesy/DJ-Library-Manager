function ConvertTo-DJLMMediaItem {

<#
.SYNOPSIS
Converts a Rekordbox database row into a DJLM media object.

.DESCRIPTION
Converts a single SQL query result row from the Rekordbox
database into a provider-independent DJ Library Manager
media object.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [System.Data.DataRow]
        $Row

    )

    [PSCustomObject]@{

        Provider = 'Rekordbox'

        MediaType = 'Unknown'

        FilePath = ($Row.FolderPath -replace '/', '\')

        FileSize = [long]$Row.FileSize

        #
        # These will be populated once the SQL joins are added.
        #

            Artist = $Row.Artist

            Title = $Row.Title

            Album = $Row.Album

            Genre = $Row.Genre

            Year = $Row.ReleaseYear

            BPM = if ($null -ne $Row.BPM) {
            [double]$Row.BPM / 100
        }
        else {
            $null
        }

            Key = $Row.MusicalKey

        Duration = [TimeSpan]::FromSeconds(
            [double]$Row.Length
        )

        DateFirstSeen = if ($Row.DateCreated) {
            [datetime]$Row.DateCreated
        }
        else {
            $null
        }

        DateLastModified = if ($Row.updated_at) {
            [datetime]$Row.updated_at
        }
        else {
            $null
        }

    }

}