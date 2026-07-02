function Write-Log {
<#
.SYNOPSIS
Writes a message to the DJLM log.

.DESCRIPTION
Outputs a timestamped message to both the console and the
current log file.

.EXAMPLE
Write-Log -Message "Loading configuration"

.EXAMPLE
Write-Log -Message "Database imported" -Level Success
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [string]$Message,

        [ValidateSet(
            "Information",
            "Success",
            "Warning",
            "Error",
            "Debug"
        )]
        [string]$Level = "Information"

    )

    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

    $entry = "[{0}] [{1}] {2}" -f `
        $timestamp, `
        $Level.ToUpper(), `
        $Message

    Write-Console `
        -Message $entry `
        -Level $Level

    Add-Content `
        -Path (Get-LogFile) `
        -Value $entry `
        -Encoding UTF8

}