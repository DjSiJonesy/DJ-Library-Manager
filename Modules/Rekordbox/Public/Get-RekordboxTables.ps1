function Get-RekordboxTables {

<#
.SYNOPSIS
Returns the tables contained in a Rekordbox database.
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        $Database

    )

    $table = [DJLM.SqlCipher.SqlCipherDatabase]::Query(
        $Database.Connection,
        @"
SELECT name
FROM sqlite_master
WHERE type='table'
ORDER BY name;
"@
    )

    return $table

}