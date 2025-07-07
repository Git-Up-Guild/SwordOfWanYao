@echo off
cd /d %~dp0
echo %~dp0
 

xcopy /y  .\PubishDll\*.*  ..\Assets\BaseScripts\Dll\ /s

pause
