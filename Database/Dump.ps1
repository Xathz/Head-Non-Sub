$mariaDBDatabase = "headnonsub"
$mariaDBUser = ""
$mariaDBPass = ""

$dateTime = Get-Date -UFormat "+%Y%m%dT%H%M%S"
$zipFile = ".\HeadNonSub_$($dateTime).zip"

$mysqlDump = ".\mysqldump.exe"
$sevenZip = ".\7z.exe"

$logFile = ".\Backup.log"
$tempDump = ".\HeadNonSub.sql"
$tempErrorLog = ".\ErrorOut.log"

# ========================================

# Dump the database to an sql file
Start-Process -FilePath $mysqlDump -ArgumentList "--databases `"$($mariaDBDatabase)`"","--user=$($mariaDBUser)","--password=$($mariaDBPass)","--result-file=`"$($tempDump)`"" -NoNewWindow -Wait -RedirectStandardError $tempErrorLog

# Compress the sql file
Start-Process -FilePath $sevenZip -ArgumentList "a","-mx=7","`"$($zipFile)`"","$($tempDump)" -NoNewWindow -Wait -RedirectStandardError $tempErrorLog

if (Get-Content $tempErrorLog) {
    "==== $(Get-Date -Format "o")`n$(Get-Content $tempErrorLog -Raw)`n========`n" | Out-File -Append $logFile
}

Remove-Item -Path $tempDump
Remove-Item -Path $tempErrorLog
