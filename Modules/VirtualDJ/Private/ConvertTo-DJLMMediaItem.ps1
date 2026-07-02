function ConvertTo-DJLMMediaItem {
<#
.SYNOPSIS
Converts a VirtualDJ Song node into a DJLM media object.
#>

    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [System.Xml.XmlElement]
        $Song
    )

        $tags  = Get-XmlChildNode -Node $Song -Name "Tags"
        $infos = Get-XmlChildNode -Node $Song -Name "Infos"
        $scan  = Get-XmlChildNode -Node $Song -Name "Scan"

    [PSCustomObject]@{

        Provider = "VirtualDJ"

        MediaType = "Unknown"

        FilePath = Get-XmlAttribute -Node $Song -Name "FilePath"

        FileSize = [long](Get-XmlAttribute -Node $Song -Name "FileSize")

        Artist = Get-XmlAttribute -Node $tags -Name "Author"

        Title = Get-XmlAttribute -Node $tags -Name "Title"

        Album = Get-XmlAttribute -Node $tags -Name "Album"

        Genre = Get-XmlAttribute -Node $tags -Name "Genre"

        Year = Get-XmlAttribute -Node $tags -Name "Year"

        BPM = Get-XmlAttribute -Node $scan -Name "Bpm"

        Key = Get-XmlAttribute -Node $scan -Name "Key"

        DateFirstSeen = ConvertFrom-UnixTime (
            Get-XmlAttribute -Node $infos -Name "FirstSeen"
        )

        DateLastModified = ConvertFrom-UnixTime (
            Get-XmlAttribute -Node $infos -Name "LastModified"
        )

    }

}