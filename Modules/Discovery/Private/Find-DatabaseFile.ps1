function Find-DatabaseFile {
    <#
    .SYNOPSIS
        Searches one or more locations for a provider database file.

    .DESCRIPTION
        Performs a recursive search beneath each supplied root directory and
        returns the first matching database file found.

    .PARAMETER SearchPaths
        One or more root folders to search.

    .PARAMETER FileName
        The database filename to locate.

    .OUTPUTS
        System.String

        Returns the full path of the first matching file, or $null if no match
        is found.
    #>

    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string[]]$SearchPaths,

        [Parameter(Mandatory)]
        [string]$FileName
    )

    foreach ($path in $SearchPaths) {

        if (-not (Test-Path -LiteralPath $path)) {
            continue
        }

        try {
            $file = Get-ChildItem `
                -Path $path `
                -Filter $FileName `
                -File `
                -Recurse `
                -ErrorAction Stop |
                Select-Object -First 1

            if ($file) {
                return $file.FullName
            }
        }
        catch {
            Write-Verbose "Unable to search '$path' : $_"
        }
    }

    return $null
}