@echo off
echo Building self-contained EXE...
dotnet restore
dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true /p:SelfContained=true -o publish
echo Done. EXE at publish\EbayDesk.exe
pause
