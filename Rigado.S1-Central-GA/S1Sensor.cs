
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rigado.S1_Central_GA
{
    class S1Sensor 
    {
        const string _runningPropertyName = "running";

        const string _refreshIntervalPropertyName = "refreshInterval";
        public delegate Task RefreshIntervalUpdated(int refreshInterval);
        RefreshIntervalUpdated refreshIntervalCallback;

        readonly DeviceClient _deviceClient;
        readonly ILogger _logger;
        
        internal S1Sensor(DeviceClient client, ILogger logger) 
        {
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
                if (refreshIntervalCallback!=null)
                { 
                    await refreshIntervalCallback(intValue);
                } 
                else
                {
                    _logger.LogInformation("RefreshInterval updated, but no callback registered");
                }
            }

            // TODO: should we read reported properties?

            //_logger.LogInformation($"S1Sensor.ReportedProperties.Count={t.Properties.Reported.Count}");
            //string reportedRunningValue =GetPropertyValueIfFound(t.Properties.Reported, _runningPropertyName);
            //if (bool.TryParse(reportedRunningValue, out bool reportedRunning))
            //{
            //    _running = reportedRunning;
            //}
        }

        string GetPropertyValueIfFound(TwinCollection properties, string propertyName)
        {
            string result = string.Empty;
            if (properties.Contains(propertyName))
            {
                var prop = properties[propertyName];
                var propVal = prop["value"];
                result = Convert.ToString(propVal);
            }
            return result;
        }
        
        public async Task ReportTwinPropertiesAsync(int refreshInterval, bool running)
        {
            TwinCollection reportedProperties = new TwinCollection();
            reportedProperties[_runningPropertyName] =  new { value = running } ; 
            reportedProperties[_refreshIntervalPropertyName] = new { value = refreshInterval };
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
                await refreshIntervalCallback(intValue);
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
            await _deviceClient.SendEventAsync(
                new Message(
                    Encoding.UTF8.GetBytes(
                        $"{{\"temperature\": {temperature}, \"humidity\": {humidity} , \"battery\" : {battery}}}"
                        )
                    )
                );
        }

        //Commands
        public async Task RegisterStartCommandAsync(MethodCallback methodHandler, object userContext)
        {
            await _deviceClient.SetMethodHandlerAsync("start", methodHandler, userContext);
        }

        public async Task RegisterStopCommandAsync(MethodCallback methodHandler, object userContext)
        {
            await _deviceClient.SetMethodHandlerAsync("stop", methodHandler, userContext);
        }
    }
}