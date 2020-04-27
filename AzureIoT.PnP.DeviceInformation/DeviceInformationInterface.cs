using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.DigitalTwin.Client;
using Microsoft.Azure.Devices.DigitalTwin.Client.Model;
using Newtonsoft.Json;

namespace AzureIoT.PnP.DeviceInformation
{
    public class DeviceInformationInterface : DigitalTwinInterfaceClient
    {
        private const string DeviceInformationInterfaceId = "urn:azureiot:DeviceManagement:DeviceInformation:1";

        private const string Manufacturer = "manufacturer";
        private const string Model = "model";
        private const string SoftwareVersion = "swVersion";
        private const string OperatingSystemName = "osName";
        private const string ProcessorArchitecture = "processorArchitecture";
        private const string ProcessorManufacturer = "processorManufacturer";
        private const string TotalStorage = "totalStorage";
        private const string TotalMemory = "totalMemory";


        //DeviceInfo devInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceInformationInterface"/> class.
        /// </summary>
        /// <param name="interfaceInstanceName">The instance name of the interface.</param>
        public DeviceInformationInterface(string interfaceInstanceName)
            : base(DeviceInformationInterfaceId, interfaceInstanceName)
        {
        }

        public async Task SendDeviceInfoPropertiesAsync(DeviceInfo di)
        {

            ICollection<DigitalTwinPropertyReport> propertyCollection = new Collection<DigitalTwinPropertyReport>();

            void AddProperty<T>(string name, T value)
            {
                propertyCollection.Add(new DigitalTwinPropertyReport(name, JsonConvert.SerializeObject(value)));
            }

            AddProperty<string>(Manufacturer, di.Manufacturer);
            AddProperty<string>(Model, di.Model);
            AddProperty<string>(SoftwareVersion, di.SoftwareVersion);
            AddProperty<string>(OperatingSystemName, di.OperatingSystemName);
            AddProperty<string>(ProcessorArchitecture, di.ProcessorArchitecture);
            AddProperty<string>(ProcessorManufacturer, di.ProcessorManufacturer);
            AddProperty<double>(TotalStorage, di.TotalStorage);
            AddProperty<double>(TotalMemory, di.TotalMemory);
            await ReportPropertiesAsync(propertyCollection).ConfigureAwait(false);
            Console.WriteLine($"DeviceInformationInterface: sent {propertyCollection.Count} properties.");
            propertyCollection.Clear();
        }

        protected override void OnRegistrationCompleted()
        {
            Console.WriteLine($"DeviceInformationInterface: OnRegistrationCompleted.");
        }
    }
}
