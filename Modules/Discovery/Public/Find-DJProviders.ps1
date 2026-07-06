function Find-DJProviders {

<#
.SYNOPSIS
Discovers installed DJ providers.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param()

    Write-Log "Discovering DJ providers..." -Level Information

    $providers = @(
        Test-VirtualDJInstallation
        Test-RekordboxInstallation
        Test-SeratoInstallation
        Test-EngineDJInstallation
        Test-TraktorInstallation
    )

    Write-Log "Provider discovery complete." -Level Success

    return $providers

}