using AzureIoT.PnP.DeviceInformation;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.DigitalTwin.Client;
using Microsoft.Extensions.Logging;
using Rido;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rigado.S1_PnP_PP
{
    class S1Device
    {
        public const string CapabilityModelId = "urn:rigado:S1_Sensor:3";

        readonly CancellationToken quitSignal;
        readonly string connectionString;
        readonly ILogger logger;

        public S1Device(string connectionString, ILogger logger, CancellationToken quitSignal)
        {
            this.connectionString = connectionString;
            this.logger = logger;
            this.quitSignal = quitSignal;
        }

        public async Task RunDeviceAsync()
        {
            var deviceInformationInterface = new DeviceInformationInterface("Device_information_S1_Sensor");
            var s1Sensor = new S1Sensor("S1_Sensor");

            DigitalTwinClient digitalTwinClient = await CreateDigitalTwinsClientAsync();

            Console.WriteLine("Registering digital twin intefaces.");
            await digitalTwinClient
                .RegisterInterfacesAsync(
                    CapabilityModelId,
                    new DigitalTwinInterfaceClient[]
                    {
                        s1Sensor,
                        deviceInformationInterface
                    },
                    quitSignal)
                .ConfigureAwait(false);

            await deviceInformationInterface.SendDeviceInfoPropertiesAsync(DeviceInfoProperties).ConfigureAwait(false);
            await s1Sensor.EnterTelemetryLoopAsync(quitSignal).ConfigureAwait(false);
        }

        private async Task<DigitalTwinClient> CreateDigitalTwinsClientAsync()
        {
            var factory = new DeviceClientFactory(connectionString, logger);
            var deviceClient = await factory.CreateDeviceClientAsync();
            deviceClient.SetConnectionStatusChangesHandler((ConnectionStatus status, ConnectionStatusChangeReason reason) => logger.LogWarning($"Connection status changed: {status} {reason}"));
            var digitalTwinClient = new DigitalTwinClient(deviceClient);
            return digitalTwinClient;
        }

        private DeviceInfo DeviceInfoProperties
        {
            get
            {
                return new DeviceInfo()
                {
                    Manufacturer = "Rigado",
                    Model = Environment.OSVersion.Platform.ToString(),
                    OperatingSystemName = Environment.GetEnvironmentVariable("OS"),
                    SoftwareVersion = Environment.OSVersion.VersionString,
                    ProcessorArchitecture = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE"),
                    ProcessorManufacturer = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"),
                    TotalMemory = Environment.WorkingSet,
                    TotalStorage = System.IO.DriveInfo.GetDrives()[0].TotalSize
                };
            }
        }
    }
}
