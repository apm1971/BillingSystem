# License Key Generator PowerShell Script
Write-Host "License Key Generator Utility" -ForegroundColor Green
Write-Host "============================" -ForegroundColor Green
Write-Host ""

# Get Hardware ID from user
$hardwareId = Read-Host "Enter Hardware ID"
$companyName = Read-Host "Enter Company Name"

# Generate a license key based on hardware ID and company name
$input = "$hardwareId|$companyName|$(Get-Date -Format 'yyyyMMdd')"
$sha256 = [System.Security.Cryptography.SHA256]::Create()
$hashBytes = $sha256.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($input))
$hash = [BitConverter]::ToString($hashBytes).Replace("-", "")

# Format the key in groups of 4 characters
$formattedKey = ""
for ($i = 0; $i -lt 20; $i += 4) {
    if ($formattedKey.Length -gt 0) { $formattedKey += "-" }
    $formattedKey += $hash.Substring($i, 4)
}

Write-Host ""
Write-Host "Generated License Key: $formattedKey" -ForegroundColor Yellow
Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown") 