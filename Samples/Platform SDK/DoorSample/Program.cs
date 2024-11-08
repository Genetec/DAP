using System;
using System.Collections.Generic;
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
        Console.WriteLine($"\nDoor Name: {door.Name}");
        Console.WriteLine($"Door ID: {door.Guid}");
        Console.WriteLine($"Access Permission Level: {door.AccessPermissionLevel}");
        Console.WriteLine($"Is Locked: {door.IsLocked}");
        Console.WriteLine($"Is Open: {door.IsOpen}");
        Console.WriteLine($"Door Lock Device: {GetEntityName(door.DoorLockDevice)}");
        Console.WriteLine($"Auto Unlock Override Active: {door.AutoUnlockOverride?.IsUnlocking}");
        Console.WriteLine($"Auto Unlock Override Start Time: {door.AutoUnlockOverride?.StartTime}");
        Console.WriteLine($"Auto Unlock Override End Time: {door.AutoUnlockOverride?.EndTime}");
        Console.WriteLine($"Re-lock Delay in Seconds: {door.RelockDelayInSeconds}");
        Console.WriteLine($"Extended Entry Time in Seconds: {door.ExtendedEntryTimeInSeconds}");
        Console.WriteLine($"Extended Grant Time in Seconds: {door.ExtendedGrantTimeInSeconds}");
        Console.WriteLine($"Standard Entry Time in Seconds: {door.StandardEntryTimeInSeconds}");
        Console.WriteLine($"Standard Grant Time in Seconds: {door.StandardGrantTimeInSeconds}");
        Console.WriteLine($"Re-lock on Close: {door.RelockOnClose}");
        Console.WriteLine($"Maintenance Mode Active: {door.MaintenanceModeActive}");
        Console.WriteLine($"Ignore Access Events When Unlocked By Schedule: {door.IgnoreAccessEventsWhenUnlockedBySchedule}");
        Console.WriteLine($"Ignore Door Open Too Long Event When Unlocked By Schedule: {door.IgnoreDoorOpenTooLongEventWhenUnlockedBySchedule}");
        Console.WriteLine($"Preferred Unit: {GetEntityName(door.PreferredUnit)}");
        Console.WriteLine($"Preferred Interface: {GetEntityName(door.PreferredInterface)}");
        Console.WriteLine($"Two Person Rule Active: {door.TwoPersonRule?.IsActive}");
        Console.WriteLine($"Door Forced Active: {door.DoorForced?.IsActive}");
        Console.WriteLine($"Door Forced Buzzer Behavior: {door.DoorForced?.BuzzerBehavior}");
        Console.WriteLine($"Door Held Active: {door.DoorHeld?.IsActive}");
        Console.WriteLine($"Door Held Buzzer Behavior: {door.DoorHeld?.BuzzerBehavior}");
        Console.WriteLine($"Visitor Escort Maximum Delay: {door.VisitorEscort?.MaximumDelayBetweenCardPresentations}");

        DisplayDoorSideDetails(door.DoorSideIn);

        DisplayDoorSideDetails(door.DoorSideOut);

        DisplayAccessRules();

        DisplaySchedules();

        DisplayConnections();

        Console.WriteLine(new string('-', 20));

        void DisplayAccessRules()
        {
            var records = door.FetchAccessRulesForSide(AccessRuleSide.None);
            foreach (var record in records)
            {
                Console.WriteLine($"Access Rule: {GetEntityName(record.Id)} Side: {record.Side}");
            }
        }

        void DisplaySchedules()
        {
            if (door.UnlockSchedules.Any())
            {
                foreach (var schedule in door.UnlockSchedules.Select(engine.GetEntity).OfType<Schedule>())
                {
                    Console.WriteLine($"Unlock Schedule: {schedule.Name}");
                }
            }

            if (door.UnlockExceptionSchedules.Any())
            {
                foreach (var schedule in door.UnlockExceptionSchedules.Select(engine.GetEntity).OfType<Schedule>())
                {
                    Console.WriteLine($"Unlock Exception Schedule: {schedule.Name}");
                }
            }
        }

        void DisplayConnections()
        {
            DisplayConnectionType(AccessPointType.Rex, "Rex Connections");
            DisplayConnectionType(AccessPointType.DoorSensor, "Door Sensor Connections");
            DisplayConnectionType(AccessPointType.PullStation, "Pull Station Connections");
            DisplayConnectionType(AccessPointType.DoorLock, "Door Lock Connections");
            DisplayConnectionType(AccessPointType.Buzzer, "Buzzer Connections");
            DisplayConnectionType(AccessPointType.LockSensor, "Lock Sensor Connections");

            void DisplayConnectionType(AccessPointType type, string header)
            {
                var connections = door.GetConnections(type);
                if (connections.Any())
                {
                    Console.WriteLine($"{header}:");
                    foreach (var entity in connections.Select(engine.GetEntity))
                    {
                        Console.WriteLine($"    Name: {entity?.Name}");
                    }
                }
            }
        }

        void DisplayDoorSideDetails(Door.DoorSide doorSide)
        {
            Console.WriteLine($"    Access Point Side: {doorSide.AccessPointSide}");

            if (doorSide.Reader != null)
            {
                Console.WriteLine($"    Reader Access Point: {doorSide.Reader.Name}");
                Console.WriteLine($"        Access Point Type: {doorSide.Reader.AccessPointType}");

                Console.WriteLine("        Cameras Associated with Reader:");
                foreach (var cameraGuid in doorSide.Reader.Cameras)
                {
                    Console.WriteLine($"            Camera: {GetEntityName(cameraGuid)}");
                }

                if (engine.GetEntity(doorSide.Reader.Device) is Reader reader)
                {
                    DisplayDeviceInformation(reader);
                }
            }

            if (doorSide.Rex != null)
            {
                Console.WriteLine($"    Rex Access Point: {doorSide.Rex.Name}");
                Console.WriteLine($"        Access Point Type: {doorSide.Rex.AccessPointType}");

                if (engine.GetEntity(doorSide.Rex.Device) is Device device)
                {
                    DisplayDeviceInformation(device);
                }
            }

            if (doorSide.EntrySensor != null)
            {
                Console.WriteLine($"    Entry Sensor Access Point: {doorSide.EntrySensor.Name}");
                Console.WriteLine($"        Access Point Type: {doorSide.EntrySensor.AccessPointType}");

                if (engine.GetEntity(doorSide.EntrySensor.Device) is Device device)
                {
                    DisplayDeviceInformation(device);
                }
            }
        }

        void DisplayDeviceInformation(Device device)
        {
            Console.WriteLine($"Name: {device.Name}");
            Console.WriteLine($"Physical name: {device.PhysicalName}");
            Console.WriteLine($"Unique ID: {device.UniqueId}");
            Console.WriteLine($"Online: {device.IsOnline}");
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
                    Console.WriteLine($"State: {inputDevice.State}");
                    try
                    {
                        DisplayDeviceSettings(inputDevice.InputDeviceSettings);
                    }
                    catch (SdkException ex) when (ex.ErrorCode == SdkError.CannotGetProperty)
                    {
                    }
                    break;

                case OutputDevice outputDevice:
                    Console.WriteLine($"State: {outputDevice.State}");
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
                        DisplayReaderInformation(reader);
                        DisplayDeviceSettings(reader.ReaderSettings);
                    }
                    catch (SdkException ex) when (ex.ErrorCode == SdkError.CannotGetProperty)
                    {
                    }
                    break;
            }

            Console.WriteLine();

            void DisplayReaderInformation(Reader reader)
            {
                Console.WriteLine($"        Reader Mode: {reader.CardAndPinMode}");
                Console.WriteLine($"        Card and PIN Timeout: {reader.CardAndPinTimeout}");
                Console.WriteLine($"        Card And PIN Schedule: {GetEntityName(reader.CardAndPinSchedule)}");
            }

            void DisplayDeviceSettings(DeviceSettings settings)
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

    string GetEntityName(Guid entityId) => engine.GetEntity(entityId)?.Name ?? string.Empty;
}