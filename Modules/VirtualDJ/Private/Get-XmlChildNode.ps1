function Get-XmlChildNode {

<#
.SYNOPSIS
Safely retrieves a child XML element.

.DESCRIPTION
Returns the specified child XML element if it exists.
Returns $null if the parent node or child node does not exist.

.PARAMETER Node
The parent XML node.

.PARAMETER Name
The name of the child element.

.EXAMPLE
$tags = Get-XmlChildNode -Node $song -Name "Tags"

.EXAMPLE
$scan = Get-XmlChildNode -Node $song -Name "Scan"

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        $Node,

        [Parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]
        $Name

    )

    if ($null -eq $Node) {
        return $null
    }

    if ($Node -isnot [System.Xml.XmlElement]) {
        return $null
    }

    foreach ($child in $Node.ChildNodes) {

        if ($child.NodeType -eq [System.Xml.XmlNodeType]::Element -and
            $child.Name -eq $Name) {

            return $child

        }

    }

    return $null

}