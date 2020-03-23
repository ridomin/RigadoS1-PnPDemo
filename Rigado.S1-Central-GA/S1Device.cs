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

            var deviceInformation = new DeviceInformation(deviceClient);
            await deviceInformation.UpdatePropertiesAsync();
            
            var s1Sensor = new S1Sensor(deviceClient);
            await s1Sensor.SyncTwinPropertiesAsync();
            await s1Sensor.RegisterCommandsAsync();
            await s1Sensor.EnterTelemetryLoopAsync(_quitSignal); 
        }
    }
}