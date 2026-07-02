function Get-LogFile {
<#
.SYNOPSIS
Returns the current DJLM log file path.

.DESCRIPTION
Creates the Logs folder if it does not exist and returns the
path to today's log file.

.NOTES
Private helper function.
#>

    [CmdletBinding()]
    param()

    $config = Get-Configuration

    $projectRoot = Get-ProjectRoot

    $logFolder = Join-Path `
        $projectRoot `
        $config.Logging.LogFolder

    if (-not (Test-Path $logFolder)) {

        New-Item `
            -Path $logFolder `
            -ItemType Directory `
            -Force | Out-Null

    }

    Join-Path `
        $logFolder `
        ("DJLM_{0:yyyy-MM-dd}.log" -f (Get-Date))

}