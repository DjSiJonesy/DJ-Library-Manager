function Get-MediaItems {

    [CmdletBinding()]
    param (

        [Parameter(Mandatory)]
        [string]$Provider,

        [Parameter(Mandatory)]
        $Database

    )

    switch ($Provider.ToLower()) {

        'virtualdj' {

            return Get-VirtualDJMediaItems -Database $Database

        }

        'rekordbox' {

            return Get-RekordboxMediaItems -Database $Database

        }

        default {

            throw "Unsupported provider '$Provider'."

        }

    }

}