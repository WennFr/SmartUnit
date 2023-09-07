using LampDevice.Models;
using SharedLibrary.Handlers.Services;



var telemetryData = new TelemetryDataModel()
{
    IsLampOn = true,
    TemperatureCelsius = 2000,
    CurrentTime = DateTime.Now
};

var device = new DeviceManager<TelemetryDataModel>("HostName=fw-kyh-iothb.azure-devices.net;DeviceId=lamp_device;SharedAccessKey=V+CuvgjqsYVonjXAPpLBZE8GsqIQVo5RMzHU97Cw3tM=", telemetryData);
device.Start();

Console.WriteLine("Press [Enter] to close application");
Console.ReadKey();