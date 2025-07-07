set UnityPath=%1
set Pdb2MdbToolPath=%2

for %%i in (*.dll) do (
    %1 %2 %%i
)

::pause