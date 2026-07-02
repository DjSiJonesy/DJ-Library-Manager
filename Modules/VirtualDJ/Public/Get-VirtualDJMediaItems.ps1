function Get-VirtualDJMediaItems {

<#
.SYNOPSIS
Converts a VirtualDJ database into DJ Library Manager media objects.

.DESCRIPTION
Reads every Song node from an imported VirtualDJ database and
converts it into a provider-independent DJLM media object.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [pscustomobject]
        $Database

    )

    Write-Log "Converting VirtualDJ media..."

    $songs = $Database.Xml.VirtualDJ_Database.Song

    $items = @()

    $successCount = 0
    $failureCount = 0

    foreach ($song in $songs) {

        try {

            $item = ConvertTo-DJLMMediaItem -Song $song

            if ($null -ne $item) {

                $items += $item
                $successCount++

            }
            else {

                Write-Log "Skipped null media item: $($song.FilePath)" -Level Warning
                $failureCount++

            }

        }
        catch {

            $failureCount++

            Write-Log "FAILED: $($song.FilePath)" -Level Warning
            Write-Log $_.Exception.Message -Level Warning

        }

    }

    Write-Log "Input records : $($songs.Count)"
    Write-Log "Successful    : $successCount"
    Write-Log "Failed        : $failureCount"
    Write-Log "Output records: $($items.Count)" -Level Success

    return $items

}