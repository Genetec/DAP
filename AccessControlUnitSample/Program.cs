// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Sdk;
    using Sdk.Entities;
    using Sdk.Entities.AccessControl.AccessControlInterfaces.AccessControlInterfaceBaseClasses;
    using Sdk.Entities.Devices;
    using Sdk.Queries;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            const string server = "localhost";
            const string username = "admin";
            const string password = "";

            using var engine = new Engine();

            ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

            if (state == ConnectionStateCode.Success)
            {
                await LoadUnitsAndDevices();

                IEnumerable<Unit> accessControlUnits = engine.GetEntities(EntityType.Unit).OfType<Unit>();

                foreach (Unit accessControlUnit in accessControlUnits)
                {
                    PrintUnitDetails(accessControlUnit);

                    IEnumerable<InterfaceModule> interfaceModules = accessControlUnit.InterfaceModules.Select(engine.GetEntity).OfType<InterfaceModule>();
                    foreach (InterfaceModule interfaceModule in interfaceModules)
                    {
                        PrintInterfaceModule(interfaceModule, "  ");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            async Task LoadUnitsAndDevices()
            {
                var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
                query.EntityTypeFilter.Add(EntityType.Role);
                query.EntityTypeFilter.Add(EntityType.Unit);
                query.EntityTypeFilter.Add(EntityType.Device);
                query.DownloadAllRelatedData = true;
                query.Page = 1;
                query.PageSize = 1000;

                QueryCompletedEventArgs args;
                do
                {
                    args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
                    query.Page++;
                } while (args.Error == ReportError.TooManyResults || args.Data.Rows.Count > query.PageSize);
            }

            void PrintInterfaceModule(InterfaceModule interfaceModule, string indent)
            {
                Console.WriteLine($"{indent}Interface Module:");
                Console.WriteLine($"{indent}  Name: {interfaceModule.Name}");
                Console.WriteLine($"{indent}  State: {interfaceModule.RunningState}");
                Console.WriteLine($"{indent}  Description: {interfaceModule.Description}");

                try
                {
                    AccessControlInterfaceBase interfaceBase = interfaceModule.GetAccessControlInterface();
                    Console.WriteLine($"{indent}  Type: {interfaceBase.GetType().Name}");
                }
                catch (SdkException ex) when (ex.ErrorCode == SdkError.InvalidOperation)
                {
                    Console.WriteLine($"{indent}  Type: Unavailable");
                }

                List<Device> devices = interfaceModule.Devices.Select(engine.GetEntity).OfType<Device>().ToList();

                PrintDevices(indent, "Readers", devices.Where(device => device.DeviceType == DeviceType.Reader));
                PrintDevices(indent, "Input devices", devices.Where(device => device.DeviceType == DeviceType.Input));
                PrintDevices(indent, "Output devices", devices.Where(device => device.DeviceType == DeviceType.Output));

                foreach (InterfaceModule childInterface in interfaceModule.ChildrenInterfaceModule.Select(engine.GetEntity).OfType<InterfaceModule>())
                {
                    PrintInterfaceModule(childInterface, $"{indent}  ");
                }
            }

            void PrintDevices(string indent, string deviceType, IEnumerable<Device> devices)
            {
                Console.WriteLine($"{indent}{deviceType}:");
                foreach (Device device in devices)
                {
                    Console.WriteLine($"{indent}  Name: {device.Name}");
                    Console.WriteLine($"{indent}  Physical name: {device.PhysicalName}");
                    Console.WriteLine($"{indent}  Online: {device.IsOnline}");

                    switch (device)
                    {
                        case InputDevice inputDevice:
                            Console.WriteLine($"{indent}  State: {inputDevice.State}");
                            try
                            {
                                DisplayDeviceSettings(inputDevice.InputDeviceSettings);
                            }
                            catch (SdkException ex) when (ex.ErrorCode == SdkError.CannotGetProperty)
                            {
                            }
                            break;

                        case OutputDevice outputDevice:
                            Console.WriteLine($"{indent}  State: {outputDevice.State}");
                            try
                            {
                                DisplayDeviceSettings(outputDevice.OutputDeviceSettings);
                            }
                            catch (SdkException ex) when (ex.ErrorCode == SdkError.CannotGetProperty)
                            {
                            }
                            break;

                        case Reader reader:
                            try
                            {
                                DisplayDeviceSettings(reader.ReaderSettings);
                            }
                            catch (SdkException ex) when (ex.ErrorCode == SdkError.CannotGetProperty)
                            {
                            }
                            break;
                    }

                    Console.WriteLine();
                }
            }

            void DisplayDeviceSettings(object settings)
            {
                switch (settings)
                {
                    case SMCInputDeviceSettings smcInputSettings:
                        DisplaySmcInputDeviceSettings(smcInputSettings);
                        break;

                    case HIDInputDeviceSettings hidInputSettings:
                        DisplayHIDInputDeviceSettings(hidInputSettings);
                        break;

                    case SMCOutputDeviceSettings smcOutputSettings:
                        DisplaySmcOutputDeviceSettings(smcOutputSettings);
                        break;

                    case HIDOutputDeviceSettings hidOutputSettings:
                        DisplayHIDOutputDeviceSettings(hidOutputSettings);
                        break;

                    case SMCReaderSettings smcReaderSettings:
                        DisplaySmcReaderSettings(smcReaderSettings);
                        break;

                    case HIDReaderSettings hidReaderSettings:
                        DisplayHIDReaderSettings(hidReaderSettings);
                        break;
                }
            }

            void PrintUnitDetails(Unit accessControlUnit)
            {
                Console.WriteLine("Unit:");
                Console.WriteLine($"  Name: {accessControlUnit.Name}");
                Console.WriteLine($"  Access Manager: {accessControlUnit.AccessManagerRole.Name}");
                Console.WriteLine($"  State: {accessControlUnit.RunningState}");
                Console.WriteLine($"  MAC address: {accessControlUnit.MacAddress}");
                Console.WriteLine($"  IP address: {accessControlUnit.IPAddress}");
                Console.WriteLine($"  Time zone: {accessControlUnit.TimeZone}");
                Console.WriteLine($"  Latitude: {accessControlUnit.GeographicalLocation.Latitude}");
                Console.WriteLine($"  Longitude: {accessControlUnit.GeographicalLocation.Longitude}");
                Console.WriteLine($"  Serial number: {accessControlUnit.SerialNumber}");
                Console.WriteLine($"  Firmware version: {accessControlUnit.FirmwareVersion}");

                try
                {
                    Console.WriteLine($"  Type: {accessControlUnit.UnitExtensionType}");
                }
                catch (SdkException ex) when (ex.ErrorCode == SdkError.InvalidValue)
                {
                    Console.WriteLine("  Type: N/A");
                }
            }

            void DisplaySmcReaderSettings(SMCReaderSettings settings)
            {
                Console.WriteLine("  Reader Settings (SMC):");
                Console.WriteLine($"    Keypad mode: {settings.KeypadMode}");
                Console.WriteLine($"    Led drive mode: {settings.LedDriveMode}");

                if (settings.LedDriveMode == LedDriveMode.CustomOSDP)
                {
                    Console.WriteLine($"    OSDP baud rate: {settings.OSDPBaudRate}");
                    Console.WriteLine($"    OSDP Tracing: {settings.Tracing}");
                    Console.WriteLine($"    OSDP Smart Card: {settings.SmartCard}");
                    Console.WriteLine($"    OSDP Address: {settings.Address}");
                    Console.WriteLine($"    OSDP Secured: {settings.Secured}");
                }

                Console.WriteLine($"    Shunted: {settings.Shunted}");
                Console.WriteLine($"    Wiegand pulses: {settings.WiegandPulses}");
                Console.WriteLine($"    Trim zero bits: {settings.TrimZeroBits}");
                Console.WriteLine($"    Format to nibble array: {settings.FormatToNibbleArray}");
                Console.WriteLine($"    Allow bi-directional mag decode: {settings.AllowBidirectionalMagDecode}");
                Console.WriteLine($"    Allow northern mag decode: {settings.AllowNorthernMagDecode}");
                Console.WriteLine($"    Casi 1-Wire F2F: {settings.Casi1WireF2F}");
                Console.WriteLine($"    Supervised: {settings.Supervised}");
                Console.WriteLine($"    Inputs come from reader: {settings.InputsComeFromReader}");
            }

            void DisplaySmcOutputDeviceSettings(SMCOutputDeviceSettings settings)
            {
                Console.WriteLine("  Output Device Settings (SMC):");
                Console.WriteLine($"    Output contact type: {settings.OutputContactType}");
            }

            void DisplaySmcInputDeviceSettings(SMCInputDeviceSettings settings)
            {
                Console.WriteLine("  Input Device Settings (SMC):");
                try
                {
                    Console.WriteLine($"    Debounce: {settings.Debounce}");
                }
                catch (SdkException ex) when (ex.ErrorCode == SdkError.CannotGetProperty)
                {
                    Console.WriteLine("    Debounce: N/A");
                }

                Console.WriteLine($"    Input contact type: {settings.InputContactType}");
                Console.WriteLine($"    Shunted: {settings.Shunted}");
            }

            void DisplayHIDReaderSettings(HIDReaderSettings settings)
            {
                Console.WriteLine("  Reader Settings (HID):");
                Console.WriteLine($"    Reader type: {settings.ReaderType}");
            }

            void DisplayHIDOutputDeviceSettings(HIDOutputDeviceSettings settings)
            {
                Console.WriteLine("  Output Device Settings (HID):");
                try
                {
                    Console.WriteLine($"    Minimum time: {settings.MinimumTime}");
                }
                catch (SdkException ex) when (ex.ErrorCode == SdkError.CannotGetProperty)
                {
                    Console.WriteLine($"    Minimum time: {ex.Message}");
                }
            }

            void DisplayHIDInputDeviceSettings(HIDInputDeviceSettings settings)
            {
                Console.WriteLine("  Input Device Settings (HID):");
                try
                {
                    Console.WriteLine($"    Debounce: {settings.Debounce}");
                }
                catch (SdkException ex) when (ex.ErrorCode == SdkError.CannotGetProperty)
                {
                    Console.WriteLine("    Debounce: N/A");
                }

                try
                {
                    Console.WriteLine($"    Normal high: {settings.NormalHigh}");
                }
                catch (SdkException ex) when (ex.ErrorCode == SdkError.CannotGetProperty)
                {
                    Console.WriteLine("    Normal high: N/A");
                }

                try
                {
                    Console.WriteLine($"    Normal low: {settings.NormalLow}");
                }
                catch (SdkException ex) when (ex.ErrorCode == SdkError.CannotGetProperty)
                {
                    Console.WriteLine("    Normal low: N/A");
                }

                try
                {
                    Console.WriteLine($"    Active high: {settings.ActiveHigh}");
                }
                catch (SdkException ex) when (ex.ErrorCode == SdkError.CannotGetProperty)
                {
                    Console.WriteLine("    Active high: N/A");
                }

                try
                {
                    Console.WriteLine($"    Active low: {settings.ActiveLow}");
                }
                catch (SdkException ex) when (ex.ErrorCode == SdkError.CannotGetProperty)
                {
                    Console.WriteLine("    Active low: N/A");
                }
            }
        }
    }
}