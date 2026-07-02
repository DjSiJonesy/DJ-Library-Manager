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