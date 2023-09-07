using LampDevice.Models;
using SharedLibrary.Handlers.Services;



var telemetryData = new TelemetryDataModel()
{
    IsLampOn = true,
    TemperatureCelsius = 2000,
    CurrentTime = DateTime.Now
};

var device = new DeviceManager<TelemetryDataModel>("HostName=fw-kyh-iothb.azure-devices.net;DeviceId=advanced_device;SharedAccessKey=WB3KAmx01yMMOJEJ5iXJs0S2gLlcI5+Az6qfar8FMzA=", telemetryData);
device.Start();