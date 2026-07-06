function Test-TraktorInstallation {

<#
.SYNOPSIS
Tests whether Traktor Pro is installed.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param()

    Find-InstalledApplication `
        -Provider "Traktor" `
        -Executables @(
            "Traktor.exe"
        ) `
        -InstallPaths @(
            "$env:ProgramFiles\Native Instruments\Traktor Pro 4",
            "$env:ProgramFiles\Native Instruments\Traktor Pro 3",
            "$env:ProgramFiles(x86)\Native Instruments\Traktor Pro 4",
            "$env:ProgramFiles(x86)\Native Instruments\Traktor Pro 3"
        )

}