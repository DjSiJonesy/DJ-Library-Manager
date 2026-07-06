function Test-VirtualDJInstallation {

<#
.SYNOPSIS
Tests whether VirtualDJ is installed.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param()

    Find-InstalledApplication `
        -Provider "VirtualDJ" `
        -Executables @(
            "virtualdj.exe"
        ) `
        -InstallPaths @(
            "$env:ProgramFiles\VirtualDJ",
            "$env:ProgramFiles(x86)\VirtualDJ"
        )

}