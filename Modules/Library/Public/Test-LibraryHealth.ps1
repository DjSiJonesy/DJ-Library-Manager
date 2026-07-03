function Test-LibraryHealth {

<#
.SYNOPSIS
Analyses the health of a DJLM media library.

.DESCRIPTION
Checks for missing metadata and common library issues.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [PSCustomObject[]]
        $Media

    )

    Write-Log "Analysing library health..."

    $health = [PSCustomObject]@{

        MissingArtist = @(
            $Media | Where-Object {
                [string]::IsNullOrWhiteSpace($_.Artist)
            }
        ).Count

        MissingTitle = @(
            $Media | Where-Object {
                [string]::IsNullOrWhiteSpace($_.Title)
            }
        ).Count

        MissingAlbum = @(
            $Media | Where-Object {
                [string]::IsNullOrWhiteSpace($_.Album)
            }
        ).Count

        MissingGenre = @(
            $Media | Where-Object {
                [string]::IsNullOrWhiteSpace($_.Genre)
            }
        ).Count

        MissingBPM = @(
            $Media | Where-Object {
                [string]::IsNullOrWhiteSpace($_.BPM)
            }
        ).Count

        MissingKey = @(
            $Media | Where-Object {
                [string]::IsNullOrWhiteSpace($_.Key)
            }
        ).Count

        MissingPath = @(
            $Media | Where-Object {
                [string]::IsNullOrWhiteSpace($_.FilePath)
            }
        ).Count

    }

    Write-Log "Library health analysis complete." -Level Success

    return $health

}