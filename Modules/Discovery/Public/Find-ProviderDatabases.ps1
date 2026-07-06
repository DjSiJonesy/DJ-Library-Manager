function Find-ProviderDatabases {

<#
.SYNOPSIS
Discovers configured provider databases.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param()

    Write-Log "Discovering provider databases..." -Level Information

    $databases = @(
        Find-VirtualDJDatabase
        Find-RekordboxDatabase
        Find-SeratoDatabase
        Find-EngineDJDatabase
        Find-TraktorDatabase
    )

    Write-Log "Provider database discovery complete." -Level Success

    return $databases

}