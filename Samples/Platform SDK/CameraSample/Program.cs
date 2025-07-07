// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");

using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Entities.Video;
using Genetec.Sdk.Entities.Video.HardwareSpecific;
using Genetec.Sdk.Entities.Video.HardwareSpecific.Axis;
using Genetec.Sdk.Entities.Video.HardwareSpecific.Hanwha;
using Genetec.Sdk.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

SdkResolver.Initialize();

await RunSample();

Console.WriteLine("Press any key to exit...");
Console.ReadKey(true);

async Task RunSample()
{
    // Connection parameters for your Security Center server
    const string server = "localhost";  // Specify the IP address or hostname of your Security Center server.
    const string username = "admin";    // Enter the username for Security Center authentication.
    const string password = "";         // Provide the corresponding password for the specified username.

    using var engine = new Engine();

    engine.LoginManager.LogonStatusChanged += (sender, args) => Console.WriteLine($"Logon status: {args.Status}");
    engine.LoginManager.LogonFailed += (sender, e) => Console.WriteLine($"Logon Failed | Error Message: {e.FormattedErrorMessage} | Error Code: {e.FailureCode}");

    // Set up cancellation support
    using var cancellationTokenSource = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) =>
    {
        Console.WriteLine("Cancelling...");
        e.Cancel = true;
        cancellationTokenSource.Cancel();
    };

    Console.WriteLine($"Logging to {server}... Press Ctrl+C to cancel");

    ConnectionStateCode state = await engine.LoginManager.LogOnAsync(server, username, password, cancellationTokenSource.Token);
    if (state != ConnectionStateCode.Success)
    {
        Console.WriteLine($"logon failed: {state}");
        return;
    }

    // Load all cameras into the entity cache
    await LoadCameras(engine);

    // Retrieve all cameras from the entity cache
    IEnumerable<Camera> cameras = engine.GetEntities(EntityType.Camera).OfType<Camera>();

    DisplayCameraInformation(engine, cameras);
}

async Task LoadCameras(Engine engine)
{
    var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
    query.EntityTypeFilter.Add(EntityType.Camera);
    query.DownloadAllRelatedData = true;
    query.Page = 1;
    query.PageSize = 50;

    QueryCompletedEventArgs args;
    do
    {
        args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
        query.Page++;
    } while (args.Data.Rows.Count >= query.PageSize);
}

