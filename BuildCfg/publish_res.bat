@echo off
cd /d %~dp0
echo %~dp0
 

xcopy /y  publish_res_config\Assets\*.*   ..\Assets  /s
