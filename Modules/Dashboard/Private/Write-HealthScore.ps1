function Write-HealthScore {

<#
.SYNOPSIS
Displays the overall library health score.
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [ValidateRange(0,100)]
        [int]
        $Score

    )

    Write-Section "Overall Library Health"

    Write-ProgressBar -Percent $Score

    $Status = switch ($Score) {

        { $_ -ge 95 } { "Excellent"; break }

        { $_ -ge 85 } { "Good"; break }

        { $_ -ge 70 } { "Fair"; break }

        { $_ -ge 50 } { "Poor"; break }

        default { "Critical" }

    }

    Write-Host ("Status: {0}" -f $Status)

}