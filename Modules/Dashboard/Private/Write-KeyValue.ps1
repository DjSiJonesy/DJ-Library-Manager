function Write-KeyValue {

<#
.SYNOPSIS
Displays a formatted key/value pair.

.DESCRIPTION
Writes a label and value with aligned formatting.

.EXAMPLE

Write-KeyValue "Media Items" 7837

Output:

Media Items.....................7837
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [string]
        $Label,

        [Parameter()]
        $Value,

        [int]
        $Width = 30

    )

    $text = $Label.PadRight($Width, '.')

    Write-Host ("{0} {1}" -f $text, $Value)

}