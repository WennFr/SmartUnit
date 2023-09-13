using SharedLibrary.Handlers.Services;
using SharedLibrary.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace LampDevice
{
    public class Program
    {
        public static IHost? AppHost { get; set; }


        static async Task Main(string[] args)
        {
           

            AppHost = Host.CreateDefaultBuilder(args)
                .ConfigureServices((config, services) =>
                {
                    services.AddSingleton(new DeviceManager("HostName=fw-kyh-iothb.azure-devices.net;DeviceId=lamp_device;SharedAccessKey=V+CuvgjqsYVonjXAPpLBZE8GsqIQVo5RMzHU97Cw3tM="));

                })
                .Build();


            using var scope = AppHost.Services.CreateScope();
            var services = scope.ServiceProvider;
            var deviceManager = services.GetRequiredService<DeviceManager>();

            await AppHost.StartAsync();
            Console.Clear();
            Console.WriteLine("Console device started");

            await SendTelemetryDataAsync(deviceManager);
            Console.WriteLine("Press [Enter] to close application");
            Console.ReadKey();

        }


        private static async Task SendTelemetryDataAsync(DeviceManager deviceManager)
        {

            while (true)
            {
                if (deviceManager.Configuration.AllowSending)
                {
                    var payload = new LampTelemetryDataModel()
                    {
                        IsLampOn = true,
                        TemperatureCelsius = 2000,
                        CurrentTime = DateTime.Now
                    };

                    var json = JsonConvert.SerializeObject(payload);

                    if (await deviceManager.SendDataAsync(JsonConvert.SerializeObject(json)))
                        Console.WriteLine($"Message sent successfully: {json}");

                    await Task.Delay(deviceManager.Configuration.TelemetryInterval);
                }
            }

        }

    }
}