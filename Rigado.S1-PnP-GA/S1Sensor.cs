
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rigado.S1_PnP_GA
{
    class S1Sensor
    {
        string pnpComponentName = string.Empty;

        const string refreshIntervalPropertyName = "refreshInterval";
        public int refreshInterval = 1;

        const string runningPropertyName = "running";
        public bool running = true;
        
        DeviceClient deviceClient;


        internal S1Sensor(DeviceClient client, string componentName)
        {
            deviceClient = client;
            pnpComponentName = "$iotin:" + componentName;
        }

        // Properties
        public async Task SyncTwinPropertiesAsync()
        {
            await UpdatePropertiesAsync();
            await ReadTwinPropsAsync();
            await deviceClient.SetDesiredPropertyUpdateCallbackAsync(this.OnDesiredPropertyChanged, null);
        }

        string GetPropertyValueIfFound(TwinCollection properties, string propertyName)
        {
            string result = string.Empty;

            if (properties.Contains(pnpComponentName))
            {
                if (null!=properties[pnpComponentName][refreshIntervalPropertyName])
                {
                    result = Convert.ToString(properties[pnpComponentName][refreshIntervalPropertyName]["value"]);
                }
            }
            return result;
        }

        async Task ReadTwinPropsAsync()
        {
            var t = await deviceClient.GetTwinAsync();
            var desiredProperties = t.Properties.Desired;
            string desiredPropertyValue = GetPropertyValueIfFound(desiredProperties, refreshIntervalPropertyName);
            if (int.TryParse(desiredPropertyValue, out int intValue))
            {
                refreshInterval = intValue;
            }
        }

        async Task UpdatePropertiesAsync()
        {
            TwinCollection reportedProperties = new TwinCollection();
            reportedProperties[pnpComponentName] = new
            {
                refreshInterval = new
                {
                    value = refreshInterval
                },
                running = new
                {
                    value = running
                }
            };

            await deviceClient.UpdateReportedPropertiesAsync(reportedProperties).ConfigureAwait(false);
        }
        async Task OnDesiredPropertyChanged(TwinCollection desiredProperties, object ctx)
        {
            Console.WriteLine("	Desired property update has received.");
            Console.WriteLine($"	{desiredProperties.ToJson()}");

            string desiredPropertyValue = GetPropertyValueIfFound(desiredProperties, refreshIntervalPropertyName);
            if (int.TryParse(desiredPropertyValue, out int intValue))
            {
                refreshInterval = intValue;
            }
            await UpdatePropertiesAsync();
        }

        //Telemetry
        public async Task EnterTelemetryLoopAsync(CancellationToken quitSignal)
        {
            async Task SendTelemetryAsync()
            {
                var rnd = new Random();
                var temp = rnd.NextDouble() + 50.0;
                var humid = rnd.NextDouble() + 20.1;
                var batt = rnd.Next(10);
                string blob = $"{{\"temperature\": {temp}, \"humidity\": {humid} , \"battery\" : {batt}}}";
                Console.WriteLine($"Sending {blob} . Waiting: {refreshInterval}s");

                var message = Encoding.UTF8.GetBytes(blob);

                await deviceClient.SendEventAsync(new Message(message));
            }

            while (!quitSignal.IsCancellationRequested)
            {
                if (running)
                {
                    await SendTelemetryAsync();
                }
                else
                {
                    Console.WriteLine("Device is stopped");
                }
                Thread.Sleep(refreshInterval * 1000);
            }
        }

        //Commands
        public async Task RegisterCommandsAsync()
        {
            await deviceClient.SetMethodHandlerAsync(pnpComponentName + "*start", start, null);
            await deviceClient.SetMethodHandlerAsync(pnpComponentName + "*stop", stop, null);
        }

        async Task<MethodResponse> start(MethodRequest methodRequest, object userContext)
        {
            running = true;
            await UpdatePropertiesAsync();

            Console.WriteLine($"	 *** start was called.");

            return await Task.FromResult(new MethodResponse(new byte[0], 200));
        }

        async Task<MethodResponse> stop(MethodRequest methodRequest, object userContext)
        {
            running = false;
            await UpdatePropertiesAsync();

            Console.WriteLine($"	 *** stop was called.");

            return await Task.FromResult(new MethodResponse(new byte[0], 200));
        }
    }
}