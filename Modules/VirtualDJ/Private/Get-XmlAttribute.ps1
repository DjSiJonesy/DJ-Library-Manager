function Get-XmlAttribute {

<#
.SYNOPSIS
Safely retrieves an XML attribute value.

.DESCRIPTION
Returns the value of an XML attribute if it exists.
Returns $null if the node or attribute does not exist.

#>

    [CmdletBinding()]
    param(

        $Node,

        [Parameter(Mandatory)]
        [string]
        $Name

    )

    if ($null -eq $Node) {
        return $null
    }

    if ($Node -isnot [System.Xml.XmlElement]) {
        return $null
    }

    if (-not $Node.HasAttribute($Name)) {
        return $null
    }

    $value = $Node.GetAttribute($Name)

    if ([string]::IsNullOrWhiteSpace($value)) {
        return $null
    }

    return $value

}