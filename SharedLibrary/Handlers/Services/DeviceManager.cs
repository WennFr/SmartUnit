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
            Task.WhenAll(Configuration.DeviceClient.SetMethodDefaultHandlerAsync(DirectMethodCallback, null),
                SetTelemetryIntervalAsync(), NetworkManager.CheckConnectivityAsync());

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

        private async Task<MethodResponse> DirectMethodCallback(MethodRequest req, object userContext)
        {

            var res = new MethodDataResponse();

            try
            {
                res.Message = $"Method: {req.Name} executed successfully.";

                switch (req.Name.ToLower())
                {
                    case "start":
                        Configuration.AllowSending = true;
                        await DeviceTwinManager.UpdateReportedTwinPropertyAsync(Configuration.DeviceClient, "allowSending", Configuration.AllowSending);
                        break;

                    case "stop":
                        Configuration.AllowSending = false;
                        await DeviceTwinManager.UpdateReportedTwinPropertyAsync(Configuration.DeviceClient, "allowSending", Configuration.AllowSending);
                        break;

                    case "settelemetryinterval":

                        int desiredInterval;
                        if (int.TryParse(req.DataAsJson, out desiredInterval))
                        {
                            Configuration.TelemetryInterval = desiredInterval;
                            await DeviceTwinManager.UpdateReportedTwinPropertyAsync(Configuration.DeviceClient, "telemetryInterval", Configuration.TelemetryInterval);
                        }

                        break;

                    default:
                        res.Message = $"Method: {req.Name} could not be found.";
                        return new MethodResponse(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(res)), 404);

                }

                await Task.Delay(10);
                return new MethodResponse(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(res)), 200);

            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                return new MethodResponse(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(res)), 400);

            }
        }


    }
}
