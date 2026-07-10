function Start-LibraryAnalysis {

    [CmdletBinding()]
    param (

        [Parameter(Mandatory)]
        [string[]]$Providers,

        [Parameter(Mandatory)]
        [string[]]$MusicFolders

    )

    Write-Log "Starting library analysis..." Information

    $allMedia = @()

    foreach ($provider in $Providers) {

        Write-Log "Processing provider: $provider" Information

        $database = Import-Database -Provider $provider

        $media = Get-MediaItems `
            -Provider $provider `
            -Database $database

        $allMedia += $media

    }

    $files = Get-LibraryFiles `
    -Path $MusicFolders `
    -Recurse

    $statistics = Get-LibraryStatistics -Media $allMedia

    $health = Test-LibraryHealth -Media $allMedia

    $healthScore = Get-LibraryHealthScore `
    -Statistics $statistics `
    -Health $health

    $analysis = Get-LibraryAnalysis `
        -Media $allMedia `
        -Files $files

    return [PSCustomObject]@{

        AnalysisDate = Get-Date

        Providers = $Providers

        MusicFolders = $MusicFolders

        Media = $allMedia

        Files = $files

        Statistics = $statistics

        Health = $health

        HealthScore = $healthScore

        Analysis = $analysis

    }

}