cd FileHashCraft
dotnet build -c Release -p:Platform=x86
"C:\Program Files\7-Zip\7z.exe" a -r -tzip ..\FileHashCraft_x86.zip .\bin\x86\Release\net8.0-windows\win-x86\*.*
rmdir /s /q bin\x86

dotnet build -c Release -p:Platform=x64
"C:\Program Files\7-Zip\7z.exe" a -r -tzip ..\FileHashCraft_x64.zip .\bin\x64\Release\net8.0-windows\win-x64\*.*
rmdir /s /q bin\x64

dotnet build -c Release -p:Platform=ARM64
"C:\Program Files\7-Zip\7z.exe" a -r -tzip ..\FileHashCraft_Arm64.zip .\bin\ARM64\Release\net8.0-windows\win-arm64\*.*
rmdir /s /q bin\ARM64
