using SharedLibrary.Handlers.Services;
using SharedLibrary.Models;


var telemetryData = new LampTelemetryDataModel()
{
    IsLampOn = false,
    TemperatureCelsius = 0,
    CurrentTime = DateTime.Now
};

var device = new DeviceManager("HostName=fw-kyh-iothb.azure-devices.net;DeviceId=lamp_device;SharedAccessKey=V+CuvgjqsYVonjXAPpLBZE8GsqIQVo5RMzHU97Cw3tM=", telemetryData);
device.Start();

Console.WriteLine("Press [Enter] to close application");
Console.ReadKey();