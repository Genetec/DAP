namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Genetec.Sdk.AccessControl.Credentials.CardCredentials;
    using Genetec.Sdk;
    using Genetec.Sdk.Credentials;
    using Genetec.Sdk.Entities;
    using Genetec.Sdk.Entities.Builders;
    using Genetec.Sdk.Queries;

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
                DisplayCredentialFormats();

                ICredentialBuilder credentialBuilder = engine.EntityManager.GetCredentialBuilder();

                await BuildAndDisplayCredential(credentialBuilder, "Standard Wiegand Credential", new WiegandStandardCredentialFormat(facility: 1, cardId: 2));
                await BuildAndDisplayCredential(credentialBuilder, "H10306 Wiegand Credential", new WiegandH10306CredentialFormat(facility: 1, cardId: 2));
                await BuildAndDisplayCredential(credentialBuilder, "H10304 Wiegand Credential", new WiegandH10304CredentialFormat(facility: 1, cardId: 2));
                await BuildAndDisplayCredential(credentialBuilder, "H10302 Wiegand Credential", new WiegandH10302CredentialFormat(cardId: 1));
                await BuildAndDisplayCredential(credentialBuilder, "CSN32 Wiegand Credential", new WiegandCsn32CredentialFormat(cardId: 1));
                await BuildAndDisplayCredential(credentialBuilder, "48-Bit Corporate 1000 Wiegand Credential", new Wiegand48BitCorporate1000CredentialFormat(companyId: 1, cardId: 2));
                await BuildAndDisplayCredential(credentialBuilder, "Corporate 1000 Wiegand Credential", new WiegandCorporate1000CredentialFormat(companyId: 1, cardId: 2));
                await BuildAndDisplayCredential(credentialBuilder, "License Plate Credential", new LicensePlateCredentialFormat(licensePlate: "12345"));
                await BuildAndDisplayCredential(credentialBuilder, "Keypad Credential", new KeypadCredentialFormat(credentialCode: 12345));
                await BuildAndDisplayCredential(credentialBuilder, "Raw Card Credential", new RawCardCredentialFormat(rawData: "1234", bitLength: 32));

                var fascN75Dict = new Dictionary<string, string>
                {
                    { FascN75BitCardCredentialFormat.AGENCY_CODE_FIELD_NAME, "16383" },
                    { FascN75BitCardCredentialFormat.SYSTEM_CODE_FIELD_NAME, "16383" },
                    { FascN75BitCardCredentialFormat.CREDENTIAL_NUMBER_FIELD_NAME, "1234" },
                    { FascN75BitCardCredentialFormat.EXP_DATE_FIELD_NAME, "9999" }
                };

                await BuildAndDisplayCredential(credentialBuilder, "FascN 75-Bit Card Credential", new FascN75BitCardCredentialFormat(fascN75Dict));

                var fascN200Dict = new Dictionary<string, string>
                {
                    { FascN200BitCardCredentialFormat.AGENCY_CODE_FIELD_NAME, "9999" },
                    { FascN200BitCardCredentialFormat.SYSTEM_CODE_FIELD_NAME, "9999" },
                    { FascN200BitCardCredentialFormat.CREDENTIAL_NUMBER_FIELD_NAME, "0" },
                    { FascN200BitCardCredentialFormat.CS_FIELD_NAME, "0" },
                    { FascN200BitCardCredentialFormat.ICI_FIELD_NAME, "0" },
                    { FascN200BitCardCredentialFormat.PI_FIELD_NAME, "0" },
                    { FascN200BitCardCredentialFormat.OC_FIELD_NAME, "0" },
                    { FascN200BitCardCredentialFormat.OI_FIELD_NAME, "0" },
                    { FascN200BitCardCredentialFormat.POA_FIELD_NAME, "0" },
                    { FascN200BitCardCredentialFormat.LRC_FIELD_NAME, "F" }
                };

                await BuildAndDisplayCredential(credentialBuilder, "FascN 200-Bit Card Credential", new FascN200BitCardCredentialFormat(fascN200Dict));
            }
            else
            {
                Console.WriteLine("Logon failed.");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            async Task BuildAndDisplayCredential(ICredentialBuilder credentialBuilder, string name, CredentialFormat format)
            {
                Console.WriteLine($"Creating credential with format: {format.UniqueId}.");

                Credential credential = await TryGetCredentialByFormat();

                if (credential != null)
                {
                    Console.WriteLine($"Credential with format {format.UniqueId} already exists.");
                }
                else
                {
                    credentialBuilder.SetName(name);
                    credentialBuilder.SetFormat(format);
                    credential = credentialBuilder.Build();
                }

                DisplayCredentialDetails(credential);

                async Task<Credential> TryGetCredentialByFormat()
                {
                    var query = (CredentialConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.CredentialConfiguration);
                    query.UniqueIds.Add(format);
                    QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
                    return args.Data.AsEnumerable().Select(row => engine.GetEntity(row.Field<Guid>(nameof(Guid)))).OfType<Credential>().FirstOrDefault();
                }
            }

            void DisplayCredentialFormats()
            {
                var config = (SystemConfiguration)engine.GetEntity(entityId: SystemConfiguration.SystemConfigurationGuid);
                foreach (CredentialFormat format in config.CredentialFormats)
                {
                    Console.WriteLine("Credential Format Details:");
                    Console.WriteLine("---------------------------");
                    Console.WriteLine($"Type Name       : {format.GetType().Name}");
                    Console.WriteLine($"Name            : {format.Name}");
                    Console.WriteLine($"Type            : {format.Type}");
                    Console.WriteLine($"Bit Length      : {format.BitLength}");
                    Console.WriteLine($"Custom Format   : {format.IsCustomFormat}");
                    Console.WriteLine();
                }
            }

            void DisplayCredentialDetails(Credential credential)
            {
                Console.WriteLine("Credential Details:");
                Console.WriteLine("-------------------");
                Console.WriteLine($"Name            : {credential.Name}");
                Console.WriteLine($"Type            : {credential.Format.GetType().Name}");
                Console.WriteLine($"Data            : {credential.Format.UniqueId}");
                Console.WriteLine();
            }
        }
    }
}