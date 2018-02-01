reg delete "HKEY_CURRENT_USER\Software\Mining Station" /f
reg delete "HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run" /v "Mining Station" /f
del "%USERPROFILE%\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup\start-miner*.lnk" /Q
pause
