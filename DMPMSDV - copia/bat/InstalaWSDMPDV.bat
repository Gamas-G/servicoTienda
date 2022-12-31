for /f "tokens=1-3" %%a in ('date /t') do set Fecha=%%a%%b%%c
for /f "tokens=1-2" %%a in ('time /t') do set Hora=%%a%%b

ECHO %Fecha% %Hora%: 	   >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log
ECHO Deteniendo servicio (si existe)...>> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log

SC STOP WSDMPDV

ECHO Eliminando servicio (si existe)...>> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log

SC DELETE WSDMPDV >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log

ECHO Finalizando procesos asociados al servicio (si existen)...>> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log

SET /A "indiceTasklist=1"
SET /A "iteracionesTasklist=60"

:loopTaskKill
TASKLIST /nh /fi "imagename eq WSDMPDV.exe" | find "WSDMPDV"
if %errorlevel% == 0 (	
	SET /A "indiceTasklist=indiceTasklist + 1"
	echo Existe proceso WSDMPDV.exe ejecutar TASKKILL #%indiceTasklist% >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log
	
	if %indiceTasklist% leq %iteracionesTasklist% (
		TASKKILL /im WSDMPDV.exe /f /t >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log
		C:\Windows\System32\PING.EXE -n 5 127.0.0.1>nul
		goto loopTaskKill
	)	
)

ECHO Iniciando proceso de instalacion...>> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log
ECHO Creando servicio...>> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log

SC CREATE WSDMPDV binpath= "E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\MicroServicios\WSDMPDV.exe" DisplayName= "DMP Monitoreo Directorios Virtuales" type= own

ECHO Servicio creado...>> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log
ECHO Estableciendo descripcion del servicio...>> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log

SC DESCRIPTION WSDMPDV "Microservicio Directorios Virtuales." >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log

ECHO Ejecutando comprobacion de estatus despues de la instalacion...>> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log

TASKLIST /fi "imagename eq WSDMPDV.exe" >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log
SC QC WSDMPDV >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log
SC QUERY WSDMPDV >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log

echo Finalizando proceso de instalacion del servicio WSDMPDV... >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log

for /f "tokens=1-3" %%a in ('date /t') do set Fecha=%%a%%b%%c
for /f "tokens=1-2" %%a in ('time /t') do set Hora=%%a%%b

ECHO %Fecha% %Hora%: >> E:\ADN\NET64_BAZ\DMP\Servicios\Monitoreo\Logs\WSDMPDV.install.log