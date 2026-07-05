function Write-Recommendations {

<#
.SYNOPSIS
Displays library recommendations.
#>

    [CmdletBinding()]
    param(

        [string[]]
        $Recommendations

    )

    if (-not $Recommendations -or $Recommendations.Count -eq 0) {

        Write-Host "No recommendations. Your library is in excellent health."

        return

    }

    foreach ($Recommendation in $Recommendations) {

        Write-Host (" • {0}" -f $Recommendation)

    }

}