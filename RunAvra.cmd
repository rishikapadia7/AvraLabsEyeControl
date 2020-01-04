TASKKILL /F /IM OptiKey.exe
TASKKILL /F /IM Avra.exe

set myPath="%~dp0
reg query "HKCU\Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers" /v %myPath%Avra\Avra.exe"

if %ERRORLEVEL% == 1 (
	reg add "HKCU\Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers" /v %myPath%Avra\Avra.exe" /t REG_SZ /d "~ HIGHDPIAWARE"
)

cd Avra
start Avra.exe