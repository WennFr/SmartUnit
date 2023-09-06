using SharedLibrary.Handlers.Services;

var device = new DeviceManager("HostName=fw-kyh-iothb.azure-devices.net;DeviceId=advanced_device;SharedAccessKey=WB3KAmx01yMMOJEJ5iXJs0S2gLlcI5+Az6qfar8FMzA=");

device.Start();