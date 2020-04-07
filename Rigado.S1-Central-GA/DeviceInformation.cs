using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Rigado.S1_Central_GA
{
    class DeviceInformation 
    {
        string _manufacturer = string.Empty;
        string _model = string.Empty;
        string _swVersion = string.Empty;
        string _osName = string.Empty;
        string _processorArchitecture = string.Empty;
        string _processorManufacturer = string.Empty;
        double _totalStorage = 0;
        double _totalMemory = 0;

        DeviceClient _deviceClient;
        TwinCollection _reportedProperties = new TwinCollection();

        ILogger _logger;

        public DeviceInformation(DeviceClient deviceClient, ILogger logger)
        {
            _deviceClient = deviceClient;
            _logger = logger;
            ReadTwinPropertiesAsync().Wait();
        }

        public async Task ReadTwinPropertiesAsync()
        {
            var twin = await _deviceClient.GetTwinAsync();
            var reportedProperties = twin.Properties.Reported;
            _logger.LogInformation($"ReportedProperties.Count={reportedProperties.Count}");
            if (reportedProperties.Count > 7 && reportedProperties.Contains("manufacturer"))
            {
                _manufacturer = Convert.ToString(reportedProperties["manufacturer"]);
                _model = Convert.ToString(reportedProperties["model"]);
                _swVersion = Convert.ToString(reportedProperties["swVersion"]);
                _osName = Convert.ToString(reportedProperties["osName"]);
                _processorArchitecture = Convert.ToString(reportedProperties["processorArchitecture"]);
                _processorManufacturer = Convert.ToString(reportedProperties["processorManufacturer"]);
                _totalStorage = Convert.ToDouble(reportedProperties["totalStorage"]);
                _totalMemory = Convert.ToDouble(reportedProperties["totalMemory"]);

                _logger.LogWarning($"Reported Properties: {_manufacturer} {_model} {_swVersion} {_osName} {_processorArchitecture} {_processorManufacturer} {_totalStorage} {_totalMemory}");
            }
            else
            {
                _logger.LogWarning("DeviceInfo Properties are not available, usually happens on first device connection.");
            }
        }

        public async Task ReportTwinPropertiesAsync()
        {
            await _deviceClient.UpdateReportedPropertiesAsync(_reportedProperties).ConfigureAwait(false);
            _logger.LogWarning("Updated DeviceInfo props");
        }

        public async Task UpdatePropertiesWithDefaultValuesAsync()
        {
            _manufacturer = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"); // (manufacturer)
            _model = Environment.OSVersion.Platform.ToString();// (model)
            _swVersion = Environment.OSVersion.VersionString; ; // (swVersion)
            _osName = Environment.GetEnvironmentVariable("OS"); // (osName)
            _processorArchitecture = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE"); // (processorArchitecture)
            _processorManufacturer = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"); // (processorManufacturer)
            _totalStorage = System.IO.DriveInfo.GetDrives()[0].TotalSize; // (totalStorage) <- try another value!
            _totalMemory = Environment.WorkingSet; // (totalMemory) <- try another value!
            await ReportTwinPropertiesAsync();
        }

        public Task SendTelemetryAsync()
        {
            throw new NotImplementedException();
        }

        public Task RegisterCommandsAsync()
        {
            throw new NotImplementedException();
        }

        public string manufacturer
        {
            get { return _manufacturer; }
            set
            {
                _manufacturer = value;
                _reportedProperties["manufacturer"] = _manufacturer;
            }
        }

        public string model
        {
            get { return _model; }
            set
            {
                _model = value;
                _reportedProperties["model"] = _model;
            }
        }

        public string swVersion
        {
            get { return _swVersion; }
            set
            {
                _swVersion = value;
                _reportedProperties["swVersion"] = _swVersion;
            }
        }

        public string osName
        {
            get { return _osName; }
            set
            {
                _osName = value;
                _reportedProperties["osName"] = _osName;
            }
        }

        public string processorArchitecture
        {
            get { return _processorArchitecture; }
            set
            {
                _processorArchitecture = value;
                _reportedProperties["processorArchitecture"] = _processorArchitecture;
            }
        }

        public string processorManufacturer
        {
            get { return _processorManufacturer; }
            set
            {
                _processorManufacturer = value;
                _reportedProperties["processorManufacturer"] = _processorManufacturer;
            }
        }

        public double totalStorage
        {
            get { return _totalStorage; }
            set
            {
                _totalStorage = value;
                _reportedProperties["totalStorage"] = _totalStorage;
            }
        }

        public double totalMemory
        {
            get { return _totalMemory; }
            set
            {
                _totalMemory = value;
                _reportedProperties["totalMemory"] = _totalMemory;
            }
        }
    }
}