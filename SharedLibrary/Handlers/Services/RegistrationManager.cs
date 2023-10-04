using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Handlers.Services
{
    public class RegistrationManager
    {
        private string _connectionString = string.Empty;
        private DeviceClient deviceClient = null!;

        public async Task<string> RegisterDevice(string deviceName, string deviceType)
        {
            using var httpClient = new HttpClient();
            var result = await httpClient.PostAsync($"http://localhost:7193/api/DeviceRegistration?deviceId={deviceName}", null!);
            _connectionString = await result.Content.ReadAsStringAsync();

            deviceClient = DeviceClient.CreateFromConnectionString(_connectionString, TransportType.Mqtt);

            var twinCollection = new TwinCollection();
            twinCollection["deviceType"] = $"{deviceType}";
            await deviceClient.UpdateReportedPropertiesAsync(twinCollection);

            var twin = await deviceClient.GetTwinAsync();

            Console.WriteLine("Device Connected!");
            Console.WriteLine($"{twin.Properties.Reported["deviceType"]}");


            return  _connectionString;
        }

    }
}
