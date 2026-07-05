function Write-Object {

<#
.SYNOPSIS
Displays all simple properties of an object.

.DESCRIPTION
Automatically formats an object's public properties using
Write-KeyValue.
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        $InputObject,

        [string[]]
        $Exclude = @()

    )

    foreach ($property in $InputObject.PSObject.Properties) {

        if ($Exclude -contains $property.Name) {
            continue
        }

        if ($property.Value -is [System.Collections.IEnumerable] -and
            $property.Value -isnot [string]) {
            continue
        }

        Write-KeyValue `
            -Label $property.Name `
            -Value $property.Value

    }

}