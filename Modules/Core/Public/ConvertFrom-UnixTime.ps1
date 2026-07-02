function ConvertFrom-UnixTime {

<#
.SYNOPSIS
Converts a Unix timestamp to a DateTime.

.DESCRIPTION
Converts a Unix timestamp (seconds since 1 January 1970 UTC)
to a local DateTime object.

Returns $null if the input is empty or invalid.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(
        $Timestamp
    )

    if ([string]::IsNullOrWhiteSpace($Timestamp)) {
        return $null
    }

    try {

        return [DateTimeOffset]::FromUnixTimeSeconds(
            [long]$Timestamp
        ).LocalDateTime

    }
    catch {

        return $null

    }

}