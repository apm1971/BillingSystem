# Hardware ID Generator PowerShell Script
Write-Host "Hardware ID Utility" -ForegroundColor Green
Write-Host "==================" -ForegroundColor Green
Write-Host ""

# Get CPU ID
$cpuId = ""
try {
    $processor = Get-WmiObject -Class Win32_Processor | Select-Object -First 1
    $cpuId = $processor.ProcessorId
}
catch {
    Write-Host "Could not retrieve CPU ID" -ForegroundColor Red
}

# Get HDD Serial
$hddSerial = ""
try {
    $disk = Get-WmiObject -Class Win32_LogicalDisk -Filter "DeviceID='C:'" | Select-Object -First 1
    $hddSerial = $disk.VolumeSerialNumber
}
catch {
    Write-Host "Could not retrieve HDD Serial" -ForegroundColor Red
}

# Combine and hash hardware info
$combined = "$cpuId|$hddSerial"
$sha256 = [System.Security.Cryptography.SHA256]::Create()
$hashBytes = $sha256.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($combined))
$hardwareId = [BitConverter]::ToString($hashBytes).Replace("-", "").Substring(0, 16)

Write-Host "Your Hardware ID: $hardwareId" -ForegroundColor Yellow
Write-Host ""
Write-Host "Use this Hardware ID when generating a license key."
Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown") 