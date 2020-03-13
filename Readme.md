# RigadoS1 PnP Demo

This repo contains a C# sample to demo [Azure IoT PnP](https://docs.microsoft.com/en-us/azure/iot-pnp/overview-iot-plug-and-play) (Plug & Play) features.

>*The materials on this repo are not related to the Rigado company*

This demo is based on the *Public Preview* milestone showing how to connect - and control - a device to Hub (with DPS) and Central.

There are three projects to show different implementation options based on the available SDKs


|Project|Description|SDK|
|:------|:----------|:--|
|[Rigado.S1-PnP-PP](Rigado.S1-PnP-PP)|Device simulator using the Public Preview DigitalTwins SDK.|[DigitalTwin.Client 1.0-preview001](https://www.nuget.org/packages/Microsoft.Azure.Devices.DigitalTwin.Client/1.0.0-preview-001)|
|[Rigado.S1-PnP-GA](Rigado.S1-Central-GA)|Device simulator using the preview DeviceClient SDK (without PnP support) by manually adopting the `$iotin` convention|[Devices.Client 1.29.0-preview004](https://www.nuget.org/packages/Microsoft.Azure.Devices.Client/1.29.0-preview-004)|
|[Rigado.S1-Central-GA](Rigado.S1-Central-GA)|Device simulator targeting Central without using PnP (no auto register)|[Devices.Client 1.23.2](https://www.nuget.org/packages/Microsoft.Azure.Devices.Client/1.23.2)

These three projects are completely functional: Telemetry, Properties and Commands work seamessly with Central and/or Hub. 

The only caveat is in the last project, it's not using the *magic payload* at registration time with DPS, since the current DeviceClient does not handle properly the DPS REST API. Although it's not a real PnP device but can report Telemetry and react to Commands/Properties from Central or Hub.


## S1 Device v1

These samples are based on the existing Rigado S1 device available in the Device Catalog [here](https://catalog.azureiotsolutions.com/details?title=S1-Sensor&source=all-devices-page&deviceId=e88f15ce-226f-4817-b0e0-712498b015da).

The capability model v1 is available in the Public Model Repository: [urn:rigado:S1_Sensor:1](https://repo.azureiotrepository.com/models/urn:rigado:S1_Sensor:1?api-version=2019-07-01-preview&expand=true) and is also copied to this repo [`_models/S1_DCM.json`](_models/S1_DCM.json)

**Telemetry**
- `humidity` double / Units/Humidity/percent
- `temperature` double / Units/Humidity/percent
- `battery` int 

## S1 Device S3

To validate the PnP features The v3 version updates the capability model  adding the next elements to the S1 interface.

**Properties**
- `refreshInterval` is a writable int property to control how often the device send telemetry
- `running` read only boolean property indicating if the device is running

**Commands**
- `start` sync command without input/output params
- `stop` sync command without input/output params


