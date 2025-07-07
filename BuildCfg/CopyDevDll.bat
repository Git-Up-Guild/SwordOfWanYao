@echo off
cd /d %~dp0
echo %~dp0
 

xcopy /y  .\DevDll\*.*  ..\Assets\BaseScripts\Dll\ /s

pause
