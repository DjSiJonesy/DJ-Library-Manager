function Import-Database {

    [CmdletBinding()]
    param (

        [Parameter(Mandatory)]
        [string]$Provider

    )

    switch ($Provider.ToLower()) {

        'virtualdj' {

            return Import-VirtualDJDatabase

        }

        'rekordbox' {

            return Import-RekordboxDatabase

        }

        default {

            throw "Unsupported provider '$Provider'."

        }

    }

}