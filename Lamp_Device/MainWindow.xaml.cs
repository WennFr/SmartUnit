using Newtonsoft.Json;
using SharedLibrary.Handlers.Services;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lamp_Device
{
    public partial class MainWindow : Window
    {
        private readonly DeviceManager _deviceManager;

        private readonly NetworkManager _networkManager;


        public MainWindow(DeviceManager deviceManager, NetworkManager networkManager)
        {
            InitializeComponent();
            _deviceManager = deviceManager;
            _networkManager = networkManager;
            Task.WhenAll(SetDeviceTypeAsync(), SendTelemetryDataAsync(), CheckConnectivityAsync(), ToggleLampStateAsync());
        }


        private async Task ToggleLampStateAsync()
        {

            while (true)
            {

                if (_deviceManager.AllowSending())
                {
                    LampOnIcon.Visibility = Visibility.Visible;
                    LampOffIcon.Visibility = Visibility.Collapsed;
                }
                else
                {
                    LampOnIcon.Visibility = Visibility.Collapsed;
                    LampOffIcon.Visibility = Visibility.Visible;

                }

                await Task.Delay(1000);
            }
        }

        private async Task CheckConnectivityAsync()
        {
            while (true)
            {
                ConnectivityStatus.Text = await NetworkManager.CheckConnectivityAsync();
                await Task.Delay(1000);

            }
        }

        private async Task SetDeviceTypeAsync()
        {
            var deviceType = "Lamp";

            await _deviceManager.SendDeviceTypeAsync(deviceType);
        }




        private async Task SendTelemetryDataAsync()
        {

            while (true)
            {
                if (_deviceManager.Configuration.AllowSending)
                {
                    var dataModel = new LampTelemetryDataModel()
                    {
                        IsLampOn = true,
                        TemperatureCelsius = 2000,
                        CurrentTime = DateTime.Now
                    };


                    var latestMessageJson = JsonConvert.SerializeObject(new
                    {
                        TemperatureCelsius = dataModel.TemperatureCelsius,
                        currentTime = dataModel.CurrentTime
                    });


                    var operationalStatusJson = JsonConvert.SerializeObject(dataModel.IsLampOn);


                    if (await _deviceManager.SendMessageAsync(latestMessageJson) &&
                        await _deviceManager.SendOperationalStatusAsync(operationalStatusJson))
                        CurrentMessageSent.Text =
                            $"Message sent successfully: {latestMessageJson} DeviceOn: {operationalStatusJson}";


                    var telemetryInterval = _deviceManager.Configuration.TelemetryInterval;

                    await Task.Delay(telemetryInterval);
                }
            }

        }

    }
}
