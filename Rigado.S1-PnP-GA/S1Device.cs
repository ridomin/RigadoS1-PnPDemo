using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using Rido;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rigado.S1_PnP_GA
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
            var deviceFactory = new DeviceClientFactory(_connectionString, _logger);
            var deviceClient = await deviceFactory.CreateDeviceClientAsync();

            var deviceInformation = new DeviceInformation(deviceClient, "Device_information_S1_Sensor");
            await deviceInformation.UpdatePropertiesAsync();
            
            var s1Sensor = new S1Sensor(deviceClient, "S1_Sensor");
            await s1Sensor.SyncTwinPropertiesAsync();
            await s1Sensor.RegisterCommandsAsync();
            await s1Sensor.EnterTelemetryLoopAsync(_quitSignal);
        }
    }
}