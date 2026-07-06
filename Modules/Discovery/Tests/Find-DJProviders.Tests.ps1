function Find-DJProviders {

<#
.SYNOPSIS
Discovers supported DJ software providers.

.DESCRIPTION
Calls each provider installation detection function and
returns a collection describing the providers found on the
current computer.

.EXAMPLE
Find-DJProviders

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param()

    Write-Log "Discovering DJ providers..." -Level Information

    $Providers = @()

    #
    # VirtualDJ
    #

    $Providers += Test-VirtualDJInstallation

    #
    # rekordbox
    #

    $Providers += Test-RekordboxInstallation

    #
    # Serato
    #

    $Providers += Test-SeratoInstallation

    #
    # Engine DJ
    #

    $Providers += Test-EngineDJInstallation

    Write-Log "Provider discovery complete." -Level Success

    return $Providers

}