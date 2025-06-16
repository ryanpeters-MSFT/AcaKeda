$Url = "https://aca-web-keda.blackforest-2c32ce68.eastus2.azurecontainerapps.io/queuemessage"

while ($true) {
    try {
        Invoke-WebRequest -Uri $Url > $null
        Write-Host "Invoked $Url at $(Get-Date)"
    } catch {
        Write-Host "Failed to invoke $Url at $(Get-Date): $_"
    }
    Start-Sleep -Seconds 1
}
