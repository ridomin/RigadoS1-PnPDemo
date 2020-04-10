using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using Rido;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rigado.S1_Central_GA
{
    class S1Device
    {
        private readonly string _connectionString;

        private readonly ILogger _logger;
        private readonly CancellationToken _quitSignal;

        int s1Sensor_refreshInterval = 3;
        bool s1Sensor_running = true;

        public S1Device(string connectionString, ILogger logger, CancellationToken quitSignal)
        {
            _connectionString = connectionString;
            _logger = logger;
            _quitSignal = quitSignal;
        }

        public async Task RunDeviceAsync()
        {
            var deviceClient = await DeviceClientFactory.CreateDeviceClientAsync(_connectionString, _logger);

            var deviceInformation = new DeviceInformationPnP(deviceClient, "Device_information_S1_Sensor", _logger);
            await deviceInformation.ReadTwinPropertiesAsync();
            deviceInformation.manufacturer = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"); // (manufacturer)
            deviceInformation.model = Environment.OSVersion.Platform.ToString();// (model)
            deviceInformation.swVersion = Environment.OSVersion.VersionString; ; // (swVersion)
            deviceInformation.osName = Environment.GetEnvironmentVariable("OS"); // (osName)
            deviceInformation.processorArchitecture = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE"); // (processorArchitecture)
            deviceInformation.processorManufacturer = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"); // (processorManufacturer)
            deviceInformation.totalStorage = System.IO.DriveInfo.GetDrives()[0].TotalSize; // (totalStorage) <- try another value!
            deviceInformation.totalMemory = Environment.WorkingSet; // (totalMemory) <- try another value!
            await deviceInformation.ReportTwinPropertiesAsync();

            var s1Sensor = new S1SensorPnP(deviceClient, "S1_Sensor", _logger);
            //commands
            await s1Sensor.RegisterStartCommandAsync(async (MethodRequest methodRequest, object userContext) => {
                _logger.LogWarning("Executing start Command");
                s1Sensor_running = true;
                await s1Sensor.ReportTwinPropertiesAsync(s1Sensor_refreshInterval, s1Sensor_running);
                return await Task.FromResult(new MethodResponse(new byte[0], 200));
            }, null);

            await s1Sensor.RegisterStopCommandAsync(async (MethodRequest methodRequest, object userContext) => {
                _logger.LogWarning("Executing stop Command");
                s1Sensor_running = false;
                await s1Sensor.ReportTwinPropertiesAsync(s1Sensor_refreshInterval, s1Sensor_running);
                return await Task.FromResult(new MethodResponse(new byte[0], 200));
            }, null);

            //properties

            s1Sensor.RegisterRefreshIntervalUpdated((int interval) => {
                s1Sensor_refreshInterval = interval;
                return Task.FromResult(0);
            });

            await s1Sensor.ReadTwinPropertiesAsync();
            await s1Sensor.ReportTwinPropertiesAsync(s1Sensor_refreshInterval, s1Sensor_running);

            //telemetry
            while (!_quitSignal.IsCancellationRequested)
            {
                if (s1Sensor_running)
                {
                    var rnd = new Random();
                    var temp = rnd.NextDouble() + 50.0;
                    var humid = rnd.NextDouble() + 20.1;
                    var batt = rnd.Next(10);
                    await s1Sensor.SendTemperatureHumidityBatteryTelemetryAsync(temp, humid, batt);
                    //await s1Sensor.SendTemperatureTelemetryAsync(temp);
                }
                else
                {
                    _logger.LogWarning("Device is stopped");
                }

                _logger.LogInformation($"Waiting {s1Sensor_refreshInterval} s.");
                Thread.Sleep(s1Sensor_refreshInterval * 1000);
            }
        }

        
    }
}