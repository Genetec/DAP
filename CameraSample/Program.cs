// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Entities.Video;
using Genetec.Sdk.Entities.Video.HardwareSpecific;
using Genetec.Sdk.Entities.Video.HardwareSpecific.Axis;
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
        // Load all cameras into the entity cache
        await LoadCameras(engine);

        // Retrieve all cameras from the entity cache
        IEnumerable<Camera> cameras = engine.GetEntities(EntityType.Camera).OfType<Camera>();

        DisplayCameraInformation(engine, cameras);
    }
    else
    {
        Console.WriteLine($"Logon failed: {state}");
    }

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}

async Task LoadCameras(Engine engine)
{
    var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
    query.EntityTypeFilter.Add(EntityType.Camera);
    await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
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

        DisplayDigitalZoomPresets(camera);

        DisplayScheduledVideoAttributes(camera);

        DisplayCameraRecordingConfiguration(camera);

        DisplayCameraStreams(camera);
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
        if (hardwareCapabilities is AxisCameraCapabilities axisCameraCapabilities)
        {
            Console.WriteLine($"  Supported Image Rotations: {axisCameraCapabilities.SupportedImageRotations}");
            Console.WriteLine($"  Tamper Detection Supported: {axisCameraCapabilities.TamperDetectionSupported}");
        }
        else
        {
            Console.WriteLine("  No hardware capabilities found.");
        }
        Console.WriteLine(new string('-', 30));
    }

    void DisplayHardwareConfiguration(Camera camera)
    {
        Console.WriteLine("Hardware Configuration:");
        HardwareSpecificConfiguration hardwareConfiguration = camera.GetHardwareConfiguration();
        if (hardwareConfiguration is AxisCameraConfiguration axisCameraConfiguration)
        {
            Console.WriteLine($"  Image Rotation: {axisCameraConfiguration.ImageRotation}");
            Console.WriteLine($"  Tamper Duration Threshold: {axisCameraConfiguration.TamperDurationThreshold}");
            Console.WriteLine($"  Dark Image Detection Enabled: {axisCameraConfiguration.DarkImageDetectionEnabled}");
            Console.WriteLine($"  Tamper Detection Enabled: {axisCameraConfiguration.TamperDetectionEnabled}");
        }
        else
        {
            Console.WriteLine("  No hardware configuration found.");
        }
        Console.WriteLine(new string('-', 30));
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
}