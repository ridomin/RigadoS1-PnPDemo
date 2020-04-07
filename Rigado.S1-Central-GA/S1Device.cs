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
        private string _connectionString;

        private readonly ILogger _logger;
        private readonly CancellationToken _quitSignal;

        public S1Device(string connectionString, ILogger logger, CancellationToken quitSignal)
        {
            _connectionString = connectionString;
            _logger = logger;
            _quitSignal = quitSignal;
        }

        public async Task RunDeviceAsync()
        {
            var deviceClient = await DeviceClientFactory.CreateDeviceClientAsync(_connectionString, _logger);

            var deviceInformation = new DeviceInformation(deviceClient, _logger);
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

            var s1Sensor = new S1Sensor(deviceClient, _logger);
            await s1Sensor.ReadTwinPropertiesAsync();
            s1Sensor.RegisterRefreshIntervalUpdated((int interval) => {
                s1Sensor.refreshInterval = interval;
            });

            await s1Sensor.RegisterStartCommandAsync(async (MethodRequest methodRequest, object userContext) => {
                _logger.LogWarning("Executing Start Command");
                s1Sensor.running = true;
                await s1Sensor.ReportTwinPropertiesAsync();
                return await Task.FromResult(new MethodResponse(new byte[0], 200));
            }, null);

            await s1Sensor.RegisterStopCommandAsync(async (MethodRequest methodRequest, object userContext) => {
                _logger.LogWarning("Executing Stop Command");
                s1Sensor.running = false;
                await s1Sensor.ReportTwinPropertiesAsync();
                return await Task.FromResult(new MethodResponse(new byte[0], 200));
            }, null);

            while (!_quitSignal.IsCancellationRequested)
            {
                if (s1Sensor.running)
                {
                    var rnd = new Random();
                    var temp = rnd.NextDouble() + 50.0;
                    var humid = rnd.NextDouble() + 20.1;
                    var batt = rnd.Next(10);
                    await s1Sensor.SendTemperatureHumidityBatteryTelemetryAsync(temp, humid, batt);
                }
                else
                {
                    _logger.LogWarning("Device is stopped");
                }

                _logger.LogInformation($"Waiting {s1Sensor.refreshInterval} s.");
                Thread.Sleep(s1Sensor.refreshInterval * 1000);
            }
        }

        
    }
}