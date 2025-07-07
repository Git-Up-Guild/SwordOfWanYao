
@rem 记录当前路径
@set CURPATH=%CD%

@rem 更新当前项目
svn update


@set NOT_WAIT_USER=0



@rem  游戏表格
cd %CURPATH%/..\..\tools\datapresent
@call "Update&GenData.bat"


@rem  游戏协议
cd %CURPATH%/..\..\..\tools\datapresent
@call "Update&GenProto.bat"

@rem 返回原来目录
cd %CURPATH%



pause


