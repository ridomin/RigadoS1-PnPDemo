using System;
using System.Collections.Generic;
using System.Text;

namespace AzureIoT.PnP.DeviceInformation
{
    /*
     this.SetManufacturer("Rigado");
            this.SetModel(Environment.OSVersion.Platform.ToString());
            this.SetSoftwareVersion(Environment.OSVersion.VersionString);
            this.SetOperatingSystemName(Environment.GetEnvironmentVariable("OS"));
            this.SetProcessorArchitecture(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE"));
            this.SetProcessorManufacturer(Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"));
            this.SetTotalMemory(Environment.WorkingSet);
            this.SetTotalStorage(System.IO.DriveInfo.GetDrives()[0].TotalSize);
            */
    public class DeviceInfo
    {
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string SoftwareVersion { get; set; }
        public string OperatingSystemName { get; set; }
        public string ProcessorArchitecture { get; set; }
        public string ProcessorManufacturer { get; set; }
        public double TotalMemory{ get; set; }
        public double TotalStorage { get; set; }
    }
}
