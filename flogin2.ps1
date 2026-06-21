$adb = "$env:LOCALAPPDATA\Android\Sdk\platform-tools\adb.exe"

# Tap on the username field area
& $adb shell input tap 540 550
Start-Sleep 0.5
# Type admin letter by letter
$chars = @('a','d','m','i','n')
foreach ($c in $chars) {
    & $adb shell input text $c
    Start-Sleep 0.2
}

# Tap password field
& $adb shell input tap 540 700
Start-Sleep 0.5
# Type password
$pchars = @('A','d','m','i','n','1','2','3','4')
foreach ($c in $pchars) {
    & $adb shell input text $c
    Start-Sleep 0.15
}

# Click login button
& $adb shell input tap 540 850
Start-Sleep 5

Write-Output "DONE"
