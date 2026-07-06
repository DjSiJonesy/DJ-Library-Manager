function Find-InstalledApplication {

<#
.SYNOPSIS
Searches for an installed application.

.DESCRIPTION
Searches one or more installation folders for one or more
possible executable names.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [string]
        $Provider,

        [Parameter(Mandatory)]
        [string[]]
        $Executables,

        [Parameter(Mandatory)]
        [string[]]
        $InstallPaths

    )

    foreach ($Path in $InstallPaths) {

        if (-not (Test-Path $Path)) {
            continue
        }

        foreach ($Executable in $Executables) {

    #
    # Look for the executable anywhere beneath the install path.
    #

    $ExecutablePath = Get-ChildItem `
        -Path $Path `
        -Filter $Executable `
        -File `
        -Recurse `
        -ErrorAction SilentlyContinue |
        Select-Object -First 1

    if ($null -eq $ExecutablePath) {
        continue
    }

    $ExecutablePath = $ExecutablePath.FullName

            try {

                $Version = (Get-Item $ExecutablePath).VersionInfo.ProductVersion

            }
            catch {

                $Version = $null

            }

            return [PSCustomObject]@{

                PSTypeName = 'DJLM.ProviderInstallation'

                Provider = $Provider

                Installed = $true

                InstallPath = Split-Path $ExecutablePath -Parent

                Executable = $ExecutablePath

                Version = $Version

            }

        }

    }

    return [PSCustomObject]@{

        PSTypeName = 'DJLM.ProviderInstallation'

        Provider = $Provider

        Installed = $false

        InstallPath = $null

        Executable = $null

        Version = $null

    }

}