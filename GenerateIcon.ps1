# PowerShell script to create a simple application icon
# This creates a basic PNG icon that can be used as the application icon

Add-Type -AssemblyName System.Drawing

try {
    # Create a 256x256 bitmap
    $bitmap = New-Object System.Drawing.Bitmap(256, 256)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    
    # Set high quality rendering
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.Clear([System.Drawing.Color]::Transparent)
    
    # Create background gradient
    $backgroundBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        [System.Drawing.Point]::new(0, 0),
        [System.Drawing.Point]::new(256, 256),
        [System.Drawing.Color]::FromArgb(45, 45, 48),
        [System.Drawing.Color]::FromArgb(70, 70, 73)
    )
    
    # Draw background circle
    $graphics.FillEllipse($backgroundBrush, 8, 8, 240, 240)
    
    # Create inner circle gradient
    $innerBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        [System.Drawing.Point]::new(64, 64),
        [System.Drawing.Point]::new(192, 192),
        [System.Drawing.Color]::FromArgb(0, 120, 215),
        [System.Drawing.Color]::FromArgb(0, 100, 180)
    )
    
    # Draw inner circle
    $graphics.FillEllipse($innerBrush, 48, 48, 160, 160)
    
    # Create font for dollar sign
    $font = New-Object System.Drawing.Font("Arial", 100, [System.Drawing.FontStyle]::Bold)
    $textBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
    $text = "$"
    
    # Measure text
    $textSize = $graphics.MeasureString($text, $font)
    $textX = (256 - $textSize.Width) / 2
    $textY = (256 - $textSize.Height) / 2
    
    # Add shadow
    $shadowBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(100, 0, 0, 0))
    $graphics.DrawString($text, $font, $shadowBrush, $textX + 2, $textY + 2)
    
    # Draw main text
    $graphics.DrawString($text, $font, $textBrush, $textX, $textY)
    
    # Add small "S" for "Sale"
    $smallFont = New-Object System.Drawing.Font("Arial", 40, [System.Drawing.FontStyle]::Bold)
    $smallText = "S"
    $smallTextSize = $graphics.MeasureString($smallText, $smallFont)
    $smallTextX = (256 - $smallTextSize.Width) / 2
    $smallTextY = 200
    
    $graphics.DrawString($smallText, $smallFont, $textBrush, $smallTextX, $smallTextY)
    
    # Create Resources directory if it doesn't exist
    if (!(Test-Path "Resources")) {
        New-Item -ItemType Directory -Name "Resources"
    }
    
    # Save as PNG
    $iconPath = "Resources\app-icon.png"
    $bitmap.Save($iconPath, [System.Drawing.Imaging.ImageFormat]::Png)
    
    # Clean up
    $graphics.Dispose()
    $bitmap.Dispose()
    $backgroundBrush.Dispose()
    $innerBrush.Dispose()
    $textBrush.Dispose()
    $shadowBrush.Dispose()
    $font.Dispose()
    $smallFont.Dispose()
    
    Write-Host "Application icon generated successfully: $iconPath" -ForegroundColor Green
    
    # Also create a copy as .ico for the project file
    Copy-Item $iconPath "Resources\app-icon.ico"
    Write-Host "ICO file created: Resources\app-icon.ico" -ForegroundColor Green
    
} catch {
    Write-Host "Error generating icon: $($_.Exception.Message)" -ForegroundColor Red
} 