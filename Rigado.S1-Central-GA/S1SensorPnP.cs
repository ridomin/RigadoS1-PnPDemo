
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rigado.S1_Central_GA
{
    class S1SensorPnP 
    {
        string pnpComponentName = string.Empty;
        const string _runningPropertyName = "running";

        const string _refreshIntervalPropertyName = "refreshInterval";
        public delegate Task RefreshIntervalUpdated(int refreshInterval);
        RefreshIntervalUpdated refreshIntervalCallback;

        readonly DeviceClient _deviceClient;
        readonly ILogger _logger;
        
        internal S1SensorPnP(DeviceClient client, string componentName, ILogger logger) 
        {
            pnpComponentName = "$iotin:" + componentName;
            _logger = logger;
            _deviceClient = client;
            _deviceClient.SetDesiredPropertyUpdateCallbackAsync(this.OnDesiredPropertyChanged, null).Wait();
        }

        public async Task ReadTwinPropertiesAsync()
        {
            var t = await _deviceClient.GetTwinAsync();

            _logger.LogInformation($"S1Sensor.DesiredProperties.Count={t.Properties.Desired.Count}");
            string desiredRefreshInterval = GetPropertyValueIfFound(t.Properties.Desired, _refreshIntervalPropertyName);
            if (int.TryParse(desiredRefreshInterval, out int intValue))
            {
                await SafeRefreshIntervalCallback(intValue);
            }

            // TODO: should we read reported properties?

            //_logger.LogInformation($"S1Sensor.ReportedProperties.Count={t.Properties.Reported.Count}");
            //string reportedRunningValue =GetPropertyValueIfFound(t.Properties.Reported, _runningPropertyName);
            //if (bool.TryParse(reportedRunningValue, out bool reportedRunning))
            //{
            //    _running = reportedRunning;
            //}
        }

        private async Task SafeRefreshIntervalCallback(int intValue)
        {
            if (refreshIntervalCallback != null)
            {
                await refreshIntervalCallback(intValue);
            }
            else
            {
                _logger.LogInformation("RefreshInterval updated, but no callback registered");
            }
        }

        string GetPropertyValueIfFound(TwinCollection properties, string propertyName)
        {
            string result = string.Empty;

            if (properties.Contains(pnpComponentName))
            {
                if (null != properties[pnpComponentName][propertyName])
                {
                    result = Convert.ToString(properties[pnpComponentName][propertyName]["value"]);
                }
            }
            return result;
        }

        public async Task ReportTwinPropertiesAsync(int refreshIntervalValue, bool runningValue)
        {
            TwinCollection reportedProperties = new TwinCollection();
            reportedProperties[pnpComponentName] = new
            {
                refreshInterval = new { value = refreshIntervalValue },
                running = new { value = runningValue } 
            };
            await _deviceClient.UpdateReportedPropertiesAsync(reportedProperties).ConfigureAwait(false);
        }

        public void RegisterRefreshIntervalUpdated(RefreshIntervalUpdated cb)
        {
            refreshIntervalCallback = cb;
        }

        async Task OnDesiredPropertyChanged(TwinCollection desiredProperties, object ctx)
        {
            _logger.LogWarning("Desired property update received.");
            _logger.LogInformation($"	{desiredProperties.ToJson()}");

            string desiredPropertyValue = GetPropertyValueIfFound(desiredProperties, _refreshIntervalPropertyName);
            if (int.TryParse(desiredPropertyValue, out int intValue))
            {
                await SafeRefreshIntervalCallback(intValue);
            }
            else
            {
                _logger.LogWarning($"Desired Property {_refreshIntervalPropertyName} : {desiredPropertyValue} no found or not valid.");
            }
        }

        //Telemetry

        public async Task SendTemperatureTelemetryAsync(double temperature)
        {
            _logger.LogInformation($"Sending Telemetry: temperature:{temperature}");
            await _deviceClient.SendEventAsync(new Message(Encoding.UTF8.GetBytes($"{{\"temperature\": {temperature} }}")));
        }

        public async Task SendHumidityTelemetryAsync(double humidity)
        {
            _logger.LogInformation($"Sending Telemetry: humidity:{humidity} ");
            await _deviceClient.SendEventAsync(new Message(Encoding.UTF8.GetBytes($"{{\"humidity\": {humidity} }}")));
        }

        public async Task SendBatteryTelemetryAsync(int battery)
        {
            _logger.LogInformation($"Sending Telemetry: battery:{battery}");
            await _deviceClient.SendEventAsync(new Message(Encoding.UTF8.GetBytes($"{{\"battery\": {battery} }}")));
        }

        public async Task SendTemperatureHumidityBatteryTelemetryAsync(double temperature, double humidity, int battery)
        {
            _logger.LogInformation($"Sending Telemetry: temperature:{temperature} humidity:{humidity} battery:{battery}");
            var msg = new Message(
                    Encoding.UTF8.GetBytes(
                        $"{{\"temperature\": {temperature}, \"humidity\": {humidity} , \"battery\" : {battery}}}"
                        )
                    );

            //msg.Properties.Add("iothub-interface-id", "urn:rigado:interfaces:S1_Sensor:3");
            //msg.Properties.Add("$.ifid", "urn:rigado:interfaces:S1_Sensor:3");
            //msg.Properties.Add("iothub-interface-name", "S1_Sensor");
            //msg.Properties.Add("$.ifname", "S1_Sensor");
            //msg.ContentType = "application/json";
            //msg.ContentEncoding = "utf-8";
           // msg.MessageSchema = "Telemetry";
            await _deviceClient.SendEventAsync(msg);
        }

        //Commands
        public async Task RegisterStartCommandAsync(MethodCallback methodHandler, object userContext)
        {
            await _deviceClient.SetMethodHandlerAsync(pnpComponentName + "*start", methodHandler, userContext);
        }

        public async Task RegisterStopCommandAsync(MethodCallback methodHandler, object userContext)
        {
            await _deviceClient.SetMethodHandlerAsync(pnpComponentName + "*stop", methodHandler, userContext);
        }
    }
}