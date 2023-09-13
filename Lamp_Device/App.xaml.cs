using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Handlers.Services;

namespace Lamp_Device
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public IHost? AppHost { get; set; }



        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((config, services) =>
                {
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton(new DeviceManager("HostName=fw-kyh-iothb.azure-devices.net;DeviceId=lamp_device;SharedAccessKey=V+CuvgjqsYVonjXAPpLBZE8GsqIQVo5RMzHU97Cw3tM="));
                })
                .Build();

        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost!.StartAsync();

            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();   // med DI
            mainWindow.Show();

            base.OnStartup(e);
        }

    }
}