void DisplayCameraInformation(Engine engine, IEnumerable<Camera> cameras)
{
    foreach (Camera camera in cameras)
    {
        Console.WriteLine($"Camera: {camera.Name}");
        Console.WriteLine(new string('-', 20));

        Console.WriteLine(camera.IsPtz ? "PTZ Camera" : "Fixed Camera");
        if (camera.IsPtz)
        {
            DisplayPtz(camera);
        }

        DisplayHardwareCapabilities(camera);

        DisplayHardwareConfiguration(camera);

        DisplayCodecConfiguration(camera);

        DisplayDigitalZoomPresets(camera);

        DisplayScheduledVideoAttributes(camera);

        DisplayCameraRecordingConfiguration(camera);

        DisplayCameraStreams(camera);

        DisplayVideoUnit(camera);
    }

    void DisplayScheduledVideoAttributes(Camera camera)
    {
        Console.WriteLine("Scheduled Video Attributes:");
        foreach (VideoAttributes videoAttributes in camera.ScheduledVideoAttributes)
        {
            Console.WriteLine($"  Schedule: {engine.GetEntity(videoAttributes.Schedule).Name}");
            Console.WriteLine($"  Contrast: {videoAttributes.Contrast}");
            Console.WriteLine($"  Saturation: {videoAttributes.Saturation}");
            Console.WriteLine($"  Brightness: {videoAttributes.Brightness}");
            Console.WriteLine(new string('-', 30));
        }
    }

    void DisplayHardwareCapabilities(Camera camera)
    {
        Console.WriteLine("Hardware Capabilities:");

        HardwareSpecificCapabilities hardwareCapabilities = camera.HardwareCapabilities;
        switch (hardwareCapabilities)
        {
            case AxisCameraCapabilities axisCameraCapabilities:
                Console.WriteLine($"  Supported Image Rotations: {axisCameraCapabilities.SupportedImageRotations}");
                Console.WriteLine($"  Tamper Detection Supported: {axisCameraCapabilities.TamperDetectionSupported}");
                break;
            case HanwhaCameraCapabilities hanwhaCameraCapabilities:
                Console.WriteLine($"  WiseStream Supported: {hanwhaCameraCapabilities.WiseStreamSupported}");
                Console.WriteLine($"  WiseStream III Supported: {hanwhaCameraCapabilities.WiseStreamIIISupported}");
                break;
            default:
                Console.WriteLine("  No hardware capabilities found.");
                break;
        }
        Console.WriteLine(new string('-', 30));
    }

    void DisplayHardwareConfiguration(Camera camera)
    {
        Console.WriteLine("Hardware Configuration:");
        HardwareSpecificConfiguration hardwareConfiguration = camera.GetHardwareConfiguration();
        switch (hardwareConfiguration)
        {
            case AxisCameraConfiguration axisCameraConfiguration:
                Console.WriteLine($"  Image Rotation: {axisCameraConfiguration.ImageRotation}");
                Console.WriteLine($"  Tamper Duration Threshold: {axisCameraConfiguration.TamperDurationThreshold}");
                Console.WriteLine($"  Dark Image Detection Enabled: {axisCameraConfiguration.DarkImageDetectionEnabled}");
                Console.WriteLine($"  Tamper Detection Enabled: {axisCameraConfiguration.TamperDetectionEnabled}");
                break;

            case HanwhaCameraConfiguration hanwhaCameraConfiguration:
                Console.WriteLine($"  WiseStream Mode: {hanwhaCameraConfiguration.WiseStreamMode}");
                Console.WriteLine($"  WiseStream III Enabled: {hanwhaCameraConfiguration.WiseStreamIIIEnabled}");
                break;
            default:
                Console.WriteLine("  No hardware configuration found.");
                break;
        }
        Console.WriteLine(new string('-', 30));
    }

    void DisplayCodecConfiguration(Camera camera)
    {
        var config = camera.GetHardwareConfiguration();
        var capabilities = camera.HardwareCapabilities;

        if (config == null || capabilities == null)
        {
            Console.WriteLine("Camera does not support hardware codec configuration.");
            return;
        }

        Console.WriteLine("Current Codec Configuration:");
        for (int i = 0; i < config.CodecConfig.Length; i++)
        {
            Console.WriteLine($"  Stream {i}: {config.CodecConfig[i]}");
        }


        Console.WriteLine();
        Console.WriteLine("Supported Codecs by Stream:");
        CodecChangeCapabilities codecCapabilities = capabilities.CodecChangeCapabilities;

        if (codecCapabilities.CodecCapabilitiesByStream != null)
        {
            for (int i = 0; i < codecCapabilities.CodecCapabilitiesByStream.Length; i++)
            {
                var supported = codecCapabilities.CodecCapabilitiesByStream[i];
                Console.WriteLine($"  Stream {i}: {string.Join(", ", supported)}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("Maximum Streams Per Codec:");
        if (codecCapabilities?.CodecCapabilities != null)
        {
            foreach (var kvp in codecCapabilities.CodecCapabilities)
            {
                Console.WriteLine($"  {kvp.Key}: max {kvp.Value} streams");
            }
        }
    }

    void DisplayPtz(Camera camera)
    {
        Console.WriteLine("PTZ Information:");
        Console.WriteLine($"  PTZ Protocol: {camera.PtzProtocol}");
        Console.WriteLine($"  PTZ Address: {camera.PtzAddress}");

        if (engine.GetEntity(camera.PtzSerialPortId) is SerialDevice serialDevice)
        {
            Console.WriteLine("  Serial Device Details:");
            Console.WriteLine($"    Baud: {serialDevice.Baud}");
            Console.WriteLine($"    DataBit: {serialDevice.DataBit}");
            Console.WriteLine($"    Mode: {serialDevice.Mode}");
            Console.WriteLine($"    Parity: {serialDevice.Parity}");
            Console.WriteLine($"    StopBit: {serialDevice.StopBit}");
        }

        Console.WriteLine("  PTZ Capabilities:");
        DisplayPtzCapabilities((PtzCapabilities)camera.PtzCapabilities);
        Console.WriteLine(new string('-', 30));

        void DisplayPtzCapabilities(PtzCapabilities ptzCapabilities)
        {
            Console.WriteLine("PTZ Capabilities:");
            Console.WriteLine(new string('-', 30));

            // Basic capabilities
            Console.WriteLine($"Number of Presets: {ptzCapabilities.NumberOfPresets}");
            Console.WriteLine($"Number of Patterns: {ptzCapabilities.NumberOfPatterns}");
            Console.WriteLine($"Number of Auxiliaries: {ptzCapabilities.NumberOfAuxiliaries}");
            Console.WriteLine($"Number of Preset Tours: {ptzCapabilities.NumberOfPresetTours}");

            // Base indices
            Console.WriteLine($"Preset Base: {ptzCapabilities.PresetBase}");
            Console.WriteLine($"Pattern Base: {ptzCapabilities.PatternBase}");
            Console.WriteLine($"Auxiliary Base: {ptzCapabilities.AuxiliaryBase}");
            Console.WriteLine($"Preset Tour Base: {ptzCapabilities.PresetTourBase}");

            // Field of View and Focal Length
            if (ptzCapabilities.IsHorizontalFieldOfViewSupported)
                Console.WriteLine($"Horizontal Field of View: {ptzCapabilities.HorizontalFieldOfViewMinimum}° to {ptzCapabilities.HorizontalFieldOfViewMaximum}°");

            if (ptzCapabilities.IsVerticalFieldOfViewSupported)
                Console.WriteLine($"Vertical Field of View: {ptzCapabilities.VerticalFieldOfViewMinimum}° to {ptzCapabilities.VerticalFieldOfViewMaximum}°");

            if (ptzCapabilities.IsFocalLengthSupported)
                Console.WriteLine($"Focal Length: {ptzCapabilities.FocalLengthMinimum}mm to {ptzCapabilities.FocalLengthMaximum}mm");

            // Zoom Factor
            if (ptzCapabilities.IsZoomFactorSupported)
                Console.WriteLine($"Zoom Factor: {ptzCapabilities.ZoomFactorMinimum}X to {ptzCapabilities.ZoomFactorMaximum}X");

            // Spherical Space Capabilities
            if (ptzCapabilities.IsSphericalSpacePanSupported)
                Console.WriteLine($"Spherical Space Pan: {ptzCapabilities.SphericalSpaceMinimumPan}° to {ptzCapabilities.SphericalSpaceMaximumPan}°");

            if (ptzCapabilities.IsSphericalSpaceTiltSupported)
                Console.WriteLine($"Spherical Space Tilt: {ptzCapabilities.SphericalSpaceMinimumTilt}° to {ptzCapabilities.SphericalSpaceMaximumTilt}°");

            // Device Limits
            if (ptzCapabilities.IsSphericalSpacePanDeviceLimitSupported)
                Console.WriteLine($"Pan Device Limits: {ptzCapabilities.SphericalSpacePanDeviceLeftLimit}° to {ptzCapabilities.SphericalSpacePanDeviceRightLimit}°");

            if (ptzCapabilities.IsSphericalSpaceTiltDeviceLimitSupported)
                Console.WriteLine($"Tilt Device Limits: {ptzCapabilities.SphericalSpaceTiltDownDeviceLimit}° to {ptzCapabilities.SphericalSpaceTiltUpDeviceLimit}°");

            // Unknown Space Capabilities
            if (ptzCapabilities.IsUnknownSpacePanSupported)
                Console.WriteLine($"Unknown Space Pan: {ptzCapabilities.UnknownSpaceMinimumPan} to {ptzCapabilities.UnknownSpaceMaximumPan}");

            if (ptzCapabilities.IsUnknownSpaceTiltSupported)
                Console.WriteLine($"Unknown Space Tilt: {ptzCapabilities.UnknownSpaceMinimumTilt} to {ptzCapabilities.UnknownSpaceMaximumTilt}");

            if (ptzCapabilities.IsUnknownSpaceZoomSupported)
                Console.WriteLine($"Unknown Space Zoom: {ptzCapabilities.UnknownSpaceMinimumZoom} to {ptzCapabilities.UnknownSpaceMaximumZoom}");

            Console.WriteLine(new string('-', 30));
        }
    }

    void DisplayDigitalZoomPresets(Camera camera)
    {
        Console.WriteLine("Digital Zoom Presets:");
        Console.WriteLine($"  Total Presets: {camera.DigitalZoomPresets.Count}");
        foreach (IDigitalZoomPreset preset in camera.DigitalZoomPresets)
        {
            Console.WriteLine($"  Preset: {preset.Name}");
            Console.WriteLine($"    Relative View Box: {preset.RelativeViewBox}");
        }
        Console.WriteLine(new string('-', 30));
    }

    void DisplayCameraStreams(Camera camera)
    {
        foreach (VideoStreamUsage streamUsage in camera.StreamUsages)
        {
            var stream = (VideoStream)engine.GetEntity(streamUsage.Stream);

            Console.WriteLine($"Stream Usage: {streamUsage.Usage}");
            Console.WriteLine($"Video Stream: {stream.Name} | Codec: {stream.VideoCompressionAlgorithm}");
            Console.WriteLine(new string('-', 20));

            DisplayVideoConfigurations();
            DisplayQualityBoostConfigurations();

            Console.WriteLine();

            void DisplayVideoConfigurations()
            {
                Console.WriteLine("Video Configuration:");
                foreach (VideoCompressionConfiguration config in stream.VideoCompressions)
                {
                    PrintVideoCompressionConfiguration(config);
                    Console.WriteLine(new string('-', 20));
                }
            }

            void DisplayQualityBoostConfigurations()
            {
                PrintQualityBoostConfiguration("Event Recording Configuration", camera.IsBoostQualityOnEventRecordingEnabled, stream.BoostQualityOnEventRecording);
                PrintQualityBoostConfiguration("Manual Recording Configuration", camera.IsBoostQualityOnManualRecordingEnabled, stream.BoostQualityOnManualRecording);
            }

            void PrintQualityBoostConfiguration(string title, bool isEnabled, VideoCompressionConfiguration config)
            {
                if (isEnabled && config != null)
                {
                    Console.WriteLine($"{title}");
                    Console.WriteLine(new string('-', 20));
                    PrintVideoCompressionConfiguration(config);
                }
            }

            void PrintVideoCompressionConfiguration(VideoCompressionConfiguration config)
            {
                Console.WriteLine($"- BitRate: {config.BitRate} kbps");
                Console.WriteLine($"- FrameRate: {config.FrameRate} fps");
                Console.WriteLine($"- ImageQuality: {config.ImageQuality}");
                Console.WriteLine($"- IsImageCropped: {config.IsImageCropped}");
                Console.WriteLine($"- KeyframeInterval: {config.KeyframeInterval} seconds");
                Console.WriteLine($"- RecordingFrameInterval: {config.RecordingFrameInterval}");
                Console.WriteLine($"- Resolution: {config.ResolutionX}x{config.ResolutionY}");

                if (config.ResolutionXPal > 0 && config.ResolutionYPal > 0)
                {
                    Console.WriteLine($"- PAL Resolution: {config.ResolutionXPal}x{config.ResolutionYPal}");
                }

                Console.WriteLine($"- Schedule: {engine.GetEntity(config.Schedule)?.Name}");
            }
        }
    }

    void DisplayCameraRecordingConfiguration(Camera camera)
    {
        Console.WriteLine("Camera Recording Configuration:");
        Console.WriteLine(new string('-', 40));

        ICameraRecordingConfiguration config = camera.RecordingConfiguration;

        Console.WriteLine($"Audio Recording: {config.AudioRecording}");
        Console.WriteLine($"Automatic Cleanup: {config.AutomaticCleanup}");
        Console.WriteLine($"Default Manual Recording Time: {config.DefaultManualRecordingTime}");
        Console.WriteLine($"Post Event Recording Time: {config.PostEventRecordingTime}");
        Console.WriteLine($"Pre Event Recording Time: {config.PreEventRecordingTime}");
        Console.WriteLine($"Retention Period: {config.RetentionPeriod.TotalDays} days");

        Console.WriteLine("\nScheduled Recording Modes:");
        foreach (ScheduledRecordingMode mode in config.ScheduledRecordingModes)
        {
            Console.WriteLine($"  - Schedule: {engine.GetEntity(mode.Schedule)?.Name}, Mode: {mode.Mode}");
        }

        Console.WriteLine($"\nUse Custom Recording Settings: {config.UseCustomRecordingSettings}");

        Console.WriteLine($"\nRedundant Archiving: {config.RedundantArchiving}");

        Console.WriteLine($"\nEncryption Enabled: {config.EncryptionEnabled}");
        Console.WriteLine($"Encryption Type: {config.EncryptionType}");

        Console.WriteLine(new string('-', 40));
    }

    void DisplayVideoUnit(Camera camera)
    {
        if (engine.GetEntity(camera.Unit) is VideoUnit videoUnit)
        {
            Console.WriteLine($"Video Unit: {videoUnit.Name}");
            Console.WriteLine(new string('-', 40));

            Console.WriteLine("IDENTITY INFORMATION:");
            Console.WriteLine("  Type: Video unit");
            Console.WriteLine($"  Name: {videoUnit.Name}");
            if (!string.IsNullOrEmpty(videoUnit.Description))
                Console.WriteLine($"  Description: {videoUnit.Description}");

            Console.WriteLine($"  Manufacturer: {videoUnit.Manufacturer}");
            Console.WriteLine($"  Product type: {videoUnit.Model}");
            Console.WriteLine($"  MAC address: {videoUnit.MacAddress}");
            Console.WriteLine($"  Firmware version: {videoUnit.HardwareVersion}");

            Console.WriteLine($"  SSL: {(videoUnit.SecureConnection ? "Yes" : "No")}");
            Console.WriteLine(new string('-', 30));

            Console.WriteLine("NETWORK PROPERTIES:");
            Console.WriteLine($"  IP address: {(videoUnit.Dhcp ? "Obtain network settings dynamically (DHCP)" : "Specific settings")}");
            Console.WriteLine($"  Local IP: {videoUnit.IPAddress}");
            Console.WriteLine($"  Subnet mask: {videoUnit.SubnetMask}");
            Console.WriteLine($"  Gateway: {videoUnit.Gateway}");
            Console.WriteLine($"  Command port: {videoUnit.CommandPort}");
            Console.WriteLine($"  Hostname: {videoUnit.Hostname}");

            Console.WriteLine("  Authentication:");
            if (!string.IsNullOrEmpty(videoUnit.Username))
            {
                Console.WriteLine($"    Username: {videoUnit.Username}");
                Console.WriteLine("    Password: ********");
            }
            Console.WriteLine($"    HTTPS: {(videoUnit.SecureConnection ? "Yes" : "No")}");
            Console.WriteLine(new string('-', 30));

            if (videoUnit.HardwareConfiguration != null &&
                videoUnit.HardwareConfiguration.ConfigurationType != HardwareConfigurationType.None)
            {
                Console.WriteLine("HARDWARE CONFIGURATION:");
                Console.WriteLine($"  Type: {videoUnit.HardwareConfiguration.ConfigurationType}");

                if (videoUnit.HardwareConfiguration.CodecConfig != null &&
                    videoUnit.HardwareConfiguration.CodecConfig.Any())
                {
                    Console.WriteLine("  Codec Configurations:");
                    foreach (var codec in videoUnit.HardwareConfiguration.CodecConfig)
                    {
                        Console.WriteLine($"    - {codec}");
                    }
                }
                Console.WriteLine(new string('-', 30));
            }

            Console.WriteLine("PERIPHERALS:");
            DisplayVideoInputs(videoUnit);
            DisplayVideoOutputs(videoUnit);
            DisplayAudioInputs(videoUnit);
            DisplayAudioOutputs(videoUnit);
        }

        void DisplayVideoInputs(VideoUnit unit)
        {
            Console.WriteLine("Video Inputs:");
            var videoInputs = unit.Devices.Select(engine.GetEntity).OfType<VideoUnitInputDevice>();
            foreach (var input in videoInputs)
            {
                Console.WriteLine($"  Input: {input.Name} | Default State: {input.DefaultState}");
            }
            Console.WriteLine(new string('-', 30));
        }

        void DisplayVideoOutputs(VideoUnit unit)
        {
            Console.WriteLine("Video Outputs:");
            var videoOutputs = unit.Devices.Select(engine.GetEntity).OfType<VideoUnitOutputDevice>();
            foreach (var output in videoOutputs)
            {
                Console.WriteLine($"  Output: {output.Name} | Default State: {output.DefaultState}");
            }
            Console.WriteLine(new string('-', 30));
        }

        void DisplayAudioInputs(VideoUnit unit)
        {
            Console.WriteLine("Audio Inputs:");
            var audioInputs = unit.Devices.Select(engine.GetEntity).OfType<AudioInputDevice>();
            foreach (var input in audioInputs)
            {
                Console.WriteLine($"  Input: {input.Name} | Bitrate: {input.Bitrate} | Channels: {input.Channel} | Format: {input.DataFormat} | Sampling Rate: {input.SamplingRate} | Sampling Bits: {input.SampleBits}");
            }
            Console.WriteLine(new string('-', 30));
        }

        void DisplayAudioOutputs(VideoUnit unit)
        {
            Console.WriteLine("Audio Outputs:");
            var audioOutputs = unit.Devices.Select(engine.GetEntity).OfType<AudioOutputDevice>();
            foreach (var output in audioOutputs)
            {
                Console.WriteLine($"  Output: {output.Name} | Volume: {output.Volume}");
            }
            Console.WriteLine(new string('-', 30));
        }
    }
}