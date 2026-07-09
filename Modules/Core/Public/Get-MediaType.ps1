function Get-MediaType {

<#
.SYNOPSIS
Determines the media type from a file path.

.DESCRIPTION
Returns a provider-independent media type based on the
file extension.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [string]
        $Path

    )

    $extension = [System.IO.Path]::GetExtension($Path).ToLowerInvariant()

    switch ($extension) {

        '.mp3'  { return 'Audio' }
        '.flac' { return 'Audio' }
        '.wav'  { return 'Audio' }
        '.aif'  { return 'Audio' }
        '.aiff' { return 'Audio' }
        '.m4a'  { return 'Audio' }
        '.aac'  { return 'Audio' }
        '.ogg'  { return 'Audio' }
        '.opus' { return 'Audio' }
        '.wma'  { return 'Audio' }

        '.mp4'  { return 'Video' }
        '.m4v'  { return 'Video' }
        '.mov'  { return 'Video' }
        '.avi'  { return 'Video' }
        '.mkv'  { return 'Video' }
        '.wmv'  { return 'Video' }
        '.mpg'  { return 'Video' }
        '.mpeg' { return 'Video' }
        '.webm' { return 'Video' }

        default { return 'Unknown' }

    }

}