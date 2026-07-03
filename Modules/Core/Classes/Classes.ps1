class DJLMMediaItem {

    # Source Information
    [string]$Provider
    [string]$MediaType

    # File Information
    [string]$FilePath
    [Int64]$FileSize
    [bool]$Exists

    # Music Metadata
    [string]$Artist
    [string]$Title
    [string]$Remix
    [string]$Album
    [string]$Genre
    [int]$Year

    # Audio Information
    [double]$BPM
    [string]$Key
    [TimeSpan]$Duration

    # Library Information
    [datetime]$DateAdded
    [datetime]$LastModified

    # DJLM
    [hashtable]$Properties

    DJLMMediaItem() {

        $this.Provider   = ""
        $this.MediaType  = "Unknown"
        $this.Exists     = $false
        $this.Properties = @{}

    }

    [string] ToString() {

        if ([string]::IsNullOrWhiteSpace($this.Artist)) {
            return $this.FilePath
        }

        return "$($this.Artist) - $($this.Title)"

    }

}
class DJLMStatistics {

    [int]$MediaItems

    [int]$UniqueArtists

    [int]$UniqueAlbums

    [int]$UniqueGenres

    [int]$UniqueDrives

    [string[]]$Drives

}

class DJLMHealth {

    [int]$MissingArtist

    [int]$MissingTitle

    [int]$MissingAlbum

    [int]$MissingGenre

    [int]$MissingBPM

    [int]$MissingKey

    [int]$MissingPath

}

class DJLMAssessment {

    [int]$OverallScore

    [int]$MetadataScore

    [int]$FileScore

    [int]$OrganisationScore

    [string[]]$Recommendations

}