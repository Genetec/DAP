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
    using Genetec.Sdk.Entities;
    using Sdk;
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
                await LoadUnitsAndDevices(engine);
                var accessControlUnits = engine.GetEntities(EntityType.Unit).OfType<Unit>();

                foreach (var accessControlUnit in accessControlUnits)
                {
                    try
                    {
                        PrintUnitDetails(accessControlUnit);
                        foreach (var interfaceModule in accessControlUnit.InterfaceModules.Select(engine.GetEntity).OfType<InterfaceModule>())
                        {
                            PrintInterfaceModule(interfaceModule, "  ");
                        }
                    }
                    catch (SdkException ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            void PrintInterfaceModule(InterfaceModule interfaceModule, string indent)
            {
                Console.WriteLine($"{indent}Interface Module:");
                Console.WriteLine($"{indent}  Name: {interfaceModule.Name}");
                Console.WriteLine($"{indent}  State: {interfaceModule.RunningState}");
                Console.WriteLine($"{indent}  Description: {interfaceModule.Description}");

                var devices = interfaceModule.Devices.Select(engine.GetEntity).OfType<Device>().ToList();
               
                PrintDevices(indent, "Readers", devices.Where(device => device.DeviceType == DeviceType.Reader));
                PrintDevices(indent, "Input Devices", devices.Where(device => device.DeviceType == DeviceType.Input));
                PrintDevices(indent, "Output Devices", devices.Where(device => device.DeviceType == DeviceType.Output));

                var childInterfaces = interfaceModule.ChildrenInterfaceModule.Select(engine.GetEntity).OfType<InterfaceModule>();
                foreach (var childInterface in childInterfaces)
                {
                    PrintInterfaceModule(childInterface, indent + "  ");
                }
            }

            void PrintDevices(string indent, string deviceType, IEnumerable<Device> devices)
            {
                Console.WriteLine($"{indent}  {deviceType}:");
                foreach (var device in devices)
                {
                    Console.WriteLine($"{indent}    Name: {device.Name}");
                    Console.WriteLine($"{indent}    PhysicalName: {device.PhysicalName}");
                    Console.WriteLine($"{indent}    IsOnline: {device.IsOnline}");

                    if (device is InputDevice inputDevice)
                    {
                        Console.WriteLine($"{indent}    State: {inputDevice.State}");
                    }

                    Console.WriteLine();
                }
            }

            void PrintUnitDetails(Unit accessControlUnit)
            {
                Console.WriteLine("Unit:");
                Console.WriteLine($"  Name: {accessControlUnit.Name}");
                Console.WriteLine($"  State: {accessControlUnit.RunningState}");
                Console.WriteLine($"  MacAddress: {accessControlUnit.MacAddress}");
                Console.WriteLine($"  IPAddress: {accessControlUnit.IPAddress}");
                Console.WriteLine($"  TimeZone: {accessControlUnit.TimeZone}");
                Console.WriteLine($"  Latitude: {accessControlUnit.GeographicalLocation.Latitude}");
                Console.WriteLine($"  Longitude: {accessControlUnit.GeographicalLocation.Longitude}");
                Console.WriteLine($"  SerialNumber: {accessControlUnit.SerialNumber}");
                Console.WriteLine($"  FirmwareVersion: {accessControlUnit.FirmwareVersion}");
                Console.WriteLine($"  Type: {accessControlUnit.UnitExtensionType}");
            }
        }

        static async Task LoadUnitsAndDevices(IEngine engine)
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

    }
}
