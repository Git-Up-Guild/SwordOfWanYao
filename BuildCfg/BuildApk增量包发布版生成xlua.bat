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

call inner_res_cs.bat
Unity.exe -projectPath C:\X1002\X1002_Game\Bin\client\game\ -executeMethod  XGameEditor.CommandBuildBoot.BuildApkFromCmd -Xlua  -IncrementABPack  -CompatibleType -SplitFix  -SplitWeb  -buildApk


