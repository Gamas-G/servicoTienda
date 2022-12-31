sc create OrquestadorCentralWs binpath= "E:\Users\b335913\Desktop\Monitoreo\MONITOREO BUENO\SERVICIOS MONITOREO\Monitoreo\OrquestadorCentral\OrquestadorCentralWs\OrquestadorCentralWs\bin\Debug\OrquestadorCentralWs.exe" DisplayName= "OrquestadorCentral" type= own start= auto
sc description OrquestadorCentralWs "Orquestador Central de Monitoreo"
net start OrquestadorCentralWs
