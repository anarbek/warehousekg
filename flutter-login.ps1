$adb = "$env:LOCALAPPDATA\Android\Sdk\platform-tools\adb.exe"

# Tap username field
& $adb shell input tap 540 600
Start-Sleep 0.5

# Type username
& $adb shell input text admin
Start-Sleep 0.5

# Tap password field
& $adb shell input tap 540 750
Start-Sleep 0.5

# Type password
& $adb shell input text "Admin1234\!"
Start-Sleep 0.5

# Tap login button
& $adb shell input tap 540 920
Start-Sleep 3

"Login submitted"
"Done"
