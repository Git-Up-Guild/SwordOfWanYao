@echo off
set PRAMA=%1
shift

:loop
if %1a==a goto :end
set PRAMA=%PRAMA% %1
shift
goto :loop
:end
@echo

taskkill /im unity.exe /f 
"D:/Unity2019.3.2f1/UnitySetup64(2019.3.2f1)/Editor/Unity.exe"  -projectPath "D:/XGame/trunk/Bin/client/Game" -quit -batchmode -logFile build.log -executeMethod XGameEditor.BuildAPPWindow.BuildPublish %PRAMA%

pause