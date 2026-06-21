$adb = "$env:LOCALAPPDATA\Android\Sdk\platform-tools\adb.exe"

# Force stop and restart app
& $adb shell am force-stop com.warehousekg.warehousekg_mobile
Start-Sleep 1
& $adb shell am start -n com.warehousekg.warehousekg_mobile/.MainActivity
Start-Sleep 4

# Tap username field
& $adb shell input tap 540 550
Start-Sleep 0.5
# Set clipboard and paste
& $adb shell cmd clipboard set "admin"
Start-Sleep 0.2
& $adb shell input keyevent 279
Start-Sleep 0.4

# Tap password field  
& $adb shell input tap 540 700
Start-Sleep 0.5
& $adb shell cmd clipboard set "Admin1234!"
Start-Sleep 0.2
& $adb shell input keyevent 279
Start-Sleep 0.4

# Tap login button
& $adb shell input tap 540 850

Write-Output "DONE"
