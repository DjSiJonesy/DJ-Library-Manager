function Get-Configuration {
<#
.SYNOPSIS
Retrieves the DJ Library Manager configuration.

.DESCRIPTION
Loads the application configuration from Config\Settings.json.

The configuration is cached after the first successful load.
Use -Refresh to force the configuration to be reloaded.

.PARAMETER Refresh
Reload the configuration from disk.

.EXAMPLE
$config = Get-Configuration

.EXAMPLE
$config = Get-Configuration -Refresh

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(
        [switch]$Refresh
    )

    # Return cached configuration if available
    if (-not $Refresh) {

        $cached = Get-Variable `
            -Name Configuration `
            -Scope Script `
            -ErrorAction SilentlyContinue

        if ($null -ne $cached) {
            return $cached.Value
        }

    }

    # Determine project root
    $projectRoot = Get-ProjectRoot

    # Configuration file
    $configurationFile = Join-Path `
        $projectRoot `
        "Config\Settings.json"

    if (-not (Test-Path $configurationFile)) {

        throw @"
Configuration file not found.

Expected:

$configurationFile
"@

    }

    try {

        $configuration =
            Get-Content `
                -Path $configurationFile `
                -Raw `
                -Encoding UTF8 |
            ConvertFrom-Json

    }
    catch {

        throw @"
Unable to read Settings.json.

$($_.Exception.Message)
"@

    }

    # Cache configuration
    $script:Configuration = $configuration

    return $configuration

}