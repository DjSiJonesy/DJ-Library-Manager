function Get-ProviderConfiguration {

<#
.SYNOPSIS
Retrieves configuration for a DJ provider.

.DESCRIPTION
Returns the configuration section for the requested provider
from Settings.json.

This provides a single, provider-independent mechanism for
retrieving provider configuration.

.PARAMETER Provider
The name of the provider.

.EXAMPLE
$config = Get-ProviderConfiguration -Provider VirtualDJ

.EXAMPLE
$config = Get-ProviderConfiguration -Provider rekordbox

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]
        $Provider

    )

    $Configuration = Get-Configuration

    if (-not $Configuration.Providers) {

        throw "No Providers section exists in Settings.json."

    }

    $providerConfiguration =
        $Configuration.Providers.PSObject.Properties[$Provider]

    if ($null -eq $providerConfiguration) {

        throw "Provider '$Provider' is not configured."

    }

    return $providerConfiguration.Value

}