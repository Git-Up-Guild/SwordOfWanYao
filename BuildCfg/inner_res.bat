@echo off
cd /d %~dp0
echo %~dp0
 

xcopy /y  Inner_res_config\Assets\*.*  ..\Assets /s
