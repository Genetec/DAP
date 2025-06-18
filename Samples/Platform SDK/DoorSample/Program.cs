using System;
// Licensed under the Apache License, Version 2.0

using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.AccessControl.AccessRules;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Entities.Devices;
using Genetec.Sdk.Queries;

SdkResolver.Initialize();

await RunSample();

async Task RunSample()
{
    const string server = "localhost";
    const string username = "admin";
    const string password = "";

    using var engine = new Engine();

    ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

    if (state == ConnectionStateCode.Success)
    {
        await LoadDoors(engine);
        IEnumerable<Door> doors = engine.GetEntities(EntityType.Door).OfType<Door>();
        DisplayDoorInformation(engine, doors);
    }
    else
    {
        Console.WriteLine($"Logon failed: {state}");
    }

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}

async Task LoadDoors(Engine engine)
{
    var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
    query.EntityTypeFilter.Add(EntityType.Door);
    await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
}

void DisplayDoorInformation(Engine engine, IEnumerable<Door> doors)
{
    foreach (Door door in doors)
    {
        Console.WriteLine($"\n=================== DOOR: {door.Name} ===================\n");

        DisplayBasicInfo();
        DisplayStatus();
        DisplayTimingConfig();
        DisplayAutoUnlock();
        DisplaySchedules();
        DisplayAccessRules();
        DisplayDoorSides();
        DisplayConnections();

        Console.WriteLine("=======================================================\n");

        void DisplayBasicInfo()
        {
            Console.WriteLine("BASIC INFORMATION");
            Console.WriteLine($"  Door ID: {door.Guid}");
            Console.WriteLine($"  Access Permission Level: {door.AccessPermissionLevel}");
            Console.WriteLine($"  Door Lock Device: {GetEntityName(door.DoorLockDevice)}");
            Console.WriteLine($"  Preferred Unit: {GetEntityName(door.PreferredUnit)}");
            Console.WriteLine();
        }

        void DisplayStatus()
        {
            Console.WriteLine("CURRENT STATUS");
            Console.WriteLine($"  Locked: {door.IsLocked}");
            Console.WriteLine($"  Open: {door.IsOpen}");
            Console.WriteLine($"  Maintenance Mode: {door.MaintenanceModeActive}");
            Console.WriteLine($"  Two Person Rule Active: {door.TwoPersonRule?.IsActive}");
            Console.WriteLine();
        }

        void DisplayTimingConfig()
        {
            Console.WriteLine("TIMING CONFIGURATION");
            Console.WriteLine($"  Re-lock Delay: {door.RelockDelayInSeconds} seconds");
            Console.WriteLine($"  Standard Entry Time: {door.StandardEntryTimeInSeconds} seconds");
            Console.WriteLine($"  Extended Entry Time: {door.ExtendedEntryTimeInSeconds} seconds");
            Console.WriteLine($"  Standard Grant Time: {door.StandardGrantTimeInSeconds} seconds");
            Console.WriteLine($"  Extended Grant Time: {door.ExtendedGrantTimeInSeconds} seconds");
            Console.WriteLine($"  Re-lock on Close: {door.RelockOnClose}");
            Console.WriteLine();
        }

        void DisplayAutoUnlock()
        {
            if (door.AutoUnlockOverride != null)
            {
                Console.WriteLine("AUTO UNLOCK OVERRIDE");
                Console.WriteLine($"  Active: {door.AutoUnlockOverride.IsUnlocking}");
                Console.WriteLine($"  Start Time: {door.AutoUnlockOverride.StartTime}");
                Console.WriteLine($"  End Time: {door.AutoUnlockOverride.EndTime}");
                Console.WriteLine();
            }
        }

        void DisplaySchedules()
        {
            Console.WriteLine("SCHEDULES");

            if (door.UnlockSchedules.Any())
            {
                Console.WriteLine("  Unlock Schedules:");
                foreach (var schedule in door.UnlockSchedules.Select(engine.GetEntity).OfType<Schedule>())
                {
                    Console.WriteLine($"    • {schedule.Name}");
                }
            }

            if (door.UnlockExceptionSchedules.Any())
            {
                Console.WriteLine("  Exception Schedules:");
                foreach (var schedule in door.UnlockExceptionSchedules.Select(engine.GetEntity).OfType<Schedule>())
                {
                    Console.WriteLine($"    • {schedule.Name}");
                }
            }
            Console.WriteLine();
        }

        void DisplayAccessRules()
        {
            Console.WriteLine("ACCESS RULES");
            ReadOnlyCollection<AccessRuleRecord> records = door.FetchAccessRulesForSide(AccessRuleSide.None);
            foreach (AccessRuleRecord record in records)
            {
                Console.WriteLine($"  • {GetEntityName(record.Id)} (Side: {record.Side})");
            }
            Console.WriteLine();
        }

        void DisplayDoorSides()
        {
            Console.WriteLine("DOOR SIDE DETAILS");
            DisplayDoorSide("ENTRY SIDE", door.DoorSideIn);
            DisplayDoorSide("EXIT SIDE", door.DoorSideOut);
            Console.WriteLine();

            void DisplayDoorSide(string sideLabel, Door.DoorSide doorSide)
            {
                Console.WriteLine($"  {sideLabel} ({doorSide.AccessPointSide})");

                if (doorSide.Reader != null)
                {
                    Console.WriteLine($"    Reader: {doorSide.Reader.Name}");
                    Console.WriteLine($"      Type: {doorSide.Reader.AccessPointType}");

                    if (doorSide.Reader.Cameras.Any())
                    {
                        Console.WriteLine("      Associated Cameras:");
                        foreach (var cameraGuid in doorSide.Reader.Cameras)
                        {
                            Console.WriteLine($"        • {GetEntityName(cameraGuid)}");
                        }
                    }

                    if (engine.GetEntity(doorSide.Reader.Device) is Reader reader)
                    {
                        DisplayDevice(reader);
                    }
                }

                if (doorSide.Rex != null)
                {
                    Console.WriteLine($"    REX: {doorSide.Rex.Name}");
                    Console.WriteLine($"      Type: {doorSide.Rex.AccessPointType}");

                    if (engine.GetEntity(doorSide.Rex.Device) is Device device)
                    {
                        DisplayDevice(device);
                    }
                }

                if (doorSide.EntrySensor != null)
                {
                    Console.WriteLine($"    Entry Sensor: {doorSide.EntrySensor.Name}");
                    Console.WriteLine($"      Type: {doorSide.EntrySensor.AccessPointType}");

                    if (engine.GetEntity(doorSide.EntrySensor.Device) is Device device)
                    {
                        DisplayDevice(device);
                    }
                }
            }

            void DisplayDevice(Device device)
            {
                Console.WriteLine($"      DEVICE DETAILS: {device.Name}");
                Console.WriteLine($"        Physical Name: {device.PhysicalName}");
                Console.WriteLine($"        Unique ID: {device.UniqueId}");
                Console.WriteLine($"        Online: {device.IsOnline}");
                Console.WriteLine($"        Device Type: {device.DeviceType}");

                if (device.InterfaceModule != Guid.Empty)
                {
                    Console.WriteLine($"        Interface Module: {GetEntityName(device.InterfaceModule)}");
                }

                if (device.Unit != Guid.Empty)
                {
                    Console.WriteLine($"        Unit: {GetEntityName(device.Unit)}");
                }

                switch (device)
                {
                    case InputDevice inputDevice:
                        Console.WriteLine($"        State: {inputDevice.State}");
                        try
                        {
                            DisplayDeviceSettings(inputDevice.InputDeviceSettings);
                        }
                        catch (SdkException ex) when (ex.ErrorCode == SdkError.CannotGetProperty)
                        {
                        }
                        break;

                    case OutputDevice outputDevice:
                        Console.WriteLine($"        State: {outputDevice.State}");
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
                            DisplayReaderInfo(reader);
                            DisplayDeviceSettings(reader.ReaderSettings);
                        }
                        catch (SdkException ex) when (ex.ErrorCode == SdkError.CannotGetProperty)
                        {
                        }
                        break;
                }
                Console.WriteLine();
            }

            void DisplayReaderInfo(Reader reader)
            {
                Console.WriteLine($"        Reader Mode: {reader.CardAndPinMode}");
                Console.WriteLine($"        Card and PIN Timeout: {reader.CardAndPinTimeout}");
                Console.WriteLine($"        Card And PIN Schedule: {GetEntityName(reader.CardAndPinSchedule)}");
            }

            void DisplayDeviceSettings(DeviceSettings settings)
            {
                switch (settings)
                {
                    case SMCInputDeviceSettings smcSettings:
                        DisplaySmcInputSettings(smcSettings);
                        break;

                    case HIDInputDeviceSettings hidSettings:
                        DisplayHidInputSettings(hidSettings);
                        break;

                    case SMCOutputDeviceSettings smcSettings:
                        DisplaySmcOutputSettings(smcSettings);
                        break;

                    case HIDOutputDeviceSettings hidSettings:
                        DisplayHidOutputSettings(hidSettings);
                        break;

                    case SMCReaderSettings smcSettings:
                        DisplaySmcReaderSettings(smcSettings);
                        break;

                    case HIDReaderSettings hidSettings:
                        DisplayHidReaderSettings(hidSettings);
                        break;
                }
            }

            void DisplaySmcInputSettings(SMCInputDeviceSettings settings)
            {
                Console.WriteLine("        SMC Input Settings:");
                try
                {
                    Console.WriteLine($"          Debounce: {settings.Debounce}");
                }
                catch (SdkException ex) when (ex.ErrorCode == SdkError.CannotGetProperty)
                {
                    Console.WriteLine("          Debounce: N/A");
                }
                Console.WriteLine($"          Input Contact Type: {settings.InputContactType}");
                Console.WriteLine($"          Shunted: {settings.Shunted}");
            }

            void DisplayHidInputSettings(HIDInputDeviceSettings settings)
            {
                Console.WriteLine("        HID Input Settings:");
                try
                {
                    Console.WriteLine($"          Debounce: {settings.Debounce}");
                    Console.WriteLine($"          Normal High: {settings.NormalHigh}");
                    Console.WriteLine($"          Normal Low: {settings.NormalLow}");
                    Console.WriteLine($"          Active High: {settings.ActiveHigh}");
                    Console.WriteLine($"          Active Low: {settings.ActiveLow}");
                }
                catch (SdkException ex) when (ex.ErrorCode == SdkError.CannotGetProperty)
                {
                    Console.WriteLine($"          {ex.Message}");
                }
            }

            void DisplaySmcOutputSettings(SMCOutputDeviceSettings settings)
            {
                Console.WriteLine("        SMC Output Settings:");
                Console.WriteLine($"          Output Contact Type: {settings.OutputContactType}");
            }

            void DisplayHidOutputSettings(HIDOutputDeviceSettings settings)
            {
                Console.WriteLine("        HID Output Settings:");
                try
                {
                    Console.WriteLine($"          Minimum Time: {settings.MinimumTime}");
                }
                catch (SdkException ex) when (ex.ErrorCode == SdkError.CannotGetProperty)
                {
                    Console.WriteLine($"          Minimum Time: N/A");
                }
            }

            void DisplaySmcReaderSettings(SMCReaderSettings settings)
            {
                Console.WriteLine("        SMC Reader Settings:");
                Console.WriteLine($"          Keypad Mode: {settings.KeypadMode}");
                Console.WriteLine($"          LED Drive Mode: {settings.LedDriveMode}");

                if (settings.LedDriveMode == LedDriveMode.CustomOSDP)
                {
                    Console.WriteLine($"          OSDP Baud Rate: {settings.OSDPBaudRate}");
                    Console.WriteLine($"          OSDP Tracing: {settings.Tracing}");
                    Console.WriteLine($"          OSDP Smart Card: {settings.SmartCard}");
                    Console.WriteLine($"          OSDP Address: {settings.Address}");
                    Console.WriteLine($"          OSDP Secured: {settings.Secured}");
                }

                Console.WriteLine($"          Shunted: {settings.Shunted}");
                Console.WriteLine($"          Wiegand Pulses: {settings.WiegandPulses}");
                Console.WriteLine($"          Trim Zero Bits: {settings.TrimZeroBits}");
                Console.WriteLine($"          Format to Nibble Array: {settings.FormatToNibbleArray}");
                Console.WriteLine($"          Allow Bi-directional Mag Decode: {settings.AllowBidirectionalMagDecode}");
                Console.WriteLine($"          Allow Northern Mag Decode: {settings.AllowNorthernMagDecode}");
                Console.WriteLine($"          Casi 1-Wire F2F: {settings.Casi1WireF2F}");
                Console.WriteLine($"          Supervised: {settings.Supervised}");
                Console.WriteLine($"          Inputs Come From Reader: {settings.InputsComeFromReader}");
            }

            void DisplayHidReaderSettings(HIDReaderSettings settings)
            {
                Console.WriteLine("        HID Reader Settings:");
                Console.WriteLine($"          Reader Type: {settings.ReaderType}");
            }
        }

        void DisplayConnections()
        {
            Console.WriteLine("CONNECTIONS");
            DisplayConnectionGroup(AccessPointType.Rex, "REX Devices");
            DisplayConnectionGroup(AccessPointType.DoorSensor, "Door Sensors");
            DisplayConnectionGroup(AccessPointType.PullStation, "Pull Stations");
            DisplayConnectionGroup(AccessPointType.DoorLock, "Door Locks");
            DisplayConnectionGroup(AccessPointType.Buzzer, "Buzzers");
            DisplayConnectionGroup(AccessPointType.LockSensor, "Lock Sensors");
            Console.WriteLine();

            void DisplayConnectionGroup(AccessPointType type, string header)
            {
                List<Guid> connections = door.GetConnections(type);
                if (connections.Any())
                {
                    Console.WriteLine($"  {header}:");
                    foreach (var entity in connections.Select(engine.GetEntity))
                    {
                        Console.WriteLine($"    • {entity?.Name}");
                    }
                }
            }
        }
    }

    string GetEntityName(Guid entityId) => engine.GetEntity(entityId)?.Name ?? string.Empty;
}
