for /f "tokens=1-3" %%a in ('date /t') do set Fecha=%%a%%b%%c
for /f "tokens=1-2" %%a in ('time /t') do set Hora=%%a%%b

if NOT EXIST "E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\" (
 mkdir "E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\"
 )
 
ECHO %Fecha% %Hora%: 	   >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log
ECHO Deteniendo servicio (si existe)...>> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log

SC STOP WSDMPDV

TIMEOUT /T 8 /NOBREAK

ECHO Eliminando servicio (si existe)...>> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log

SC DELETE WSDMPDV >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log

ECHO Finalizando procesos asociados al servicio (si existen)...>> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log

SET /A "indiceTasklist=1"
SET /A "iteracionesTasklist=60"

:loopTaskKill
TASKLIST /nh /fi "imagename eq DMPMSDV.exe" | find "DMPMSDV"
if %errorlevel% == 0 (	
	SET /A "indiceTasklist=indiceTasklist + 1"
	echo Existe proceso DMPMSDV.exe ejecutar TASKKILL #%indiceTasklist% >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log
	
	if %indiceTasklist% leq %iteracionesTasklist% (
		TASKKILL /im DMPMSDV.exe /f /t >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log
		C:\Windows\System32\PING.EXE -n 5 127.0.0.1>nul
		goto loopTaskKill
	)	
)

ECHO Iniciando proceso de instalacion...>> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log
ECHO Creando servicio...>> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log

SC CREATE WSDMPDV binpath= "E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\DirecVirtuales\DMPMSDV.exe" DisplayName= "DMP Monitoreo Directorios Virtuales" type= own start= auto

ECHO Servicio creado...>> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log
ECHO Estableciendo descripcion del servicio...>> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log

SC DESCRIPTION WSDMPDV "Microservicio Directorios Virtuales." >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log

ECHO Iniciando servicio...>> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log

SC START WSDMPDV >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log

ECHO Ejecutando comprobacion de estatus despues de la instalacion...>> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log

TASKLIST /fi "imagename eq DMPMSDV.exe" >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log
SC QC WSDMPDV >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log
SC QUERY WSDMPDV >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log

echo Finalizando proceso de instalacion del servicio WSDMPDV... >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log

for /f "tokens=1-3" %%a in ('date /t') do set Fecha=%%a%%b%%c
for /f "tokens=1-2" %%a in ('time /t') do set Hora=%%a%%b

ECHO %Fecha% %Hora%: >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log