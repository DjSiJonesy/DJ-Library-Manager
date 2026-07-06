function Update-VirtualDJMediaPaths {

<#
.SYNOPSIS
Updates the path of a media item within a VirtualDJ database.

.DESCRIPTION
Searches the imported VirtualDJ database for a Song element
whose FilePath matches the supplied path.

If found, the FilePath attribute is updated in memory.

The database is NOT written to disk. Call
Save-VirtualDJDatabase to persist the changes.

.PARAMETER Database
A VirtualDJ database object returned by
Import-VirtualDJDatabase.

.PARAMETER OldPath
The existing media path.

.PARAMETER NewPath
The replacement media path.

.EXAMPLE
Update-VirtualDJMediaPaths `
    -Database $db `
    -OldPath "D:\Music\Old.mp3" `
    -NewPath "G:\DJ Library\Active\Old.mp3"

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        $Database,

        [Parameter(Mandatory)]
        [string]
        $OldPath,

        [Parameter(Mandatory)]
        [string]
        $NewPath

    )

    Write-Log "Updating VirtualDJ media path..." -Level Information

    if ($null -eq $Database.Xml) {

        throw "The supplied object is not a valid VirtualDJ database."

    }

    #
    # Locate the matching Song node
    #

    $song = $Database.Xml.SelectSingleNode(
        "//Song[@FilePath=`"$OldPath`"]"
    )

    if ($null -eq $song) {

        Write-Log "Media path not found: $OldPath" -Level Warning

        return [PSCustomObject]@{

            PSTypeName = 'DJLM.ProviderUpdateResult'

            Success = $false

            Updated = $false

            OldPath = $OldPath

            NewPath = $NewPath

            Message = "Media path not found."

        }

    }

    #
    # Update the FilePath attribute
    #

    $song.SetAttribute("FilePath", $NewPath)

    Write-Log "Media path updated." -Level Success

    return [PSCustomObject]@{

        PSTypeName = 'DJLM.ProviderUpdateResult'

        Success = $true

        Updated = $true

        OldPath = $OldPath

        NewPath = $NewPath

        Message = "Media path updated successfully."

    }

}