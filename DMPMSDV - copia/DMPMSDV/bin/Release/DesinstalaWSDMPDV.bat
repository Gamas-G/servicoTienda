for /f "tokens=1-3" %%a in ('date /t') do set Fecha=%%a%%b%%c
for /f "tokens=1-2" %%a in ('time /t') do set Hora=%%a%%b

if NOT EXIST "E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\" (
 mkdir "E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\"
 )
 
ECHO %Fecha% %Hora%: >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\DesinstalaWSDMPDV.log

ECHO Deteniendo servicio WSDMPDV >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\DesinstalaWSDMPDV.log

SC STOP WSDMPDV

TIMEOUT /T 8 /NOBREAK

SC DELETE WSDMPDV
SET /A "indiceTasklist=1"
SET /A "iteracionesTasklist=60"

:loopTaskKill
TASKLIST /nh /fi "imagename eq WSDMPDV.exe" | find "WSDMPDV"
if %errorlevel% == 0 (	
	SET /A "indiceTasklist=indiceTasklist + 1"
	echo Existe proceso WSDMPDV.exe ejecutar TASKKILL #%indiceTasklist% >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\DesinstalaWSDMPDV.log
	
	if %indiceTasklist% leq %iteracionesTasklist% (
		TASKKILL /im WSDMPDV.exe /f /t >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\DesinstalaWSDMPDV.log
		C:\Windows\System32\PING.EXE -n 5 127.0.0.1>nul
		goto loopTaskKill
	)	
)

echo -------CONSULTA INFO [FIN]------ >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\DesinstalaWSDMPDV.log
REM consultar el status del windows service despues de desinstalar
TASKLIST /fi "imagename eq WSDMPDV.exe" >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\DesinstalaWSDMPDV.log
SC QC WSDMPDV >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\DesinstalaWSDMPDV.log
SC QUERY WSDMPDV >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\DesinstalaWSDMPDV.log

for /f "tokens=1-3" %%a in ('date /t') do set Fecha=%%a%%b%%c
for /f "tokens=1-2" %%a in ('time /t') do set Hora=%%a%%b

ECHO %Fecha% %Hora%: >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\DesinstalaWSDMPDV.log
