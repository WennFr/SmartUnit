using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharedLibrary.Models;
using SharedLibrary.Models.Devices;
using System.Diagnostics;

namespace SharedLibrary.Handlers.Services
{
    public class DeviceManager
    {
        public DeviceConfiguration Configuration { get; set; }


        public DeviceManager(string connectionString)
        {
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
                    //await SendDataAsync(JsonConvert.SerializeObject());
                    //await Task.Delay(Configuration.TelemetryInterval);

                //}

            }



        }

        //public async Task SendDataAsync(string dataAsJson)
        //{

        //    if (!string.IsNullOrEmpty(dataAsJson))
        //    {
        //        var message = new Message(Encoding.UTF8.GetBytes(dataAsJson));
        //        await Configuration.DeviceClient.SendEventAsync(message);

        //        await DeviceTwinManager.UpdateReportedTwinPropertyAsync(Configuration.DeviceClient, "latestMessage", dataAsJson);

        //        Console.WriteLine($"Message sent at {DateTime.Now} with data {dataAsJson}");


        //    }

        //}

        public async Task<bool> SendDataAsync(string payload)
        {
            try
            {
                var message = new Message(Encoding.UTF8.GetBytes(payload));
                await Configuration.DeviceClient.SendEventAsync(message);
                await DeviceTwinManager.UpdateReportedTwinPropertyAsync(Configuration.DeviceClient, "latestMessage", payload);
                await Task.Delay(10);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return false;
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

                    await DeviceTwinManager.UpdateReportedTwinPropertyAsync(Configuration.DeviceClient, "allowSending", Configuration.AllowSending);

                    return new MethodResponse(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)), 200);

                case "stop":
                    Configuration.AllowSending = false;
                    await DeviceTwinManager.UpdateReportedTwinPropertyAsync(Configuration.DeviceClient, "allowSending", Configuration.AllowSending);

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
