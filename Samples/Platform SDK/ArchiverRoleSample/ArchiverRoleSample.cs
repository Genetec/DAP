namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Sdk;
using Sdk.Entities;
using Sdk.Entities.Video;
using Sdk.Entities.Video.ArchiverStatistics;

class ArchiverRoleSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        // Load roles into the entity cache
        await LoadEntities(engine, token, EntityType.Role);

        // Retrieve all archiver roles from the entity cache
        List<ArchiverRole> archiverRoles = engine.GetEntities(EntityType.Role).OfType<ArchiverRole>().ToList();

        Console.WriteLine($"Archiver roles loaded: {archiverRoles.Count}");

        // Display information for each archiver role
        foreach (ArchiverRole archiverRole in archiverRoles)
        {
            DisplayArchiverRoleInfo(engine, archiverRole);
        }
    }

    // Displays detailed information about an Archiver Role
    void DisplayArchiverRoleInfo(Engine engine, ArchiverRole archiverRole)
    {
        // Display basic information about the archiver role
        Console.WriteLine($"""

                           Archiver Role: {archiverRole.Name} ({archiverRole.Guid})
                           {new string('-', 50)}
                           Primary Server: {GetEntityName(engine, archiverRole.PrimaryServerGuid)}
                           Encryption Type: {archiverRole.EncryptionType}
                           Encryption Supported: {archiverRole.IsEncryptionSupported}
                           """);

        // Display various configurations and statistics
        DisplayNtpConfiguration(archiverRole.NtpConfiguration);
        DisplayRecordingConfiguration(archiverRole.RecordingConfiguration);

        // Refresh archiver statistics before accessing the ArchiverStatisticsCollection
        engine.ActionManager.RefreshArchiverStatistics(archiverRole.Guid);
        DisplayArchiverStatisticsCollection(archiverRole.ArchiverStatisticsCollection);
        DisplayCertificates(archiverRole.Certificates);
        DisplayArchiveSources(archiverRole.ArchiveSources);

        // Displays NTP configuration for the Archiver Role
        void DisplayNtpConfiguration(NtpSettings ntpConfig)
        {
            Console.WriteLine($"""
                               NTP Configuration:
                               Server: {ntpConfig.Server}
                               Port: {ntpConfig.Port}
                               Poll Timeout: {ntpConfig.PollTimeout}
                               """);
        }

        // Displays recording configuration for the Archiver Role
        void DisplayRecordingConfiguration(IArchiverRecordingConfiguration recordingConfig)
        {
            Console.WriteLine($"""
                               Recording Configuration:
                               Audio Recording: {recordingConfig.AudioRecording}
                               Automatic Cleanup: {recordingConfig.AutomaticCleanup}
                               Default Manual Recording Time: {recordingConfig.DefaultManualRecordingTime}
                               Post Event Recording Time: {recordingConfig.PostEventRecordingTime}
                               Pre Event Recording Time: {recordingConfig.PreEventRecordingTime}
                               Retention Period: {recordingConfig.RetentionPeriod}
                               Encryption Enabled: {recordingConfig.EncryptionEnabled}
                               Encryption Type: {recordingConfig.EncryptionType}
                               Redundant Archiving: {recordingConfig.RedundantArchiving}
                               """);

            // Display scheduled recording modes if any exist
            if (recordingConfig.ScheduledRecordingModes.Any())
            {
                Console.WriteLine("Scheduled Recording Modes:");
                foreach (var mode in recordingConfig.ScheduledRecordingModes)
                {
                    Console.WriteLine($"  - {mode.Mode} {GetEntityName(engine, mode.Schedule)}");
                }
            }
        }

        // Displays archiver statistics collection
        void DisplayArchiverStatisticsCollection(IArchiverStatisticsCollection statisticsCollection)
        {
            Console.WriteLine("\nArchiver Statistics Collection:");
            foreach (IArchiverStatistics stat in statisticsCollection)
            {
                // Display main archiver statistics
                Console.WriteLine($"""
                                   Archiver: {stat.ArchiverName} ({stat.Guid})
                                   Active Cameras: {stat.ActiveCameras}
                                   Archiving Cameras: {stat.ArchivingCameras}
                                   Capture Time: {stat.CaptureTime}
                                   Network Incoming Bitrate: {stat.NetworkIncomingBitrate}
                                   Network Outgoing Bitrate: {stat.NetworkOutgoingBitrate}
                                   Server: {GetEntityName(engine, stat.Server)}
                                   """);

                // Display disk group statistics
                foreach (var diskGroupStat in stat.DiskGroupStats)
                {
                    Console.WriteLine($"""
                                       
                                         Disk Group: {diskGroupStat.Name}
                                         Total Available Space: {FormatBytes(diskGroupStat.TotalAvailableSpace)}
                                         Total Free Space: {FormatBytes(diskGroupStat.TotalFreeSpace)}
                                         Total Space: {FormatBytes(diskGroupStat.TotalSpace)}
                                         Total Used Space: {FormatBytes(diskGroupStat.TotalUsedSpace)}
                                         Total Load Percentage: {diskGroupStat.TotalLoadPercentage:F2}%
                                       """);

                    // Display encoder statistics for each disk group
                    foreach (var encoderStat in diskGroupStat.EncoderStats)
                    {
                        Console.WriteLine($"""

                                           Encoder: {GetEntityName(engine, encoderStat.Guid)}
                                           Archiving Bandwidth: {FormatBitrate(encoderStat.ArchivingBandwidth)}
                                           Audio Receiving Rate: {FormatBitrate(encoderStat.AudioReceivingRate)}
                                           Audio Writing Rate: {FormatBitrate(encoderStat.AudioWritingRate)}
                                           Metadata Receiving Rate: {FormatBitrate(encoderStat.MetadataReceivingRate)}
                                           Metadata Writing Rate: {FormatBitrate(encoderStat.MetadataWritingRate)}
                                           Video Receiving Rate: {FormatBitrate(encoderStat.VideoReceivingRate)}
                                           Video Writing Rate: {FormatBitrate(encoderStat.VideoWritingRate)}
                                           """);
                    }
                }
            }

            // Formats bytes to a human-readable string with appropriate unit
            string FormatBytes(long bytes)
            {
                string[] suffixes = ["B", "KB", "MB", "GB", "TB", "PB"];
                int suffixIndex = 0;
                double size = bytes;

                while (size >= 1024 && suffixIndex < suffixes.Length - 1)
                {
                    size /= 1024;
                    suffixIndex++;
                }

                return $"{size:F2} {suffixes[suffixIndex]}";
            }

            // Formats bitrate to a human-readable string with appropriate unit
            string FormatBitrate(double bitrate)
            {
                string[] suffixes = ["bps", "Kbps", "Mbps", "Gbps"];
                int suffixIndex = 0;

                while (bitrate >= 1000 && suffixIndex < suffixes.Length - 1)
                {
                    bitrate /= 1000;
                    suffixIndex++;
                }

                return $"{bitrate:F2} {suffixes[suffixIndex]}";
            }
        }

        // Displays certificate information
        void DisplayCertificates(IReadOnlyCollection<X509Certificate2> certificates)
        {
            Console.WriteLine("\nCertificates:");
            if (!certificates.Any())
            {
                Console.WriteLine("No certificates available.");
                return;
            }

            foreach (X509Certificate2 cert in certificates)
            {
                Console.WriteLine($"""
                                   Subject: {cert.Subject}
                                   Issuer: {cert.Issuer}
                                   Thumbprint: {cert.Thumbprint}
                                   Serial Number: {cert.SerialNumber}
                                   Valid From: {cert.NotBefore}
                                   Valid Until: {cert.NotAfter}
                                   Has Private Key: {cert.HasPrivateKey}
                                   Public Key Algorithm: {cert.PublicKey.Key.KeyExchangeAlgorithm}
                                   Signature Algorithm: {cert.SignatureAlgorithm.FriendlyName}
                                   Version: {cert.Version}
                                   Friendly Name: {cert.FriendlyName}
                                   Key Usage: {cert.Extensions["2.5.29.15"]}
                                   """);
            }
        }

        // Displays archive sources
        void DisplayArchiveSources(ICollection<Guid> archiveSources)
        {
            Console.WriteLine($"\nArchive Sources: {archiveSources.Count()}");

            foreach (var entityId in archiveSources)
            {
                Console.WriteLine($"  {GetEntityName(engine, entityId)}");
            }
        }
    }

    // Retrieves the name of an entity given its GUID
    string GetEntityName(Engine engine, Guid entity) => engine.GetEntity(entity) is { } entityObj ? entityObj.Name : "Unknown";

    // Loads roles asynchronously
}