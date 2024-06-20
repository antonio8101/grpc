$serviceName = "MyGrpcService"
$serviceDisplayName = "My GRPC Service"
$serviceDescription = "A .NET Core GRPC Service running as a Windows Service"
$exePath = "C:\path\to\publish\MyGrpcService.exe --windows-service"

# Controlla se il servizio esiste già
$service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue

if ($service -eq $null) {
    # Se il servizio non esiste, crealo
    New-Service -Name $serviceName -BinaryPathName $exePath -DisplayName $serviceDisplayName -Description $serviceDescription -StartupType Automatic
    Write-Output "Il servizio $serviceName è stato creato con successo."
} else {
    Write-Output "Il servizio $serviceName esiste già."
}

# Facoltativo: Avvia il servizio se è stato appena creato
if ($service -eq $null) {
    Start-Service -Name $serviceName
    Write-Output "Il servizio $serviceName è stato avviato."
}
