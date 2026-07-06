function Test-SeratoInstallation {

<#
.SYNOPSIS
Tests whether Serato DJ Pro is installed.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param()

    Find-InstalledApplication `
        -Provider "Serato" `
        -Executables @(
            "Serato DJ Pro.exe"
        ) `
        -InstallPaths @(
            "$env:ProgramFiles\Serato",
            "$env:ProgramFiles(x86)\Serato"
        )

}