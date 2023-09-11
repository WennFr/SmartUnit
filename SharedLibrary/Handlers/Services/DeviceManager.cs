using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharedLibrary.Models;
using SharedLibrary.Models.Devices;

namespace SharedLibrary.Handlers.Services
{
    public class DeviceManager
    {
        public DeviceConfiguration Configuration { get; set; }

        public LampTelemetryDataModel LampData { get; set; }

        public DeviceManager(string connectionString, LampTelemetryDataModel lampData)
        {
            LampData = lampData;
            Configuration = new DeviceConfiguration(connectionString);
            Configuration.DeviceClient.SetMethodDefaultHandlerAsync(DirectMethodCallback, null).Wait();
        }


        public void Start()
        {
            Task.WhenAll(
                SetTelemetryIntervalAsync(),
                NetworkManager.CheckConnectivityAsync(),
                SendTelemetryAsync()
            );

        }


        private async Task SetTelemetryIntervalAsync()
        {

            var _telemetryInterval = await DeviceTwinManager
                .GetDesiredTwinPropertyAsync(Configuration.DeviceClient, "telemetryInterval");

            if (_telemetryInterval != null)
            {
                Configuration.TelemetryInterval = int.Parse(_telemetryInterval.ToString()!);
            }

            await DeviceTwinManager.UpdateReportedTwinPropertyAsync(Configuration.DeviceClient, "telemetryInterval",
                Configuration.TelemetryInterval);

        }

        private async Task SendTelemetryAsync()
        {
            while (true)
            {

                //if (Configuration.AllowSending)
                //{
                    await SendDataAsync(JsonConvert.SerializeObject(LampData));
                    await Task.Delay(Configuration.TelemetryInterval);

                //}

            }



        }

        private async Task SendDataAsync(string dataAsJson)
        {

            if (!string.IsNullOrEmpty(dataAsJson))
            {
                var message = new Message(Encoding.UTF8.GetBytes(dataAsJson));
                await Configuration.DeviceClient.SendEventAsync(message);

                await DeviceTwinManager.UpdateReportedTwinPropertyAsync(Configuration.DeviceClient, "latestMessage", dataAsJson);

                Console.WriteLine($"Message sent at {DateTime.Now} with data {dataAsJson}");


            }

        }


        private async Task<MethodResponse> DirectMethodCallback(MethodRequest methodRequest, object userContext)
        {

            var response = new
            {
                message = $"Executed Direct Method: {methodRequest.Name}",
            };


            switch (methodRequest.Name.ToLower())
            {
                case "start":
                    Configuration.AllowSending = true;
                    LampData.IsLampOn = true;
                    LampData.TemperatureCelsius = 2000;
                    LampData.CurrentTime = DateTime.Now;

                    await DeviceTwinManager.UpdateReportedTwinPropertyAsync(Configuration.DeviceClient, "allowSending", Configuration.AllowSending);
                    await DeviceTwinManager.UpdateReportedTwinPropertyAsync(Configuration.DeviceClient, "lampOn", LampData.IsLampOn);

                    return new MethodResponse(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)), 200);

                case "stop":
                    Configuration.AllowSending = false;
                    LampData.IsLampOn = false;
                    LampData.TemperatureCelsius = 0;
                    LampData.CurrentTime = DateTime.Now;

                    await DeviceTwinManager.UpdateReportedTwinPropertyAsync(Configuration.DeviceClient, "allowSending", Configuration.AllowSending);
                    await DeviceTwinManager.UpdateReportedTwinPropertyAsync(Configuration.DeviceClient, "lampOn", LampData.IsLampOn);

                    return new MethodResponse(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)), 200);

                case "settelemetryinterval":

                    int desiredInterval;
                    if (int.TryParse(methodRequest.DataAsJson, out desiredInterval))
                    {
                        Configuration.TelemetryInterval = desiredInterval;

                        await DeviceTwinManager.UpdateReportedTwinPropertyAsync(Configuration.DeviceClient, "telemetryInterval", Configuration.TelemetryInterval);
                        return new MethodResponse(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)), 200);
                    }

                    break;


            }

            return new MethodResponse(400);
        }


    }
}
