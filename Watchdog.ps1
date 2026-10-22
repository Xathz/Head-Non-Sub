#Requires -Version 7.4

<#
.SYNOPSIS
Starts Head Non-Sub and restarts it when it stops, up to a limit. A long enough run resets the count.

.DESCRIPTION
Run from anywhere, it expects to sit next to HeadNonSub.dll.

Exit codes from the bot:
    0  Stopped normally, the watchdog exits
    1  Needs a restart, the watchdog restarts it until the limit is reached
    2  Needs fixing, the watchdog exits, the reason is in the bot's log
#>

[CmdletBinding()]
param(
    # How many times to restart the bot before giving up
    [ValidateRange(0, [int]::MaxValue)]
    [int] $MaxRestarts = 3,

    # Seconds to wait before each restart
    [ValidateRange(0, [int]::MaxValue)]
    [int] $RestartDelaySeconds = 15,

    # A run at least this long resets the restart count, 0 never resets it
    [ValidateRange(0, [double]::MaxValue)]
    [double] $HealthyRunMinutes = 60
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$appPath = Join-Path $PSScriptRoot 'HeadNonSub.dll'

# Same format as the bot's console log
function Write-WatchdogLog {
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', '', Justification = 'Colored console output is the point')]
    param(
        [Parameter(Mandatory)] [ValidateSet('Info', 'Warn', 'Error')] [string] $Level,
        [Parameter(Mandatory)] [string] $Message
    )

    $color = @{ Info = 'Gray'; Warn = 'Yellow'; Error = 'Red' }[$Level]
    $timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss.ffff'
    Write-Host "$timestamp [$($Level.ToUpper().PadLeft(5))] [Watchdog] $Message" -ForegroundColor $color
}

if (-not (Test-Path -LiteralPath $appPath)) {
    Write-WatchdogLog -Level Error -Message "Can not find '$appPath', put the watchdog next to HeadNonSub.dll"
    exit 1
}

if (-not (Get-Command -Name 'dotnet' -CommandType Application -ErrorAction SilentlyContinue)) {
    Write-WatchdogLog -Level Error -Message 'Can not find dotnet, install the .NET runtime'
    exit 1
}

# Do not start a second copy
$running = @(Get-Process -Name 'dotnet' -ErrorAction SilentlyContinue | Where-Object { $_.CommandLine -like '*HeadNonSub.dll*' })
if ($running.Count -gt 0) {
    Write-WatchdogLog -Level Error -Message "Head Non-Sub is already running (PID $($running.Id -join ', ')), stop it first"
    exit 1
}

$restarts = 0

while ($true) {
    Write-WatchdogLog -Level Info -Message 'Starting Head Non-Sub'

    # Runs in this console so the bot's console commands still work
    $started = Get-Date
    & dotnet exec $appPath
    $exitCode = $LASTEXITCODE
    $ranFor = (Get-Date) - $started

    switch ($exitCode) {
        0 {
            Write-WatchdogLog -Level Info -Message 'Head Non-Sub stopped normally'
            exit 0
        }
        2 {
            Write-WatchdogLog -Level Error -Message 'Head Non-Sub needs fixing, the reason is in its log above. Fix it then start the watchdog again'
            exit 2
        }
    }

    # Only crashes close together count towards the limit
    if ($restarts -gt 0 -and $HealthyRunMinutes -gt 0 -and $ranFor.TotalMinutes -ge $HealthyRunMinutes) {
        Write-WatchdogLog -Level Info -Message "Head Non-Sub ran for $([math]::Round($ranFor.TotalMinutes, 1)) minutes, resetting the restart count"
        $restarts = 0
    }

    if ($restarts -ge $MaxRestarts) {
        Write-WatchdogLog -Level Error -Message "Head Non-Sub stopped with exit code $exitCode, it was already restarted $MaxRestarts time(s), giving up"
        exit 1
    }

    $restarts++
    Write-WatchdogLog -Level Warn -Message "Head Non-Sub stopped with exit code $exitCode, restarting in $RestartDelaySeconds seconds ($restarts of $MaxRestarts)"
    Start-Sleep -Seconds $RestartDelaySeconds
}
