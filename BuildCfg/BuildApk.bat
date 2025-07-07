@echo off
taskkill /f /im Unity.exe
set PRAMA=%1
shift

:loop
if %1a==a goto :end
set PRAMA=%PRAMA% %1
shift
goto :loop
:end
@echo

call inner_res.bat

Unity.exe -projectPath C:\WMCQ\trunk\Bin\client\Game\ -executeMethod  XGameEditor.CommandBuildBoot.BuildApkFromCmd -IncrementABPack -CompatibleType -LuaLog -debug  -buildApk 
pause 


