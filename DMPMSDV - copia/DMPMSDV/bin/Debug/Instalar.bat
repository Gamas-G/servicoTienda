sc create DMPMSDV binpath= "E:\Users\b335913\Desktop\Monitoreo\MONITOREO BUENO\SERVICIOS MONITOREO\Monitoreo\Microservicios\DMPMSDV\DMPMSDV\bin\Debug\DMPMSDV.exe" DisplayName= "DMPMSDV" type= own start= auto
sc description OrquestadorWS "DMPMSDV"
net start DMPMSDV
pause