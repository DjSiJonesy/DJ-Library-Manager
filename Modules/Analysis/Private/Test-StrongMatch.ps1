function Test-StrongMatch {

<#
.SYNOPSIS
Determines whether two media items are likely duplicates.
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [int]
        $Score,

        [int]
        $Threshold = 85

    )

    return ($Score -ge $Threshold)

}