Dim objShell, dotNetInstalled
Set objShell = CreateObject("WScript.Shell")


dotNetInstalled = False
On Error Resume Next
Dim result
result = objShell.Run("cmd.exe /c dotnet --list-sdks", 0, True)
If Err.Number = 0 Then
    If InStr(result, "5.0") > 0 Then
        dotNetInstalled = True
    End If
End If
On Error GoTo 0

If Not dotNetInstalled Then
    Dim psScriptPath
    psScriptPath = objShell.CurrentDirectory & "\install_dotnet.ps1"

    Dim fso, psFile
    Set fso = CreateObject("Scripting.FileSystemObject")
    Set psFile = fso.CreateTextFile(psScriptPath, True)
    
    psFile.WriteLine("# Ustawienia")
    psFile.WriteLine("$dotnetVersion = ""5.0.403""")
    psFile.WriteLine("$installerUrl = ""https://download.visualstudio.microsoft.com/download/pr/14ccbee3-e812-4068-af47-1631444310d1/3b8da657b99d28f1ae754294c9a8f426/dotnet-sdk-5.0.408-win-x64.exe""")
    psFile.WriteLine("$installerPath = ""$env:TEMP\dotnet-installer.exe""")
    psFile.WriteLine("")
    psFile.WriteLine("# Logowanie do pliku")
    psFile.WriteLine("$logFile = ""$env:TEMP\dotnet-install-log.txt""")
    psFile.WriteLine("Add-Content $logFile ""Rozpoczêcie instalacji .NET 5 w $(Get-Date)""")
    psFile.WriteLine("")
    psFile.WriteLine("# SprawdŸ, czy skrypt jest uruchamiany jako administrator")
    psFile.WriteLine("if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {")
    psFile.WriteLine("    Start-Process powershell ""-NoProfile -ExecutionPolicy Bypass -File `""$PSCommandPath`"""" -Verb RunAs")
    psFile.WriteLine("    exit")
    psFile.WriteLine("}")
    psFile.WriteLine("")
    psFile.WriteLine("# Pobierz instalator")
    psFile.WriteLine("Invoke-WebRequest -Uri $installerUrl -OutFile $installerPath")
    psFile.WriteLine("Add-Content $logFile ""Pobieranie instalatora...""")
    psFile.WriteLine("")
    psFile.WriteLine("# Uruchom instalator")
    psFile.WriteLine("Start-Process -FilePath $installerPath -ArgumentList ""/quiet"", ""/norestart"" -Wait")
    psFile.WriteLine("Add-Content $logFile ""Instalacja zakoñczona.""")
    psFile.WriteLine("")
    psFile.WriteLine("# Usuñ instalator po zakoñczeniu")
    psFile.WriteLine("Remove-Item $installerPath")
    psFile.WriteLine("Add-Content $logFile ""Usuniêto instalator.""")
    psFile.WriteLine("")
    psFile.WriteLine("# Odliczanie przed zakoñczeniem")
    psFile.WriteLine("Add-Content $logFile ""Zamkniêcie instalatora za 3 sekundy...""")
    psFile.WriteLine("for ($i = 3; $i -ge 1; $i--) {")
    psFile.WriteLine("    Add-Content $logFile ""$i...""")
    psFile.WriteLine("    Start-Sleep -Seconds 1")
    psFile.WriteLine("}")

    psFile.Close
    
    objShell.Run "powershell.exe -NoProfile -ExecutionPolicy Bypass -File """ & psScriptPath & """", 0, True
Else
    Dim shortcutPath
    shortcutPath = objShell.SpecialFolders("Desktop") & "\Interpolacja.lnk"
    
    Dim WshShell
    Set WshShell = CreateObject("WScript.Shell")
    Dim shortcut
    Set shortcut = WshShell.CreateShortcut(shortcutPath)
    shortcut.TargetPath = "C:\Path\To\Your\File.exe" 
    shortcut.Description = "Skrót do Interpolacji"
    shortcut.Save
End If


Set objShell = Nothing
Set fso = Nothing
Set psFile = Nothing
Set WshShell = Nothing
Set shortcut = Nothing
