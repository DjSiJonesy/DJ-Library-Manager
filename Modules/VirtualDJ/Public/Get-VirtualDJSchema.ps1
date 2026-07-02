function Get-VirtualDJSchema {

<#
.SYNOPSIS
Analyses the structure of a VirtualDJ database.

.DESCRIPTION
Scans every Song node and discovers available attributes,
child elements and attribute usage.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [pscustomobject]$Database

    )

    Write-Log "Analysing VirtualDJ database schema..."

    $songs = $Database.Xml.VirtualDJ_Database.Song

    $schema = @{}

    foreach ($song in $songs) {

        #
        # Song Attributes
        #

        foreach ($attribute in $song.Attributes) {

            $key = "Song.$($attribute.Name)"

            if (-not $schema.ContainsKey($key)) {

                $schema[$key] = [ordered]@{
                    Path        = $key
                    Count       = 0
                    Example     = $attribute.Value
                }

            }

            $schema[$key].Count++

        }

        #
        # Child Nodes
        #

        foreach ($child in $song.ChildNodes) {

            $nodeKey = "Song.$($child.Name)"

            if (-not $schema.ContainsKey($nodeKey)) {

                $schema[$nodeKey] = [ordered]@{
                    Path    = $nodeKey
                    Count   = 0
                    Example = ""
                }

            }

            $schema[$nodeKey].Count++

            foreach ($attribute in $child.Attributes) {

                $key = "Song.$($child.Name).$($attribute.Name)"

                if (-not $schema.ContainsKey($key)) {

                    $schema[$key] = [ordered]@{
                        Path        = $key
                        Count       = 0
                        Example     = $attribute.Value
                    }

                }

                $schema[$key].Count++

            }

        }

    }

    Write-Log "Schema analysis complete." -Level Success

    return $schema.Values |
        Sort-Object Path

}