using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Threading.Tasks;

namespace Rigado.S1_Central_GA
{
    class DeviceInformation
    {
        DeviceClient _deviceClient;

        public DeviceInformation(DeviceClient deviceClient)
        {
            _deviceClient = deviceClient;
        }

        public async Task UpdatePropertiesAsync()
        {
            TwinCollection reportedProperties = new TwinCollection();
            reportedProperties["manufacturer"] = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"); // (manufacturer)
            reportedProperties["model"] = Environment.OSVersion.Platform.ToString();// (model)
            reportedProperties["swVersion"] = Environment.OSVersion.VersionString; ; // (swVersion)
            reportedProperties["osName"] = Environment.GetEnvironmentVariable("OS"); // (osName)
            reportedProperties["processorArchitecture"] = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE"); // (processorArchitecture)
            reportedProperties["processorManufacturer"] = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"); // (processorManufacturer)
            reportedProperties["totalStorage"] = System.IO.DriveInfo.GetDrives()[0].TotalSize.ToString(); // (totalStorage) <- try another value!
            reportedProperties["totalMemory"] = Environment.WorkingSet; // (totalMemory) <- try another value!
            await _deviceClient.UpdateReportedPropertiesAsync(reportedProperties).ConfigureAwait(false);
            Console.WriteLine("Updated DeviceInfo props");
        }
    }
}