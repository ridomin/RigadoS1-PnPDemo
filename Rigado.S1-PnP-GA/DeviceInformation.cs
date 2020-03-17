using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Threading.Tasks;

namespace Rigado.S1_PnP_GA
{
    class DeviceInformation
    {
        string pnpComponentName = string.Empty;
        DeviceClient _deviceClient;

        public DeviceInformation(DeviceClient deviceClient, string componentName)
        {
            _deviceClient = deviceClient;
            pnpComponentName = "$iotin:" + componentName;
        }

        public async Task UpdatePropertiesAsync()
        {
            TwinCollection reportedProperties = new TwinCollection();

            reportedProperties[pnpComponentName] = new
            {
                manufacturer = new 
                { 
                    value = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") 
                },
                model = new
                {
                    value = Environment.OSVersion.Platform.ToString()
                },
                swVersion = new
                {
                    value = Environment.OSVersion.VersionString
                },
                osName = new
                {
                    value = Environment.GetEnvironmentVariable("OS")
                },
                processorArchitecture = new
                {
                    value = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE")
                },
                processorManufacturer = new
                {
                    value = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER")
                },
                totalStorage = new
                {
                    value = System.IO.DriveInfo.GetDrives()[0].TotalSize.ToString()
                },
                totalMemory = new
                {
                    value = Environment.WorkingSet
                }
            };

            await _deviceClient.UpdateReportedPropertiesAsync(reportedProperties).ConfigureAwait(false);
            Console.WriteLine("Updated DeviceInfo props");
        }
    }
}