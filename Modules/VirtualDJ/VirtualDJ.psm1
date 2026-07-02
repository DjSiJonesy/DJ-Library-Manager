Set-StrictMode -Version Latest

$ModuleRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

foreach ($folder in 'Private','Public') {

    $path = Join-Path $ModuleRoot $folder

    if (Test-Path $path) {

        Get-ChildItem $path -Filter '*.ps1' |
            Sort-Object Name |
            ForEach-Object {

                . $_.FullName

            }

    }

}

Export-ModuleMember -Function (
    Get-ChildItem (
        Join-Path $ModuleRoot 'Public'
    ) -Filter '*.ps1' |
    Select-Object -ExpandProperty BaseName
)