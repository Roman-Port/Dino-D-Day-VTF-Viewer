@echo off
echo Copying files...
mkdir %appdata%\rpvtf\
xcopy DinoDDayVtfEditor\bin\Debug %appdata%\rpvtf\ /s /e
xcopy SetupVtfFile\bin\Debug %appdata%\rpvtf\ /s /e
cls
echo Registering .vtf so you can double click on it to view. (don't worry, this won't break Source)
echo This will require admin. Is that okay? (if not, close this window. It'll still work.)
pause
%appdata%\rpvtf\SetupVtfFile.exe
echo Done, thanks!
pause