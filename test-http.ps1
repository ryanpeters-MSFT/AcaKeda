# Define the URL for the curl request.
$url = "https://aca-web-keda.blackforest-2c32ce68.eastus2.azurecontainerapps.io/longrunning"

# Loop to start 5 parallel jobs.
for ($i = 1; $i -le 5; $i++) {
    # Capture the current iteration number for clarity.
    $jobNumber = $i

    # Start a background job that will loop indefinitely.
    Start-Job -Name "CurlJob$jobNumber" -ScriptBlock {
        param($url, $jobNumber)
        while ($true) {
            try {
                Write-Host "Job $($jobNumber): Sending request to $url"
                # Execute the curl command. Note: On Windows, 'curl' may refer
                # to Invoke-WebRequest; if you require the external curl.exe,
                # specify its full path or adjust accordingly.
                curl $url
            }
            catch {
                Write-Error "Job $($jobNumber): Error sending request - $_"
            }
            # Optionally, add a brief delay to prevent overwhelming the target.
            Start-Sleep -Seconds 3
        }
    } -ArgumentList $url, $jobNumber
}

Write-Host "Started 5 parallel jobs. They will continue running in the background."
Write-Host "To view job status, use: Get-Job"
Write-Host "To stop all jobs, use: Get-Job | Stop-Job"