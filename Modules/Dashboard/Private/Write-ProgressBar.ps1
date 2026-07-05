function Write-ProgressBar {

<#
.SYNOPSIS
Displays a text-based progress bar.
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [ValidateRange(0,100)]
        [int]
        $Percent

    )

    $Length = 30

    $Filled = [math]::Round(($Percent / 100) * $Length)

    $Bar = ("█" * $Filled).PadRight($Length, "░")

    Write-Host ("{0} {1}%" -f $Bar, $Percent)

}