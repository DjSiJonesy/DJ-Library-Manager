function Show-Dashboard {

<#
.SYNOPSIS
Displays the DJ Library Manager Home Screen.
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        $Statistics,

        [Parameter(Mandatory)]
        $Health,

        [Parameter(Mandatory)]
        $Assessment

    )

    Clear-Host

    Write-Banner

    Write-Section "Library Summary"

    Write-Object `
        -InputObject $Statistics `
        -Exclude Drives

    Write-Section "Library Health"

    Write-Object `
        -InputObject $Health

    Write-HealthScore `
        -Score $Assessment.OverallScore

    Write-Recommendations `
        -Recommendations $Assessment.Recommendations

}