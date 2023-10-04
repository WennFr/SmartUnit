﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedLibrary.Handlers.Services;
using SharedLibrary.Models.Devices;

namespace Fan_Device
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IHost? AppHost { get; set; }



        public App()
        {

            DeviceRegistrationSetup();

            AppHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices((config, services) =>
                {
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton(new DeviceConfiguration(config.Configuration.GetConnectionString("FanDevice")!));
                    services.AddTransient<DeviceManager>();
                    services.AddSingleton<NetworkManager>();
                })
                .Build();

        }


        private void DeviceRegistrationSetup()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            
            var root = configurationBuilder.Build();
            var connectionString = root.GetConnectionString("FanDevice");

            if (string.IsNullOrEmpty(connectionString))
            {
                var registrationManager = new RegistrationManager();
                connectionString = registrationManager.RegisterDeviceAndGetConnectionString();

                configurationBuilder = new ConfigurationBuilder();
                root = configurationBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
                var configSource = new Dictionary<string, string>
                {
                    { "ConnectionStrings:FanDevice", connectionString }
                };
                root = configurationBuilder.AddInMemoryCollection(configSource).Build();
            }
        }



        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost!.StartAsync();

            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }
    }
}
