using Microsoft.Azure.Devices.DigitalTwin.Client;
using Microsoft.Azure.Devices.DigitalTwin.Client.Model;
using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Rigado.S1_PnP_PP
{
    class S1Sensor : DigitalTwinInterfaceClient
    {
        public const string InterfaceId = "urn:rigado:interfaces:S1_Sensor:3";

        int refreshTelemetryInterval = 5;
        bool running = true;

        public S1Sensor(string instanceName) : base(InterfaceId, instanceName)
        {
            ShowTelemetryRefresh();
        }

        protected override async Task<DigitalTwinCommandResponse> OnCommandRequest(DigitalTwinCommandRequest commandRequest)
        {
            Console.WriteLine($"Received Command request: {commandRequest.Name}");
            switch (commandRequest.Name)
            {
                case "start":
                    running = true;
                    break;
                case "stop":
                    running = false;
                    break;
                default:
                    Console.WriteLine($"Command {commandRequest.Name} not supported.");
                    break;
            }
            await ReportRunningProp(running);
            return await Task.FromResult(new DigitalTwinCommandResponse(StatusCodeCompleted, null));
        }

        protected override async Task OnPropertyUpdated(DigitalTwinPropertyUpdate propertyUpdate)
        {
            Console.WriteLine($"Received updates for property [{propertyUpdate.PropertyName}]");

            switch (propertyUpdate.PropertyName)
            {
                case "refreshInterval":
                    refreshTelemetryInterval = JsonSerializer.Deserialize<int>(propertyUpdate.PropertyDesired);
                    await ReportRefreshIntervalAsync(propertyUpdate).ConfigureAwait(false);
                    ShowTelemetryRefresh();
                    break;
                default:
                    Console.WriteLine($"Property name [{propertyUpdate.PropertyName}] is not supported.");
                    return;
            }
        }

        async Task ReportRunningProp(bool running)
        {
            var runningProperty = new DigitalTwinPropertyReport("running", JsonSerializer.Serialize(running));
            await base.ReportPropertiesAsync(new Collection<DigitalTwinPropertyReport> { runningProperty }).ConfigureAwait(false);
        }

        async Task ReportRefreshIntervalAsync(DigitalTwinPropertyUpdate refreshIntervalUpdate)
        {
            Console.WriteLine($"Request to set refresh interval");

            // code to consume customer value, currently just displaying on screen.
            string refreshInterval = refreshIntervalUpdate.PropertyDesired;
            Console.WriteLine($"\tDesired [{refreshInterval}]. \tReported [{refreshIntervalUpdate.PropertyReported}].\tVersion [{refreshIntervalUpdate.DesiredVersion}].");

            // report Completed
            var propertyReport = new Collection<DigitalTwinPropertyReport>
            {
                new DigitalTwinPropertyReport(
                    refreshIntervalUpdate.PropertyName,
                    refreshIntervalUpdate.PropertyDesired,
                    new DigitalTwinPropertyResponse(
                        refreshIntervalUpdate.DesiredVersion,
                        StatusCodeCompleted,
                        "Processing Completed")),
            };
            await base.ReportPropertiesAsync(propertyReport).ConfigureAwait(false);
        }


        public async Task EnterTelemetryLoopAsync(CancellationToken quitSignal)
        {
            async Task SendTelemetryValueAsync(string telemetryName, double telemetryValue)
            {
                Console.WriteLine($"Sending {telemetryName}: [{telemetryValue}].");
                await base.SendTelemetryAsync(telemetryName, JsonSerializer.Serialize(telemetryValue)).ConfigureAwait(false);
            }

            while (!quitSignal.IsCancellationRequested)
            {
                if (running)
                {
                    var rand = new Random();
                    // TODO: when telemetry flattening is supported, send these together
                    await SendTelemetryValueAsync("temperature",rand.NextDouble() + 20.0).ConfigureAwait(false);
                    await SendTelemetryValueAsync("humidity",rand.NextDouble() + 50).ConfigureAwait(false);
                    await SendTelemetryValueAsync("battery", rand.Next(0, 100)).ConfigureAwait(false);
                    Console.WriteLine($"Waiting {refreshTelemetryInterval}s to send more telemetry.\n---\n");
                } 
                else
                {
                    Console.WriteLine("Device is stopped");
                }
                Thread.Sleep(refreshTelemetryInterval * 1000);
            }
        }

        void ShowTelemetryRefresh()
        {
            Console.WriteLine("RefreshInterval set to " + refreshTelemetryInterval);
        }
    }
}
