function Update-MediaPaths {

<#
.SYNOPSIS
Updates media paths using the appropriate provider.

.DESCRIPTION
Dispatches media path updates to the provider responsible
for the supplied database object.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        $Database,

        [Parameter(Mandatory)]
        [object[]]
        $MovedFiles

    )

    if ($null -eq $Database) {

        throw "A provider database must be supplied."

    }

    switch ($Database.PSTypeNames[0]) {

        'DJLM.VirtualDJDatabase' {

            return Update-VirtualDJMediaPaths `
                -Database $Database `
                -MovedFiles $MovedFiles

        }

        'DJLM.RekordboxDatabase' {

            return Update-RekordboxMediaPaths `
                -Database $Database `
                -MovedFiles $MovedFiles

        }

        default {

            throw (
                "Media path updates are not supported for database type '{0}'." -f
                $Database.PSTypeNames[0]
            )

        }

    }

}