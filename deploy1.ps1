$serviceName = "MyGrpcService"
$serviceDisplayName = "My gRPC Service"
$serviceDescription = "A .NET Core gRPC Service running as a Windows Service"
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

# Configura il servizio per il riavvio automatico
$serviceRecoveryOptions = New-Object -TypeName "System.ServiceProcess.ServiceController"
$serviceRecoveryOptions.ServiceName = $serviceName
$serviceRecoveryOptions.StartType = [System.ServiceProcess.ServiceStartMode]::Automatic

# Imposta le opzioni di ripristino del servizio
$serviceRecoveryOptionsFailureActions = @{
    FailureActionsFlag = 1
    ResetPeriod = 0
    RebootMsg = ''
    Command = ''
    Actions = @(
        (New-Object -TypeName "System.ServiceProcess.RecoveryAction" -ArgumentList ([System.ServiceProcess.ServiceActionType]::Restart, 60000)),
        (New-Object -TypeName "System.ServiceProcess.RecoveryAction" -ArgumentList ([System.ServiceProcess.ServiceActionType]::Restart, 60000)),
        (New-Object -TypeName "System.ServiceProcess.RecoveryAction" -ArgumentList ([System.ServiceProcess.ServiceActionType]::Restart, 60000))
    )
}

Set-Service -Name $serviceName -StartupType Automatic

# Utilizza sc.exe per impostare le opzioni di ripristino del servizio
sc.exe failure $serviceName reset= 0 actions= restart/60000/restart/60000/restart/60000

# Facoltativo: Avvia il servizio se è stato appena creato
if ($service -eq $null) {
    Start-Service -Name $serviceName
    Write-Output "Il servizio $serviceName è stato avviato."
}
