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

            void AddProperty(string name, string value)
            {
                propertyCollection.Add(new DigitalTwinPropertyReport(name, JsonConvert.SerializeObject(value)));
            }

            void AddPropertyD(string name, double value)
            {
                propertyCollection.Add(new DigitalTwinPropertyReport(name, JsonConvert.SerializeObject(value)));
            }


            AddProperty(Manufacturer, di.Manufacturer);
            AddProperty(Model, di.Model);
            AddProperty(SoftwareVersion, di.SoftwareVersion);
            AddProperty(OperatingSystemName, di.OperatingSystemName);
            AddProperty(ProcessorArchitecture, di.ProcessorArchitecture);
            AddProperty(ProcessorManufacturer, di.ProcessorManufacturer);
            AddPropertyD(TotalStorage, di.TotalStorage);
            AddPropertyD(TotalMemory, di.TotalMemory);
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
