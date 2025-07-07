set UnityPath=F:\Setup\Unity\2021.3.18f1\Editor

for %%i in (*.dll) do (
    "%UnityPath%\Data\MonoBleedingEdge\bin\mono" "%UnityPath%\Data\MonoBleedingEdge\lib\mono/4.5/pdb2mdb.exe" %%i
)
@pause