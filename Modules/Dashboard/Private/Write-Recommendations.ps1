function Write-Recommendations {

    [CmdletBinding()]
    param(

        [string[]]
        $Recommendations

    )

    Write-Section "Recommendations"

    if (-not $Recommendations) {

        Write-Host "No recommendations."

        return

    }

    foreach ($item in $Recommendations) {

        Write-Host (" • {0}" -f $item)

    }

}