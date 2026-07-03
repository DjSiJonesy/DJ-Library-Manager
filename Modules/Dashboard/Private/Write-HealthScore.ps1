function Write-HealthScore {

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [int]
        $Score

    )

    Write-Section "Overall Library Health"

    Write-ProgressBar $Score

}