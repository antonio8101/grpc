$serviceName = "MyGrpcService"
$serviceDisplayName = "My GRPC Service"
$serviceDescription = "A .NET Core GRPC Service running as a Windows Service"
$exePath = "C:\Users\abruno\source\repos\GrpcPoc\GrpcPoc.Service\bin\Release\net8.0\publish\GrpcPoc.Service.exe --windows-service"

# Check service existance
$service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue

# Automatic start mode
$serviceRecoveryOptions = New-Object -TypeName "System.ServiceProcess.ServiceController"
$serviceRecoveryOptions.ServiceName = $serviceName
$serviceRecoveryOptions.StartType = [System.ServiceProcess.ServiceStartMode]::Automatic

# Utilizza sc.exe per impostare le opzioni di ripristino del servizio
sc.exe failure $serviceName reset= 0 actions= restart/60000/restart/60000/restart/60000

if ($service -eq $null) {
    # if not exists
    New-Service -Name $serviceName -BinaryPathName $exePath -DisplayName $serviceDisplayName -Description $serviceDescription -StartupType Automatic
    Write-Output "Serivce $serviceName created successfully."
} else {
    Write-Output "Service $serviceName already exists."
}

# Start the service
if ($service -eq $null) {
    Start-Service -Name $serviceName
    Write-Output "Services $serviceName started."
}
