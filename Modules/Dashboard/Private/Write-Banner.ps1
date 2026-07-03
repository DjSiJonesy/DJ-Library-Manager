function Write-Banner {

    [CmdletBinding()]
    param()

    $line = ("═" * 62)

    Write-Host $line
    Write-Host
    Write-Host "               DJ Library Manager"
    Write-Host
    Write-Host "                  v0.3.0-alpha"
    Write-Host
    Write-Host "         Protect the music. Respect the DJ."
    Write-Host
    Write-Host $line
    Write-Host

}