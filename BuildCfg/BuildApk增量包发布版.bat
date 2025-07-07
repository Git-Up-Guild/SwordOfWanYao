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

call publish_res.bat
Unity.exe -projectPath E:\OnlineProject\immortalfamily\trunkPublish\Bin\client\Game\ -executeMethod  XGameEditor.CommandBuildBoot.BuildApkFromCmd   -CompatibleType   -buildApk  -FullPack  -CSCodeUpdate -LuaLog -CheckCodeModified

pause


