function Test-EngineDJInstallation {

<#
.SYNOPSIS
Tests whether Engine DJ is installed.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param()

    Find-InstalledApplication `
        -Provider "EngineDJ" `
        -Executables @(
            "Engine DJ.exe"
        ) `
        -InstallPaths @(
            "$env:ProgramFiles\Engine DJ",
            "$env:ProgramFiles(x86)\Engine DJ"
        )

}