function Test-RekordboxInstallation {

<#
.SYNOPSIS
Tests whether rekordbox is installed.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param()

    Find-InstalledApplication `
        -Provider "rekordbox" `
        -Executables @(
            "rekordbox.exe",
            "rekordboxAgent.exe"
        ) `
        -InstallPaths @(
            "$env:ProgramFiles\rekordbox",
            "$env:ProgramFiles(x86)\rekordbox"
        )

}