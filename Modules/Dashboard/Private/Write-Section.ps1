function Write-Section {

<#
.SYNOPSIS
Displays a dashboard section heading.
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [string]
        $Title

    )

    Write-Host
    Write-Host $Title
    Write-Host ("─" * $Title.Length)

}