Set fso = CreateObject("Scripting.FileSystemObject")
Set shell = CreateObject("WScript.Shell")

currentFolder = fso.GetParentFolderName(WScript.ScriptFullName)

psScriptPath = fso.BuildPath(currentFolder, "WpfApp1\dotNet 5 installer.ps1")
command = "powershell.exe -ExecutionPolicy Bypass -File """ & psScriptPath & """"
shell.Run command, 0, True

shortcutPath = fso.BuildPath(currentFolder, "Interpolation App.lnk")
exePath = fso.BuildPath(currentFolder, "WpfApp1\WpfApp1\bin\Debug\net5.0-windows\WpfApp1.exe")

Set shortcut = shell.CreateShortcut(shortcutPath)
shortcut.TargetPath = exePath
shortcut.Save
