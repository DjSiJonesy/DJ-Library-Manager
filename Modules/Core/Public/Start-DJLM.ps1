function Start-DJLM {

<#
.SYNOPSIS
Runs the DJ Library Manager application.

.DESCRIPTION
Coordinates the complete DJLM workflow:

 - Load provider media
 - Scan library files
 - Analyse the library
 - Calculate library statistics
 - Assess library health
 - Display the dashboard

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param()

    Write-Log "Starting DJ Library Manager..." -Level Information

    #
    # Load configuration
    #

    $Configuration = Get-Configuration

    #
    # Import provider database
    #

    Write-Log "Loading VirtualDJ database..." -Level Information

    $Database = Import-VirtualDJDatabase `
    -Path $Configuration.Providers.VirtualDJ.DatabasePath

    #
    # Translate provider media
    #

    $Media = Get-VirtualDJMediaItems `
        -Database $Database

    #
    # Scan library
    #

    Write-Log "Scanning library files..." -Level Information

    $Files = Get-LibraryFiles `
    -Path $Configuration.Library.Paths `
    -Recurse:$Configuration.Library.Recurse

    #
    # Statistics
    #

    $Statistics = Get-LibraryStatistics `
        -Media $Media

    #
    # Health
    #

    $Health = Test-LibraryHealth `
        -Media $Media

    $HealthScore = Get-LibraryHealthScore `
        -Statistics $Statistics `
        -Health $Health

    #
    # Analysis
    #

    $Analysis = Get-LibraryAnalysis `
        -Media $Media `
        -Files $Files

    #
    # Dashboard
    #

    Show-Dashboard `
        -Statistics $Statistics `
        -Health $Health `
        -HealthScore $HealthScore `
        -Analysis $Analysis

    Write-Log "DJ Library Manager complete." -Level Success

}