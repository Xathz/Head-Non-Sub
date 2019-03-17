while ($true) {
    if (!(Get-WmiObject Win32_Process | Where-Object { ( $_.Name -eq "dotnet.exe" ) -and ( $_.CommandLine -like "*HeadNonSub.dll*" ) } | Select-Object ProcessID -ErrorAction SilentlyContinue)) {
        Start-Process -FilePath "dotnet.exe" -ArgumentList "exec","`".\HeadNonSub.dll`"" -NoNewWindow
    }

    Start-Sleep -Seconds 10
}
