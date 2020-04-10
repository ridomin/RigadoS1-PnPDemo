using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Rigado.S1_Central_GA
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "From DTLD")]
    class DeviceInformationPnP
    {
        string _manufacturer = string.Empty;
        string _model = string.Empty;
        string _swVersion = string.Empty;
        string _osName = string.Empty;
        string _processorArchitecture = string.Empty;
        string _processorManufacturer = string.Empty;
        double _totalStorage = 0;
        double _totalMemory = 0;
        readonly DeviceClient _deviceClient;
        readonly TwinCollection _reportedProperties = new TwinCollection();
        readonly ILogger _logger;

        string pnpComponentName = string.Empty;

        public DeviceInformationPnP(DeviceClient deviceClient, string componentName, ILogger logger)
        {
            _deviceClient = deviceClient;
            _logger = logger;
            pnpComponentName = "$iotin:" + componentName;
        }

        public async Task ReadTwinPropertiesAsync()
        {
            var twin = await _deviceClient.GetTwinAsync();
            var reportedProperties = twin.Properties.Reported;
            _logger.LogInformation($"ReportedProperties.Count={reportedProperties.Count}");
            if (reportedProperties.Count > 7 && reportedProperties.Contains(pnpComponentName))
            {
                _manufacturer = Convert.ToString(reportedProperties[pnpComponentName]["manufacturer"]);
                _model = Convert.ToString(reportedProperties[pnpComponentName]["model"]);
                _swVersion = Convert.ToString(reportedProperties[pnpComponentName]["swVersion"]);
                _osName = Convert.ToString(reportedProperties[pnpComponentName]["osName"]);
                _processorArchitecture = Convert.ToString(reportedProperties[pnpComponentName]["processorArchitecture"]);
                _processorManufacturer = Convert.ToString(reportedProperties[pnpComponentName]["processorManufacturer"]);
                _totalStorage = Convert.ToDouble(reportedProperties[pnpComponentName]["totalStorage"].Value);
                _totalMemory = Convert.ToDouble(reportedProperties[pnpComponentName]["totalMemory"].Value);

                _logger.LogWarning($"Reported Properties: {_manufacturer} {_model} {_swVersion} {_osName} {_processorArchitecture} {_processorManufacturer} {_totalStorage} {_totalMemory}");
            }
            else
            {
                _logger.LogWarning("DeviceInfo Properties are not available, usually happens on first device connection.");
            }
        }

        public async Task ReportTwinPropertiesAsync()
        {
            TwinCollection reportedProperties = new TwinCollection();

            reportedProperties[pnpComponentName] = new
            {
                manufacturer = new{ value = _manufacturer},
                model = new { value = _model},
                swVersion = new { value = _swVersion },
                osName = new { value = _osName },
                processorArchitecture = new { value = _processorArchitecture },
                processorManufacturer = new { value = _processorManufacturer },
                totalStorage = new { value = _totalStorage }, 
                totalMemory = new { value = _totalMemory }
            };

            await _deviceClient.UpdateReportedPropertiesAsync(reportedProperties).ConfigureAwait(false);
            Console.WriteLine("Updated DeviceInfo props");
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
            set { _manufacturer = value; }
        }

        public string model
        {
            get { return _model; }
            set { _model = value; }
        }

        public string swVersion
        {
            get { return _swVersion; }
            set { _swVersion = value; }
        }

        public string osName
        {
            get { return _osName; }
            set { _osName = value; }
        }

        public string processorArchitecture
        {
            get { return _processorArchitecture; }
            set {  _processorArchitecture = value; }
        }

        public string processorManufacturer
        {
            get { return _processorManufacturer; }
            set { _processorManufacturer = value; }
        }

        public double totalStorage
        {
            get { return _totalStorage; }
            set { _totalStorage = value; }
        }

        public double totalMemory
        {
            get { return _totalMemory; }
            set { _totalMemory = value; }
        }
    }
}