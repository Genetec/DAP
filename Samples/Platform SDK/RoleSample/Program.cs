using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Entities.Roles;
using Genetec.Sdk.Entities.Video;
using Genetec.Sdk.Queries;

const string server = "localhost";
const string username = "admin";
const string password = "";

SdkResolver.Initialize();

await RunSample();

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey(true);

async Task RunSample()
{
    using var engine = new Engine();

    engine.LogonStatusChanged += (_, args) => Console.Write($"\rConnection status: {args.Status}".PadRight(Console.WindowWidth));
    engine.LogonFailed += (_, args) => Console.WriteLine($"\rError: {args.FormattedErrorMessage}".PadRight(Console.WindowWidth));
    engine.LoggedOn += (_, args) => Console.WriteLine($"\rConnected to {args.ServerName}".PadRight(Console.WindowWidth));

    ConnectionStateCode state = await engine.LoginManager.LogOnAsync(server, username, password);
    if (state != ConnectionStateCode.Success)
    {
        Console.WriteLine($"Logon failed: {state}");
        return;
    }

    Console.Write("Loading roles");
    await LoadRoles();

    var roles = engine.GetEntities(EntityType.Role).OfType<Role>();
    Console.WriteLine($"\r{roles.Count()} roles loaded\n");

    foreach (var role in roles)
    {
        DisplayRoleInfo(role);
    }

    async Task LoadRoles()
    {
        var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
        query.EntityTypeFilter.Add(EntityType.Role);
        await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
    }

    void DisplayRoleInfo(Role role)
    {
        Console.WriteLine(new string('-', 50));
        Console.WriteLine($"Name: {role.Name}");
        Console.WriteLine($"Description: {role.Description}");
        Console.WriteLine($"Type: {role.Type}");

        if (role.SubType != Guid.Empty)
            Console.WriteLine($"Subtype: {role.SubType}");

        Console.WriteLine($"Is Online: {role.IsOnline}");
        Console.WriteLine($"Running State: {role.RunningState}");

        Console.WriteLine("Network Binding:");
        Console.WriteLine($"  MAC Address: {role.CurrentNetworkBinding.MacAddress}");
        Console.WriteLine($"  IP address: {role.CurrentNetworkBinding.IpAddress}");

        if (!string.IsNullOrEmpty(role.DatabaseServer))
        {
            Console.WriteLine("Database Information:");
            Console.WriteLine($"  Server: {role.DatabaseServer}");
            Console.WriteLine($"  Name: {role.DatabaseName}");
            Console.WriteLine($"  Encrypted Connections: {role.EnableEncryptionToDatabase}");
            Console.WriteLine($"  Validate Certificate: {!role.TrustServerCertificate}");
        }

        Console.WriteLine("Associated Servers:");
        foreach (var server in role.RoleServers.Select(engine.GetEntity).OfType<Server>())
        {
            DisplayServerInfo(server);
        }

        if (engine.GetEntity(role.CurrentServer) is Server currentServer)
        {
            Console.WriteLine($"Current Server: {currentServer.FullyQualifiedName}");
        }

        DisplaySpecificInfo(role);
    }

    void DisplayServerInfo(Server server)
    {
        Console.WriteLine("  Server Information:");
        Console.WriteLine($"    Name: {server.FullyQualifiedName}");
        Console.WriteLine($"    Main Server: {server.IsMainServer}");
        Console.WriteLine($"    Time Zone: {server.TimeZone}");
        Console.WriteLine($"    Current Time: {server.GetCurrentTime():f}");
        Console.WriteLine($"    Version: {server.Version}");
        Console.WriteLine($"    Certificate Thumbprint: {server.ServerCertificateThumbprint}");
        Console.WriteLine($"    Public IP: {server.PublicEndPoint ?? "Unavailable"}");
        Console.WriteLine($"    Web Port: {server.WebPort}");
        Console.WriteLine($"    Secure Web Port: {server.SecureWebPort}");

        if (server.PrivateEndPoints.Any())
        {
            Console.WriteLine("    Private IP Addresses:");
            foreach (var endpoint in server.PrivateEndPoints)
            {
                Console.WriteLine($"      - {endpoint}");
            }
        }
        else
        {
            Console.WriteLine("    No private IP addresses available.");
        }
    }

    void DisplaySpecificInfo(Role role)
    {
        switch (role)
        {
            case AccessManagerRole accessManagerRole:
                DisplayAccessManagerInfo(accessManagerRole);
                break;
            case AuthenticationServiceRole authenticationServiceRole:
                DisplayAuthenticationServiceInfo(authenticationServiceRole);
                break;
            case ArchiverRole archiverRole:
                DisplayArchiverRoleInfo(archiverRole);
                break;
            case AuxiliaryArchiverRole auxiliaryArchiverRole:
                DisplayAuxiliaryArchiverRoleInfo(auxiliaryArchiverRole);
                break;
            case CloudPlaybackRole cloudPlaybackRole:
                DisplayCloudPlaybackInfo(cloudPlaybackRole);
                break;
            case DirectoryManagerRole directoryManagerRole:
                DisplayDirectoryManagerInfo(directoryManagerRole);
                break;
            case FederationRole federationRole:
                DisplayFederationInfo(federationRole);
                break;
            case LprManagerRole lprManagerRole:
                DisplayLprManagerInfo(lprManagerRole);
                break;
            case RtspMediaRouterRole rtspMediaRouterRole:
                DisplayRtspMediaRouterInfo(rtspMediaRouterRole);
                break;
            case RestRole restRole:
                DisplayRestRoleInfo(restRole);
                break;
        }
    }

    void DisplayAccessManagerInfo(AccessManagerRole role)
    {
        Console.WriteLine($"  Is Peer-to-Peer Enabled: {role.IsPeerToPeerEnabled}");
        if (role.IsPeerToPeerEnabled)
        {
            Console.WriteLine($"  Is Global Antipassback Enabled: {role.IsGlobalAntiPassbackActivated}");
            Console.WriteLine($"  Peer-T-Peer Groups: {role.PeerToPeerGroups.Count()}");
        }
    }

    void DisplayAuthenticationServiceInfo(AuthenticationServiceRole role)
    {
        Console.WriteLine($"  Issuer Name: {role.IssuerName}");
        if (role.Children.Any())
        {
            Console.WriteLine("  Child Roles:");
            foreach (var child in role.Children.Select(engine.GetEntity).OfType<UserGroup>())
            {
                Console.WriteLine($"    Child Role: {child}");
            }
        }
        else
        {
            Console.WriteLine("  No child roles found.");
        }
    }

    void DisplayCloudPlaybackInfo(CloudPlaybackRole role)
    {
        Console.WriteLine($"  RTSP Port: {role.RtspPort}");
    }

    void DisplayDirectoryManagerInfo(DirectoryManagerRole role)
    {
        if (role.DirectoryFailovers.Any())
        {
            Console.WriteLine("  Directory Failovers:");
            foreach (var failover in role.DirectoryFailovers)
            {
                if (engine.GetEntity(failover.GatewayServer) is Server gatewayServer)
                {
                    Console.WriteLine($"    Gateway Server: {gatewayServer.Name}");
                }
                Console.WriteLine($"    Is Disaster Recovery: {failover.IsDisasterRecovery}");
                Console.WriteLine($"    Is Gateway Only: {failover.IsGatewayOnly}");
            }
        }
        else
        {
            Console.WriteLine("  No directory failovers configured.");
        }
    }

    void DisplayFederationInfo(FederationRole role)
    {
        Console.WriteLine($"  Default Stream Type: {role.DefaultStreamType}");
        Console.WriteLine($"  Federate Alarms: {role.FederateAlarms}");
        bool resilientConnectionActivated = role.ResilientConnection.IsResilientConnectionActivated;
        Console.WriteLine($"  Resilient Connection Enabled: {resilientConnectionActivated}");
        if (resilientConnectionActivated)
        {
            Console.WriteLine($"  Resilient Connection Timeout: {role.ResilientConnection.ResiliencyTimeoutInSecondes} seconds");
        }
    }

    void DisplayLprManagerInfo(LprManagerRole role)
    {
        Console.WriteLine("  Permit Associations:");
        foreach (var permit in role.PermitAssociations.Select(engine.GetEntity).OfType<Permit>())
        {
            Console.WriteLine($"    {permit.Name}");
        }

        Console.WriteLine("  Hotlist Associations:");
        foreach (var hotList in role.HotlistAssociations.Select(engine.GetEntity).OfType<HotlistRule>())
        {
            Console.WriteLine($"    {hotList.Name}");
        }
    }

    void DisplayRtspMediaRouterInfo(RtspMediaRouterRole role)
    {
        Console.WriteLine($"  RTSP Port: {role.ListenPort}");
        Console.WriteLine($"  RTSPS Enabled: {role.UseRtsps}");
        Console.WriteLine($"  User Authentication Enabled: {role.NeedsAuthentication}");
        if (role.NeedsAuthentication)
        {
            Console.WriteLine("  Registered Users:");
            foreach (var user in role.RegisteredUsers.Select(engine.GetEntity).OfType<User>())
            {
                Console.WriteLine($"    {user.Name}");
            }
        }
    }

    void DisplayRestRoleInfo(RestRole role)
    {
        Console.WriteLine($"  Application Path: {role.ApplicationPath}");
        Console.WriteLine($"  HTTPS Port: {role.HttpsPort}");
    }

    void DisplayArchiverRoleInfo(ArchiverRole role)
    {
        Console.WriteLine($"\nArchiver Role: {role.Name} ({role.Guid})");
        Console.WriteLine(new string('-', 50));
        Console.WriteLine($"Primary Server: {engine.GetEntity(role.PrimaryServerGuid).Name}");
        Console.WriteLine($"Encryption Type: {role.EncryptionType}");
        Console.WriteLine($"Encryption Supported: {role.IsEncryptionSupported}");

        DisplayNtpConfiguration(role.NtpConfiguration);
        DisplayRecordingConfiguration(role.RecordingConfiguration);
        DisplayCertificates(role.Certificates);
        DisplayArchiveSources(role.ArchiveSources);
    }

    void DisplayAuxiliaryArchiverRoleInfo(AuxiliaryArchiverRole role)
    {
        DisplayAuxRecordingConfiguration(role.RecordingConfiguration);
        DisplayArchiveSources(role.ArchiveSources);
    }

    void DisplayNtpConfiguration(NtpSettings ntpConfig)
    {
        Console.WriteLine($"""
        NTP Configuration:
        Server: {ntpConfig.Server}
        Port: {ntpConfig.Port}
        Poll Timeout: {ntpConfig.PollTimeout}
        """);
    }

    void DisplayAuxRecordingConfiguration(IAuxRecordingConfiguration config)
    {
        Console.WriteLine($"""
        Recording Configuration:
        Audio Recording: {config.AudioRecording}
        Automatic Cleanup: {config.AutomaticCleanup}
        Default Manual Recording Time: {config.DefaultManualRecordingTime}
        Post Event Recording Time: {config.PostEventRecordingTime}
        Pre Event Recording Time: {config.PreEventRecordingTime}
        Retention Period: {config.RetentionPeriod}
        Stream Type: {config.VideoStream}
        """);

        DisplayScheduledRecordingModes(config.ScheduledRecordingModes);
    }

    void DisplayRecordingConfiguration(IArchiverRecordingConfiguration config)
    {
        Console.WriteLine($"""
        Recording Configuration:
        Audio Recording: {config.AudioRecording}
        Automatic Cleanup: {config.AutomaticCleanup}
        Default Manual Recording Time: {config.DefaultManualRecordingTime}
        Post Event Recording Time: {config.PostEventRecordingTime}
        Pre Event Recording Time: {config.PreEventRecordingTime}
        Retention Period: {config.RetentionPeriod}
        Encryption Enabled: {config.EncryptionEnabled}
        Encryption Type: {config.EncryptionType}
        Redundant Archiving: {config.RedundantArchiving}
        """);

        DisplayScheduledRecordingModes(config.ScheduledRecordingModes);
    }

    void DisplayScheduledRecordingModes(IReadOnlyCollection<ScheduledRecordingMode> modes)
    {
        Console.WriteLine("Scheduled Recording Modes:");
        if (!modes.Any())
        {
            Console.WriteLine("Scheduled recording modes available.");
            return;
        }

        foreach (ScheduledRecordingMode mode in modes)
        {
            if (engine.GetEntity(mode.Schedule) is Schedule schedule)
            {
                Console.WriteLine($"  - {mode.Mode} ({schedule.Name})");
            }
        }
    }

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

    void DisplayArchiveSources(ICollection<Guid> archiveSources)
    {
        Console.WriteLine("\nArchive Sources:");
        if (!archiveSources.Any())
        {
            Console.WriteLine("No archive source available.");
            return;
        }

        foreach (var source in archiveSources.Select(engine.GetEntity).OfType<Agent>())
        {
            if (engine.GetEntity(source.RoleId) is Role role)
            {
                Console.WriteLine($"  - {role.Name} ({source.AgentType})");
            }
        }
    }
}
