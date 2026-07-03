function Write-ProgressBar {

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [ValidateRange(0,100)]
        [int]
        $Percent

    )

    $length = 30

    $filled = [math]::Round(($Percent / 100) * $length)

    $bar = ("█" * $filled).PadRight($length, "░")

    Write-Host $bar
    Write-Host
    Write-Host ("{0}%" -f $Percent)

}