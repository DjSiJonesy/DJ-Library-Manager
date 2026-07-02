function Write-Console {

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [string]$Message,

        [Parameter(Mandatory)]
        [ValidateSet(
            "Information",
            "Success",
            "Warning",
            "Error",
            "Debug"
        )]
        [string]$Level

    )

    $colour = switch ($Level) {

        "Information" { "White" }

        "Success" { "Green" }

        "Warning" { "Yellow" }

        "Error" { "Red" }

        "Debug" { "DarkGray" }

    }

    Write-Host $Message -ForegroundColor $colour

}