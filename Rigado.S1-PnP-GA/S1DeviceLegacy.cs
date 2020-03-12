using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using Rido;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rigado.S1_PnP_GA
{
    class S1DeviceLegacy
    {
        private string _connectionString;

        private readonly ILogger _logger;
        private readonly CancellationToken _quitSignal;

        public S1DeviceLegacy(string connectionString, ILogger logger, CancellationToken quitSignal)
        {
            _connectionString = connectionString;
            _logger = logger;
            _quitSignal = quitSignal;
        }

        public async Task RunDeviceAsync()
        {
            var deviceFactory = new DeviceClientFactory(_connectionString, _logger);
            var deviceClient = await deviceFactory.CreateDeviceClientAsync();

            var deviceInformation = new DeviceInformation(deviceClient);
            await deviceInformation.UpdatePropertiesAsync();
            
            var s1Sensor = new S1Sensor(deviceClient);
            await s1Sensor.SyncTwinPropertiesAsync();
            await s1Sensor.RegisterCommandsAsync();
            
            while (!_quitSignal.IsCancellationRequested )
            {
                if (s1Sensor.running)
                {
                    await s1Sensor.SendTelemetryAsync();
                }
                else
                {
                    Console.WriteLine($"Waiting {s1Sensor.refreshInterval}s. Status {s1Sensor.running}\n---\n");
                }
                Thread.Sleep(s1Sensor.refreshInterval*1000);
            }
        }
    }
}